using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq.Extensions;
using System.Data.SqlClient;
using System.Collections;
using System.Linq;

namespace System.Data.SqlDatabase
{
    /// <summary>
    /// Represents a SQL database.
    /// </summary>
    public class SqlDatabase : IDisposable
    {
        #region Delegates

        /// <summary>
        /// Represents the method(s) that will be called when a <see cref="SqlDatabase"/> table is created.
        /// </summary>
        public delegate void TableCreatedEventHandler(object sender, TableCreatedEventArgs e);

        /// <summary>
        /// Represents the method(s) that will be called when a <see cref="SqlDatabase"/> table is dropped.
        /// </summary>
        public delegate void TableDroppedEventHandler(object sender, TableDroppedEventArgs e);

        /// <summary>
        /// Represents the method(s) that will be called when a <see cref="SqlDatabase"/> table is selected.
        /// </summary>
        public delegate void TableSelectedEventHandler(object sender, TableSelectedEventArgs e);

        /// <summary>
        /// Represents the method(s) that will be called when one or multiple rows are added to a <see cref="SqlDatabase"/> table.
        /// </summary>
        public delegate void RowsAddedEventHandler(object sender, RowsAddedEventArgs e);

        /// <summary>
        /// Represents the method(s) that will be called when one or multiple rows are removed from a <see cref="SqlDatabase"/> table.
        /// </summary>
        public delegate void RowsRemovedEventHandler(object sender, RowsRemovedEventArgs e);

        /// <summary>
        /// Represents the method(s) that will be called when one or multiple rows are altered.
        /// </summary>
        public delegate void RowsAlteredEventHandler(object sender, RowsAlteredEventArgs e);

        /// <summary>
        /// Represents the method(s) that will be called when one or multiple columns are added to a <see cref="SqlDatabase"/> table.
        /// </summary>
        public delegate void ColumnsAddedEventHandler(object sender, ColumnsAddedEventArgs e);

        /// <summary>
        /// Represents the method(s) that will be called when one or multiple columns are removed from a <see cref="SqlDatabase"/> table.
        /// </summary>
        public delegate void ColumnsRemovedEventHandler(object sender, ColumnsRemovedEventArgs e);

        /// <summary>
        /// Represents the method(s) that will be called when one or multiple columns are altered.
        /// </summary>
        public delegate void ColumnsAlteredEventHandler(object sender, ColumnsAlteredEventArgs e);

        /// <summary>
        /// Represents the method(s) that will be called when the primary key of a <see cref="SqlDatabase"/> table is changed.
        /// </summary>
        public delegate void PrimaryKeyChangedEventHandler(object sender, PrimaryKeyChangedEventArgs e);

        /// <summary>
        /// Represents the method(s) that will be called when the primary key of a <see cref="SqlDatabase"/> table is removed.
        /// </summary>
        public delegate void PrimaryKeyRemovedEventHandler(object sender, PrimaryKeyRemovedEventArgs e);

        /// <summary>
        /// Represents the method(s) that will be called when an operation fails.
        /// </summary>
        public delegate void OperationFailedEventHandler(object sender, OperationFailedEventArgs e);

        #endregion

        #region Properties

        /// <summary>
        /// The connection string of the database.
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// The number of tables in the database.
        /// </summary>
        public int TableCount
        {
            get => tables.Count;
        }

        /// <summary>
        /// The name of the currently selected table.
        /// </summary>
        public string SelectedTable 
        {
            get => selectTable;
        }

        /// <summary>
        /// If <see langword="true"/>, the TableSelected event will be invoked regardless if selecting a table won't actually change the selected table.
        /// </summary>
        public bool EnableSameSelection { get; set; }

        /// <summary>
        /// The number of columns in the selected table.
        /// </summary>
        public int ColumnCount 
        {
            get
            {
                //Check if any table is selected
                if (string.IsNullOrEmpty(SelectedTable)) throw new InvalidOperationException("No table is selected.");

                return currentCols.Length;
            }
        }

        /// <summary>
        /// An array of strings which represent the current tables in the database.
        /// </summary>
        public string[] Tables 
        {
            get => tables.ToArray();
        }

        #endregion

        #region Events

        /// <summary>
        /// Invoked when a new table is created.
        /// </summary>
        public event TableCreatedEventHandler TableCreated;

        /// <summary>
        /// Invoked when the selected table is dropped.
        /// </summary>
        public event TableDroppedEventHandler TableDropped;

        /// <summary>
        /// Invoked when a new table is selected.
        /// <para>
        /// If the same table is selected, this event won't invoke, unless specified to do so by the EnableSameSelection property.
        /// </para>
        /// </summary>
        public event TableSelectedEventHandler TableSelected;

        /// <summary>
        /// Invoked when one or multiple rows are added to the selected table.
        /// </summary>
        public event RowsAddedEventHandler RowsAdded;

        /// <summary>
        /// Invoked when one or multiple rows are removed from the selected table.
        /// </summary>
        public event RowsRemovedEventHandler RowsRemoved;

        /// <summary>
        /// Invoked when one or multiple rows are altered.
        /// </summary>
        public event RowsAlteredEventHandler RowsAltered;

        /// <summary>
        /// Invoked when one or multiple columns are added to the selected table.
        /// </summary>
        public event ColumnsAddedEventHandler ColumnsAdded;

        /// <summary>
        /// Invoked when one or multiple columns are removed from the selected table.
        /// </summary>
        public event ColumnsRemovedEventHandler ColumnsRemoved;

        /// <summary>
        /// Invoked when one or multiple columns are altered.
        /// </summary>
        public event ColumnsAlteredEventHandler ColumnsAltered;

        /// <summary>
        /// Invoked when the primary key of the selected table is changed.
        /// </summary>
        public event PrimaryKeyChangedEventHandler PrimaryKeyChanged;

        /// <summary>
        /// Invoked when the primary key of the selected table is removed.
        /// </summary>
        public event PrimaryKeyRemovedEventHandler PrimaryKeyRemoved;

        /// <summary>
        /// Invoked when an operation fails.
        /// </summary>
        public event OperationFailedEventHandler OperationFailed;

        #endregion

        #region Indexer

        /// <summary>
        /// Gets the name of the table at <paramref name="index"/>.
        /// </summary>
        public string this[int index]
        {
            get
            {
                return tables[index];
            }

            set
            {
                try
                {
                    //Actually rename
                    Rename(tables[index], value, true);

                    //Update the collections
                    hashNames.Remove(tables[index]);
                    hashNames.Add(value);
                    tables[index] = value;
                }
                catch (Exception ex)
                {
                    Fail(ex);
                }
            }
        }

        #endregion

        #region Fields

        SqlConnection connection;
        SqlCommand command;

        HashSet<string> hashNames;
        List<string> tables;
        string selectTable = null;

        SqlDbColumn[] currentCols;
        Dictionary<string, SqlDbColumn> colDict;

        static ExclusiveString stringRand = new ExclusiveString(true);

        #endregion

        /// <summary>
        /// Creates a new instance of the <see cref="SqlDatabase"/> class from <paramref name="connectionString"/>.
        /// </summary>
        public SqlDatabase(string connectionString)
        {
            //Assignments
            ConnectionString = connectionString;
            connection = new SqlConnection(connectionString);
            command = connection.CreateCommand();

            //Get table names
            if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
            tables = connection.GetSchema("Tables").Rows.Cast<DataRow>().Select(x => x[2].ToString()).ToList();
            hashNames = tables.ToHashSet();
            connection.Close();
        }

        #region SelectTable Method

        /// <summary>
        /// Selects the table at <paramref name="index"/> in the database.
        /// <para>
        /// All operations will be done on selected table.
        /// </para>
        /// </summary>
        public void SelectTable(int index)
        {
            //Check if the table should actually change to not get the columns again for no reason
            bool changed = selectTable != tables[index];

            //Change the table name
            selectTable = tables[index];
            if (changed) GetColumns(true);

            //Invoke event if necessary
            if (changed || EnableSameSelection) TableSelected?.Invoke(this, new TableSelectedEventArgs(selectTable, currentCols, colDict));
        }

        /// <summary>
        /// Selectes the table with name <paramref name="name"/>.
        /// <para>
        /// All operations will be done on selected table.
        /// </para>
        /// </summary>
        public void SelectTable(string name)
        {
            //Check if name is actually a table
            if (!hashNames.Contains(name)) throw new ArgumentException($"No table with name {name} exists.");

            //Check if the table should actually change to not get the columns again for no reason
            bool changed = selectTable != name;

            //Change the table name
            selectTable = name;
            if (changed) GetColumns(true);

            //Invoke event if necessary
            if (changed || EnableSameSelection) TableSelected?.Invoke(this, new TableSelectedEventArgs(selectTable, currentCols, colDict));
        }

        #endregion

        #region AddRow Method

        /// <summary>
        /// Adds a row to the selected table with the specified values. The values will be inserted in all the table's columns.
        /// </summary>
        public void AddRow(params object[] values)
        {
            try
            {
                AddRowInternal(true, values);

                //Invoke event
                RowsAdded?.Invoke(this, new RowsAddedEventArgs(SelectedTable, values.Zip(currentCols, (v, n) => (n.Name, v)).ToDictionary(x => x.Name, x => x.v)));
            }
            catch (Exception ex)
            {
                Fail(ex);
            }
        }

        /// <summary>
        /// Adds a row to the selected table with the specified values and column names.
        /// </summary>
        public void AddRow(Dictionary<string, object> values)
        {
            try
            {
                AddRowInternal(true, values);

                //Invoke event
                RowsAdded?.Invoke(this, new RowsAddedEventArgs(SelectedTable, values));
            }
            catch (Exception ex)
            {
                Fail(ex);
            }
        }

        #endregion

        #region AddRows Method

        /// <summary>
        /// Adds an array of rows to the selected table with the specified values. The values will be inserted in all the table's columns.
        /// </summary>
        public void AddRows(params object[][] values)
        {
            try
            {
                //Begin transaction, if any addition throws, cancel all
                command.CommandText = "BEGIN TRAN [AddRowsTran];";
                
                //Execute and don't close to keep the transaction active
                if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
                command.ExecuteNonQuery();

                for (int i = 0; i < values.Length; i++) AddRowInternal(false, values[i]);

                //Commit the transaction since all no exceptions were thrown
                command.CommandText = "COMMIT TRAN [AddRowsTran];";
                command.ExecuteNonQuery();
                connection.Close();

                //Invoke event
                RowsAdded?.Invoke(this, new RowsAddedEventArgs(SelectedTable, values.Select(x => x.Zip(currentCols, (v, n) => (n.Name, v)).ToDictionary(k => k.Name, v => v.v)).ToArray()));
            }
            catch (Exception ex)
            {
                //In case of an exception, rollback
                command.CommandText = "ROLLBACK TRAN [AddRowsTran];";
                command.ExecuteNonQuery();
                connection.Close();

                Fail(ex);
            }
        }

        /// <summary>
        /// Adds an array of rows to the selected table with the specified values and column names.
        /// </summary>
        public void AddRows(params Dictionary<string, object>[] values)
        {
            try
            {
                //Begin transaction, if any addition throws, cancel all
                command.CommandText = "BEGIN TRAN [AddRowsTranDict];";

                //Execute and don't close to keep the transaction active
                if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
                command.ExecuteNonQuery();

                for (int i = 0; i < values.Length; i++) AddRowInternal(false, values[i]);

                //Commit the transaction since all no exceptions were thrown
                command.CommandText = "COMMIT TRAN [AddRowsTranDict];";
                command.ExecuteNonQuery();
                connection.Close();

                //Invoke event
                RowsAdded?.Invoke(this, new RowsAddedEventArgs(SelectedTable, values));
            }
            catch (Exception ex)
            {
                //In case of an exception, rollback
                command.CommandText = "ROLLBACK TRAN [AddRowsTranDict];";
                command.ExecuteNonQuery();
                connection.Close();

                Fail(ex);
            }
        }

        #endregion

        #region AddColumn Method

        /// <summary>
        /// Tries to add an identity column to the selected table with the specified seed and increment values.
        /// <para>
        /// If the identity is successfully set, this method returns <see langword="true"/>, otherwise returns <see langword="false"/>.
        /// </para>
        /// </summary>
        public bool AddColumn(SqlDbColumn column, object identitySeed, object identityIncr)
        {
            bool hasIdent = HasIdentity();

            try
            {
                //Check if any table is selected
                if (string.IsNullOrEmpty(SelectedTable)) throw new InvalidOperationException("No table is selected.");

                //Set the command
                command.CommandText = "ALTER TABLE [" + SelectedTable + "] ADD [" + column.Name + "] " + column.Type.ToString().ToLower() + (column.Data.Length.HasValue ? "(" + column.Data.Length + ")" : "") + (hasIdent ? "" : " IDENTITY(" + identitySeed + ", " + identityIncr + ")") + (column.Data.CanBeNull ? "" : " NOT") + " NULL" + ";";

                //Execute
                if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
                command.ExecuteNonQuery();

                //Any alteration of any column should refresh the current columns
                GetColumns(true);

                //Invoke event
                ColumnsAdded?.Invoke(this, new ColumnsAddedEventArgs(SelectedTable, column));
            }
            catch (Exception ex)
            {
                Fail(ex);
            }

            return !hasIdent;
        }

        /// <summary>
        /// Adds a column to the selected table.
        /// </summary>
        public void AddColumn(SqlDbColumn column)
        {
            try
            {
                AddColumnInternal(column, true);

                //Invoke event
                ColumnsAdded?.Invoke(this, new ColumnsAddedEventArgs(SelectedTable, column));
            }
            catch (Exception ex)
            {
                Fail(ex);
            }
        }

        #endregion

        /// <summary>
        /// Adds a column to the selected table.
        /// </summary>
        public void AddColumns(params SqlDbColumn[] columns)
        {
            try
            {
                //Begin transaction, if any addition throws, cancel all
                command.CommandText = "BEGIN TRAN [AddColumnsTran];";

                //Execute and don't close to keep the transaction active
                if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
                command.ExecuteNonQuery();

                //Update on the last addition
                for (int i = 0; i < columns.Length; i++) AddColumnInternal(columns[i], i == columns.Length - 1);

                //Commit the transaction since all no exceptions were thrown
                command.CommandText = "COMMIT TRAN [AddColumnsTran];";
                command.ExecuteNonQuery();
                connection.Close();

                //Invoke
                ColumnsAdded?.Invoke(this, new ColumnsAddedEventArgs(SelectedTable, columns));
            }
            catch (Exception ex)
            {
                //In case of an exception, rollback and get the columns
                command.CommandText = "ROLLBACK TRAN [AddColumnsTran];";
                command.ExecuteNonQuery();
                GetColumns(true);

                Fail(ex);
            }
        }

        /// <summary>
        /// Removes any rows from the selected table which satisfy the where clause.
        /// <para>
        /// <paramref name="whereClause"/> does not need to begin with the where keyword.  If <paramref name="whereClause"/> is <see langword="null"/> or empty, all rows will be removed.
        /// </para>
        /// </summary>
        public void RemoveRows(string whereClause)
        {
            try
            {
                //Check if any table is selected
                if (string.IsNullOrEmpty(SelectedTable)) throw new InvalidOperationException("No table is selected.");

                //Set the command
                command.CommandText = "DELETE FROM [" + SelectedTable + (string.IsNullOrEmpty(whereClause) ? "]" : "] WHERE " + Regex.Replace(whereClause, @"^where |(?<!\[)(;|--|\/\*|\*\/)(?!\])", "", RegexOptions.IgnoreCase)) + ";";

                //Execute
                if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
                int rows = command.ExecuteNonQuery();
                connection.Close();

                //Invoke event
                RowsRemoved?.Invoke(this, new RowsRemovedEventArgs(SelectedTable, rows));
            }
            catch (Exception ex)
            {
                Fail(ex);
            }
        }

        #region RemoveColumn Method

        /// <summary>
        /// Removes the column named <paramref name="columnName"/> from the selected table.
        /// </summary>
        public void RemoveColumn(string columnName)
        {
            try
            {

                //Check if any table is selected
                if (string.IsNullOrEmpty(SelectedTable)) throw new InvalidOperationException("No table is selected.");

                //Check if column exists
                if (!colDict.ContainsKey(columnName)) throw new ArgumentException($"No column with name {columnName} exists in the table {SelectedTable}.");

                //Set the command
                command.CommandText = "ALTER TABLE [" + SelectedTable + "] DROP COLUMN [" + columnName + "];";

                //Execute
                if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
                command.ExecuteNonQuery();

                //Any alteration of any column should refresh the current columns
                var column = colDict[columnName];
                GetColumns(true);

                //Invoke event
                ColumnsRemoved?.Invoke(this, new ColumnsRemovedEventArgs(SelectedTable, column));
            }
            catch (Exception ex)
            {
                Fail(ex);
            }
        }

        /// <summary>
        /// Removes the column in the selected table at the specified index.
        /// </summary>
        public void RemoveColumn(int index)
        {
            try
            {
                //Check if any table is selected
                if (string.IsNullOrEmpty(SelectedTable)) throw new InvalidOperationException("No table is selected.");

                //Set the command
                command.CommandText = "ALTER TABLE [" + SelectedTable + "] DROP COLUMN [" + currentCols[index].Name + "];";

                //Execute
                if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
                command.ExecuteNonQuery();

                //Any alteration of any column should refresh the current columns
                var column = currentCols[index];
                GetColumns(true);

                //Invoke event
                ColumnsRemoved?.Invoke(this, new ColumnsRemovedEventArgs(SelectedTable, column));
            }
            catch (Exception ex)
            {
                Fail(ex);
            }
        }

        /// <summary>
        /// Removes all columns in the selected table whose types are equivalent to <paramref name="type"/>.
        /// </summary>
        public void RemoveColumns(SqlDbType type)
        {
            try
            {
                //Check if any table is selected
                if (string.IsNullOrEmpty(SelectedTable)) throw new InvalidOperationException("No table is selected.");

                //Declarations
                int length = currentCols.Length;
                List<SqlDbColumn> cols = new List<SqlDbColumn>(currentCols.Length);

                //Begin transaction, if any addition throws, cancel all
                command.CommandText = "BEGIN TRAN [AddColumnsTran];";

                //Execute and don't close to keep the transaction active
                if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
                command.ExecuteNonQuery();

                for (int i = 0; i < length; i++)
                {
                    if (currentCols[i].Type == type)
                    {
                        cols.Add(currentCols[i]);

                        //Set and execute the command
                        command.CommandText = "ALTER TABLE [" + SelectedTable + "] DROP COLUMN [" + currentCols[i].Name + "];";
                        command.ExecuteNonQuery();
                    }
                }

                //Commit the transaction since all no exceptions were thrown
                command.CommandText = "COMMIT TRAN [AddColumnsTran];";
                command.ExecuteNonQuery();

                //Any alteration of any column should refresh the current columns
                GetColumns(true);

                //Invoke event
                ColumnsRemoved?.Invoke(this, new ColumnsRemovedEventArgs(SelectedTable, cols.ToArray()));
            }
            catch (Exception ex)
            {
                //In case of an exception, rollback and get the columns
                command.CommandText = "ROLLBACK TRAN [AddColumnsTran];";
                command.ExecuteNonQuery();
                GetColumns(true);

                Fail(ex);
            }
        }

        #endregion

        #region AlterRows Method

        /// <summary>
        /// Changes the values of one or multiple rows in the selected table based on a where clause.
        /// <para>
        /// If <paramref name="whereClause"/> is <see langword="null"/> or empty, all rows will be changed.
        /// </para>
        /// </summary>
        /// <exception cref="ArgumentException">The length of <paramref name="values"/> does not correspond to the number of columns in the selected table.</exception>
        public void AlterRows(string whereClause, params object[] values)
        {
            try
            {
                //Check if any table is selected
                if (string.IsNullOrEmpty(SelectedTable)) throw new InvalidOperationException("No table is selected.");

                //Check if the length of the values corresponds to the nuber of columns
                if (values.Length != currentCols.Length - Convert.ToInt32(HasIdentity())) throw new ArgumentException("The length of the values cannot be longer than the number of columns.", "values");

                //Set the command
                command.CommandText = "UPDATE [" + SelectedTable + "] SET " + string.Join(", ", values.Select((x, i) => "[" + currentCols[i].Name + "] = @value" + i)) + (string.IsNullOrEmpty(whereClause) ? "" : " WHERE " + Regex.Replace(whereClause, @"^where |(?<!\[)(;|--|\/\*|\*\/)(?!\])", "", RegexOptions.IgnoreCase)) + ";";

                //Set the parameters
                for (int i = 0; i < values.Length; i++) command.Parameters.AddWithValue("@value" + i, values[i]);

                //Execute
                if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
                command.ExecuteNonQuery();
                connection.Close();

                //Clear parameters
                command.Parameters.Clear();

                //Invoke event
                RowsAltered?.Invoke(this, new RowsAlteredEventArgs(SelectedTable, currentCols.Zip(values, (n, v) => (n.Name, v)).ToDictionary(x => x.Name, x => x.v)));

            }
            catch (Exception ex)
            {
                //In case of an exception, rollback and get the columns
                command.CommandText = "ROLLBACK TRAN [AddColumnsTran];";
                command.ExecuteNonQuery();
                GetColumns(true);

                OperationFailed?.Invoke(this, new OperationFailedEventArgs(ex));
            }
        }

        /// <summary>
        /// Changes the values of one or multiple rows in the selected table based on a where clause and the specified columns.
        /// <para>
        /// If <paramref name="whereClause"/> is <see langword="null"/> or empty, all rows will be changed.
        /// </para>
        /// </summary>
        public void AlterRows(string whereClause, Dictionary<string, object> values)
        {
            try
            {
                //Check if any table is selected
                if (string.IsNullOrEmpty(SelectedTable)) throw new InvalidOperationException("No table is selected.");

                //Set the command
                command.CommandText = "UPDATE [" + SelectedTable + "] SET " + string.Join(", ", values.Select((x, i) => "[" + x.Key + "] = @value" + i)) + (string.IsNullOrEmpty(whereClause) ? "" : " WHERE " + Regex.Replace(whereClause, @"^where |(?<!\[)(;|--|\/\*|\*\/)(?!\])", "", RegexOptions.IgnoreCase)) + ";";

                //Set the parameters
                var valueArr = values.Select(x => x.Value).ToArray();
                for (int i = 0; i < values.Count; i++) command.Parameters.AddWithValue("@value" + i, valueArr[i]);

                //Execute
                if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
                command.ExecuteNonQuery();
                connection.Close();

                //Clear parameters
                command.Parameters.Clear();

                //Invoke event
                RowsAltered?.Invoke(this, new RowsAlteredEventArgs(SelectedTable, values));
            }
            catch (Exception ex)
            {
                Fail(ex);
            }
        }

        #endregion

        #region AlterColumn Method

        /// <summary>
        /// Changes the column named <paramref name="columnName"/> from the selected table to <paramref name="newCol"/>.
        /// </summary>
        public void AlterColumn(string columnName, SqlDbColumn newCol)
        {
            try
            {
                //Check if any table is selected
                if (string.IsNullOrEmpty(SelectedTable)) throw new InvalidOperationException("No table is selected.");

                //Check if column exists
                if (!colDict.ContainsKey(columnName)) throw new ArgumentException($"No column with name {columnName} exists in the table {SelectedTable}.");

                //Rename if necessary
                var column = colDict[columnName];
                if (columnName != newCol.Name && !(newCol.Name is null)) RenameColumn(columnName, columnName = newCol.Name, false);

                //Set the command
                command.CommandText = "ALTER TABLE [" + SelectedTable + "] ALTER COLUMN [" + columnName + "] " + newCol.Type.ToString().ToLower() + (newCol.Data.Length.HasValue ? "(" + newCol.Data.Length + (newCol.Data.Scale.HasValue ? ", " + newCol.Data.Scale.Value : "") + ")" : "") + (newCol.Data.CanBeNull ? "" : " NOT") + " NULL" + ";";

                //Execute
                if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
                command.ExecuteNonQuery();

                //Any alteration of any column should refresh the current columns
                GetColumns(true);

                //Invoke event
                ColumnsAltered?.Invoke(this, new ColumnsAlteredEventArgs(SelectedTable, newCol, column));
            }
            catch (Exception ex)
            {
                Fail(ex);
            }
        }

        /// <summary>
        /// Changes the column in the selected table at the specified index to <paramref name="newCol"/>.
        /// </summary>
        public void AlterColumn(int index, SqlDbColumn newCol)
        {
            try
            {
                //Check if any table is selected
                if (string.IsNullOrEmpty(SelectedTable)) throw new InvalidOperationException("No table is selected.");

                //Rename if necessary
                var column = currentCols[index];
                var colName = currentCols[index].Name;
                if (currentCols[index].Name != newCol.Name && !(newCol.Name is null)) RenameColumn(colName, colName = newCol.Name, false);

                //Set the command
                command.CommandText = "ALTER TABLE [" + SelectedTable + "] ALTER COLUMN [" + colName + "] " + newCol.Type.ToString().ToLower() + (newCol.Data.Length.HasValue ? "(" + newCol.Data.Length + (newCol.Data.Scale.HasValue ? ", " + newCol.Data.Scale.Value : "") + ")" : "") + (newCol.Data.CanBeNull ? "" : " NOT") + " NULL" + ";";

                //Execute
                if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
                command.ExecuteNonQuery();

                //Any alteration of any column should refresh the current columns
                GetColumns(true);

                //Invoke event
                ColumnsAltered?.Invoke(this, new ColumnsAlteredEventArgs(SelectedTable, newCol, column));
            }
            catch (Exception ex)
            {
                Fail(ex);
            }
        }

        /// <summary>
        /// Changes all columns in the selected table whose types are equivalent to <paramref name="type"/> to <paramref name="newCol"/>.
        /// </summary>
        public void AlterColumns(SqlDbType type, SqlDbColumn newCol)
        {
            try
            {
                //Check if any table is selected
                if (string.IsNullOrEmpty(SelectedTable)) throw new InvalidOperationException("No table is selected.");

                //Declarations
                var cols = new List<SqlDbColumn>(currentCols.Length);

                //Begin transaction, if any addition throws, cancel all
                command.CommandText = "BEGIN TRAN [AddColumnsTran];";

                //Execute and don't close to keep the transaction active
                if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
                command.ExecuteNonQuery();

                for (int i = 0; i < currentCols.Length; i++)
                {
                    if (currentCols[i].Type == type)
                    {
                        cols.Add(currentCols[i]);

                        //Rename if necessary
                        var colName = currentCols[i].Name;
                        if (currentCols[i].Name != newCol.Name && !(newCol.Name is null)) RenameColumn(colName, colName = newCol.Name, false);

                        //Set the command
                        command.CommandText = "ALTER TABLE [" + SelectedTable + "] ALTER COLUMN [" + colName + "] " + newCol.Type.ToString().ToLower() + (newCol.Data.Length.HasValue ? "(" + newCol.Data.Length + (newCol.Data.Scale.HasValue ? ", " + newCol.Data.Scale.Value : "") + ")" : "") + (newCol.Data.CanBeNull ? "" : " NOT") + " NULL" + ";";

                        command.ExecuteNonQuery();
                    }
                }

                //Commit the transaction since all no exceptions were thrown
                command.CommandText = "COMMIT TRAN [AddColumnsTran];";
                command.ExecuteNonQuery();

                //Any alteration of any column should refresh the current columns
                GetColumns(true);

                //Invoke event
                ColumnsAltered?.Invoke(this, new ColumnsAlteredEventArgs(SelectedTable, newCol, cols.ToArray()));
            }
            catch (Exception ex)
            {
                //In case of an exception, rollback and get the columns
                command.CommandText = "ROLLBACK TRAN [AddColumnsTran];";
                command.ExecuteNonQuery();
                GetColumns(true);

                OperationFailed?.Invoke(this, new OperationFailedEventArgs(ex));
            }
        }

        #endregion

        #region GetRows Method

        /// <summary>
        /// Gets all rows in the selected table which satisfy the where clause.
        /// <para>
        /// <paramref name="whereClause"/> does not need to begin with the where keyword. If <paramref name="whereClause"/> is <see langword="null"/> or empty, all rows will be retrieved.
        /// </para>
        /// </summary>
        public object[][] GetRows(string whereClause)
        {
            try
            {
                //Check if any table is selected
                if (string.IsNullOrEmpty(SelectedTable)) throw new InvalidOperationException("No table is selected.");

                //Set the command
                command.CommandText = "SELECT * FROM [" + SelectedTable + (string.IsNullOrEmpty(whereClause) ? "]" : "] WHERE " + Regex.Replace(whereClause, @"^where |(?<!\[)(;|--|\/\*|\*\/)(?!\])", "", RegexOptions.IgnoreCase)) + ";";

                //Execute
                if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
                var reader = command.ExecuteReader();

                //Get all entries
                var entries = new List<object[]>();
                while (reader.Read())
                {
                    var row = new object[currentCols.Length];
                    reader.GetValues(row);
                    entries.Add(row);
                }
                connection.Close();

                return entries.ToArray();
            }
            catch (Exception ex)
            {
                Fail(ex);
            }

            return null;
        }

        /// <summary>
        /// Gets the elements which belong to <paramref name="colNames"/> from all rows in the selected table which satisfy the where clause.
        /// <para>
        /// <paramref name="whereClause"/> does not need to begin with the where keyword. If <paramref name="whereClause"/> is <see langword="null"/> or empty, all rows will be retrieved.
        /// </para>
        /// </summary>
        public object[][] GetRows(string whereClause, params string[] colNames)
        {
            try
            {
                //Check if any table is selected
                if (string.IsNullOrEmpty(SelectedTable)) throw new InvalidOperationException("No table is selected.");

                //Set the command
                command.CommandText = "SELECT " + Regex.Replace("[" + string.Join("], [", colNames) + "]", @"^\[\]$", "*") + " FROM [" + SelectedTable + (string.IsNullOrEmpty(whereClause) ? "]" : "] WHERE " + Regex.Replace(whereClause, @"^where |(?<!\[)(;|--|\/\*|\*\/)(?!\])", "", RegexOptions.IgnoreCase)) + ";";

                //Execute
                if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
                var reader = command.ExecuteReader();

                //Get all entries
                var entries = new List<object[]>();
                while (reader.Read())
                {
                    var row = new object[colNames.Length == 0 ? currentCols.Length : colNames.Length];
                    reader.GetValues(row);
                    entries.Add(row);
                }
                connection.Close();

                return entries.ToArray();
            }
            catch (Exception ex)
            {
                Fail(ex);
            }

            return null;
        }

        /// <summary>
        /// Gets the elements at <paramref name="indexes"/> from all rows in the selected table which satisfy the where clause.
        /// <para>
        /// <paramref name="whereClause"/> does not need to begin with the where keyword. If <paramref name="whereClause"/> is <see langword="null"/> or empty, all rows will be retrieved.
        /// </para>
        /// </summary>
        public object[][] GetRows(string whereClause, params int[] indexes)
        {
            try
            {
                //Check if any table is selected
                if (string.IsNullOrEmpty(SelectedTable)) throw new InvalidOperationException("No table is selected.");

                //Set the command
                command.CommandText = "SELECT " + Regex.Replace("[" + string.Join("], [", indexes.Select(x => currentCols[x].Name)) + "]", @"^\[\]$", "*") + " FROM [" + SelectedTable + (string.IsNullOrEmpty(whereClause) ? "]" : "] WHERE " + Regex.Replace(whereClause, @"^where |(?<!\[)(;|--|\/\*|\*\/)(?!\])", "", RegexOptions.IgnoreCase)) + ";";

                //Execute
                if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
                var reader = command.ExecuteReader();

                //Get all entries
                var entries = new List<object[]>();
                while (reader.Read())
                {
                    var row = new object[indexes.Length == 0 ? currentCols.Length : indexes.Length];
                    reader.GetValues(row);
                    entries.Add(row);
                }
                connection.Close();

                return entries.ToArray();
            }
            catch (Exception ex)
            {
                Fail(ex);
            }

            return null;
        }

        #endregion

        #region GetRowAt Method

        /// <summary>
        /// Gets the elements at <paramref name="colIndexes"/> in the row at <paramref name="rowIndex"/> in the selected table.
        /// <para>
        ///  If <paramref name="colIndexes"/> is <see langword="null"/> or empty, all elements in the row will be retrieved.
        /// </para>
        /// </summary>
        public object[] GetRowAt(int rowIndex, params int[] colIndexes)
        {
            try
            {
                //Check if any table is selected
                if (string.IsNullOrEmpty(SelectedTable)) throw new InvalidOperationException("No table is selected.");

                //Set the command
                command.CommandText = "SELECT TOP " + (rowIndex + 1) + " " + Regex.Replace("[" + string.Join("], [", colIndexes.Select(x => currentCols[x].Name)) + "]", @"^\[\]$", "*") + " FROM [" + SelectedTable + "];";

                //Execute
                if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
                var reader = command.ExecuteReader();

                //Get the last entry
                for (int i = 0; i <= rowIndex; i++) reader.Read();

                //Create the final result
                var result = new object[colIndexes.Length == 0 ? currentCols.Length : colIndexes.Length];
                reader.GetValues(result);
                connection.Close();

                return result;
            }
            catch (Exception ex)
            {
                Fail(ex);
            }

            return null;
        }

        /// <summary>
        /// Gets the elements which belong to <paramref name="colNames"/> in the row at <paramref name="rowIndex"/> in the selected table.
        /// <para>
        ///  If <paramref name="colNames"/> is <see langword="null"/> or empty, all elements in the row will be retrieved.
        /// </para>
        /// </summary>
        public object[] GetRowAt(int rowIndex, params string[] colNames)
        {
            try
            {
                //Check if any table is selected
                if (string.IsNullOrEmpty(SelectedTable)) throw new InvalidOperationException("No table is selected.");

                //Set the command
                command.CommandText = "SELECT TOP " + (rowIndex + 1) + " " + Regex.Replace("[" + string.Join("], [", colNames) + "]", @"^\[\]$", "*") + " FROM [" + SelectedTable + "];";

                //Execute
                if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
                var reader = command.ExecuteReader();

                //Get the last entry
                for (int i = 0; i <= rowIndex; i++) reader.Read();

                //Create the final result
                var result = new object[colNames.Length == 0 ? currentCols.Length : colNames.Length];
                reader.GetValues(result);
                connection.Close();

                return result;
            }
            catch (Exception ex)
            {
                Fail(ex);
            }

            return null;
        }

        #endregion

        /// <summary>
        /// Gets the row at <paramref name="index"/> in the selected table.
        /// </summary>
        public object[] GetRow(int index) 
        {
            try
            {
                //Check if any table is selected
                if (string.IsNullOrEmpty(SelectedTable)) throw new InvalidOperationException("No table is selected.");

                //Set the command
                command.CommandText = "SELECT TOP " + (index + 1) + " * FROM [" + SelectedTable + "];";

                //Execute
                if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
                var reader = command.ExecuteReader();

                //Get the last entry
                for (int i = 0; i <= index; i++) reader.Read();

                //Create the final result
                var result = new object[currentCols.Length];
                reader.GetValues(result);
                connection.Close();

                return result;
            }
            catch (Exception ex)
            {
                Fail(ex);
            }

            return null;
        }

        #region AddTable Method

        /// <summary>
        /// Adds a table to this <see cref="SqlDatabase"/>.
        /// </summary>
        public void AddTable(string tableName, params SqlDbColumn[] columns)
        {
            try
            {
                //Set the command
                command.CommandText = "CREATE TABLE [" + tableName + "] (" + string.Join(", ", columns.Select(x => "[" + x.Name + "] " + x.Type.ToString().ToLower() + (x.Data.Length.HasValue ? "(" + x.Data.Length + (x.Data.Scale.HasValue ? ", " + x.Data.Scale.Value : "") + ")" : "") + (x.Data.CanBeNull ? " NULL" : " NOT NULL"))) + ");";

                //Execute
                if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
                command.ExecuteNonQuery();
                connection.Close();

                //Add the table to all collections
                tables.Add(tableName);
                hashNames.Add(tableName);

                //Invoke event
                TableCreated?.Invoke(this, new TableCreatedEventArgs(tableName, columns));
            }
            catch (Exception ex)
            {
                Fail(ex);
            }
        }

        /// <summary>
        /// Adds a table to this <see cref="SqlDatabase"/> with a primary key.
        /// </summary>
        public void AddTable(string tableName, int primaryIndex, params SqlDbColumn[] columns)
        {
            try
            {
                //Set the command
                command.CommandText = "CREATE TABLE [" + tableName + "] (" + string.Join(", ", columns.Select(x => "[" + x.Name + "] " + x.Type.ToString().ToLower() + (x.Data.Length.HasValue ? "(" + x.Data.Length + (x.Data.Scale.HasValue ? ", " + x.Data.Scale.Value : "") + ")" : "") + (x.Data.CanBeNull ? " NULL" : " NOT NULL")).Select((x, i) => x + (i == primaryIndex ? " PRIMARY KEY" : ""))) + ");";

                //Execute
                if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
                command.ExecuteNonQuery();
                connection.Close();

                //Add the table to all collections
                tables.Add(tableName);
                hashNames.Add(tableName);

                //Invoke event
                TableCreated?.Invoke(this, new TableCreatedEventArgs(tableName, columns));
            }
            catch (Exception ex)
            {
                Fail(ex);
            }
        }

        /// <summary>
        /// Adds a table to this <see cref="SqlDatabase"/> with an identity column.
        /// </summary>
        public void AddTable(string tableName, int identityIndex, int seed, int increment, params SqlDbColumn[] columns)
        {
            try
            {
                //Set the command
                command.CommandText = "CREATE TABLE [" + tableName + "] (" + string.Join(", ", columns.Select(x => "[" + x.Name + "] " + x.Type.ToString().ToLower() + (x.Data.Length.HasValue ? "(" + x.Data.Length + (x.Data.Scale.HasValue ? ", " + x.Data.Scale.Value : "") + ")" : "") + (x.Data.CanBeNull ? " NULL" : " NOT NULL")).Select((x, i) => x + (i == identityIndex ? " IDENTITY(" + seed + ", " + increment + ")" : ""))) + ");";

                //Execute
                if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
                command.ExecuteNonQuery();
                connection.Close();

                //Add the table to all collections
                tables.Add(tableName);
                hashNames.Add(tableName);

                //Invoke event
                TableCreated?.Invoke(this, new TableCreatedEventArgs(tableName, columns));
            }
            catch (Exception ex)
            {
                Fail(ex);
            }
        }

        /// <summary>
        /// Adds a table to this <see cref="SqlDatabase"/> with a primary key and identity column.
        /// </summary>
        public void AddTable(string tableName, int primaryIndex, int identityIndex, int seed, int increment, params SqlDbColumn[] columns)
        {
            try
            {
                //Set the command
                command.CommandText = "CREATE TABLE [" + tableName + "] (" + string.Join(", ", columns.Select(x => "[" + x.Name + "] " + x.Type.ToString().ToLower() + (x.Data.Length.HasValue ? "(" + x.Data.Length + (x.Data.Scale.HasValue ? ", " + x.Data.Scale.Value : "") + ")" : "") + (x.Data.CanBeNull ? " NULL" : " NOT NULL")).Select((x, i) => x + (i == primaryIndex ? " PRIMARY KEY" : "") + (i == identityIndex ? " IDENTITY(" + seed + ", " + increment + ")" : ""))) + ");";

                //Execute
                if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
                command.ExecuteNonQuery();
                connection.Close();

                //Add the table to all collections
                tables.Add(tableName);
                hashNames.Add(tableName);

                //Invoke event
                TableCreated?.Invoke(this, new TableCreatedEventArgs(tableName, columns));
            }
            catch (Exception ex)
            {
                Fail(ex);
            }
        }

        #endregion

        /// <summary>
        /// Removes all rows from the selected table.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// No table is selected.
        /// </exception>
        public void Truncate()
        {
            try
            {
                //Check if any table is selected
                if (string.IsNullOrEmpty(SelectedTable)) throw new InvalidOperationException("No table is selected.");

                //Set the command
                command.CommandText = "TRUNCATE TABLE [" + SelectedTable + "];";

                //Execute
                if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception ex)
            {
                Fail(ex);
            }
        }

        /// <summary>
        /// Removes all rows from the selected table, also resetting the table's identity to its seed if specified and if the table has an identity column.
        /// </summary>
        public void Truncate(bool resetIdentity)
        {
            try
            {
                //Check if any table is selected
                if (string.IsNullOrEmpty(SelectedTable)) throw new InvalidOperationException("No table is selected.");

                //Reset if needed
                if (resetIdentity) ResetIdentityInternal(GetSeedInternal(false), false);

                //Set the command
                command.CommandText = "TRUNCATE TABLE [" + SelectedTable + "];";

                //Execute
                if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception ex)
            {
                connection.Close();
                OperationFailed?.Invoke(this, new OperationFailedEventArgs(ex));
            }
        }

        /// <summary>
        /// Drops the selected table, setting the selected table to <see langword="null"/>.
        /// </summary>
        public void Drop()
        {
            try
            {
                //Check if any table is selected
                if (string.IsNullOrEmpty(SelectedTable)) throw new InvalidOperationException("No table is selected.");

                //Set the command
                command.CommandText = "DROP TABLE " + SelectedTable + ";";

                //Fill the argument table
                if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
                DataTable table = new DataTable();
                using (var adapter = new SqlDataAdapter("SELECT * FROM " + SelectedTable + ";", connection)) adapter.Fill(table);

                //Execute
                command.ExecuteNonQuery();
                connection.Close();

                //Reset data
                tables.Remove(selectTable);
                hashNames.Remove(selectTable);
                selectTable = null;
                currentCols = null;
                colDict = null;

                //Invoke event
                TableDropped?.Invoke(this, new TableDroppedEventArgs(table));
            }
            catch (Exception ex)
            {
                connection.Close();
                OperationFailed?.Invoke(this, new OperationFailedEventArgs(ex));
            }
        }

        /// <summary>
        /// Checks if the selected table has a primary key.
        /// </summary>
        public bool HasPrimaryKey()
        {
            try
            {
                //Check if any table is selected
                if (string.IsNullOrEmpty(SelectedTable)) throw new InvalidOperationException("No table is selected.");

                //Set the command
                command.CommandText = "SELECT OBJECTPROPERTY(OBJECT_ID('" + SelectedTable + "'), 'TableHasPrimaryKey');";

                //Execute
                if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
                bool result = Convert.ToBoolean(command.ExecuteScalar());
                connection.Close();

                return result;
            }
            catch (Exception ex)
            {
                Fail(ex);
            }

            return false;
        }

        /// <summary>
        /// Gets the name of the primary key of the selected table.
        /// <para>
        /// If the selected table has no primary key, this method returns <see langword="null"/>.
        /// </para>
        /// </summary>
        public string GetPrimaryKey()
        {
            try
            {
                //Check for primary key existance
                if (!HasPrimaryKey()) return null;

                //Set the command
                command.CommandText = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE WHERE CONSTRAINT_NAME = '" + GetPrimaryKeyConstraint(false) + "' AND TABLE_NAME = '" + SelectedTable + "';";

                //Execute
                if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
                string colName = (string)command.ExecuteScalar();
                connection.Close();

                return colName;
            }
            catch (Exception ex)
            {
                Fail(ex);
            }

            return null;
        }

        /// <summary>
        /// Removes the primary key from the selected table.
        /// <para>
        /// If the selected table has no primary key, nothing happens.
        /// </para>
        /// </summary>
        public void RemovePrimaryKey()
        {
            try
            {
                RemovePrimaryKeyInternal(true, true);
            }
            catch (Exception ex)
            {
                Fail(ex);
            }
        }

        #region SetPrimaryKey Method

        /// <summary>
        /// Sets the column at <paramref name="index"/> in the selected table as the primary key of the table.
        /// </summary>
        public void SetPrimaryKey(int index)
        {
            try
            {
                //Check if any table is selected
                if (string.IsNullOrEmpty(SelectedTable)) throw new InvalidOperationException("No table is selected.");

                //Check for primary key existance
                string oldKey = GetPrimaryKey();
                if (HasPrimaryKey()) RemovePrimaryKeyInternal(false, false);

                //Set the command
                command.CommandText = "ALTER TABLE [" + SelectedTable + "] ADD CONSTRAINT [PK_" + SelectedTable + stringRand.Next(5) + "] PRIMARY KEY (" + currentCols[index].Name + ");";

                //Execute
                if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
                command.ExecuteNonQuery();
                connection.Close();

                //Invoke event
                PrimaryKeyChanged?.Invoke(this, new PrimaryKeyChangedEventArgs(SelectedTable, currentCols[index], oldKey is null ? null : new SqlDbColumn?(colDict[oldKey])));
            }
            catch (Exception ex)
            {
                Fail(ex);
            }
        }

        /// <summary>
        /// Sets the column named <paramref name="columnName"/> in the selected table as the primary key of the table.
        /// </summary>
        public void SetPrimaryKey(string columnName)
        {
            try
            {
                //Check if any table is selected
                if (string.IsNullOrEmpty(SelectedTable)) throw new InvalidOperationException("No table is selected.");

                //Check if column exists
                if (!colDict.ContainsKey(columnName)) throw new ArgumentException($"No column with name {columnName} exists in the table {SelectedTable}.");

                //Check for primary key existance
                string oldKey = GetPrimaryKey();
                if (HasPrimaryKey()) RemovePrimaryKeyInternal(false, false);

                //Set the command
                command.CommandText = "ALTER TABLE [" + SelectedTable + "] ADD CONSTRAINT [PK_" + SelectedTable + stringRand.Next(5) + "] PRIMARY KEY (" + colDict[columnName].Name + ");";

                //Execute
                if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
                command.ExecuteNonQuery();
                connection.Close();

                //Invoke event
                PrimaryKeyChanged?.Invoke(this, new PrimaryKeyChangedEventArgs(SelectedTable, colDict[columnName], oldKey is null ? null : new SqlDbColumn?(colDict[oldKey])));
            }
            catch (Exception ex)
            {
                Fail(ex);
            }
        }

        #endregion

        /// <summary>
        /// Checks if the selected table has an identity column.
        /// </summary>
        public bool HasIdentity()
        {
            try
            {
                //Check if any table is selected
                if (string.IsNullOrEmpty(SelectedTable)) throw new InvalidOperationException("No table is selected.");

                //Set the command
                command.CommandText = "SELECT OBJECTPROPERTY(OBJECT_ID('" + SelectedTable + "'), 'TableHasIdentity');";

                //Execute
                if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
                bool result = Convert.ToBoolean(command.ExecuteScalar());
                connection.Close();

                return result;
            }
            catch (Exception ex)
            {
                Fail(ex);
            }

            return false;
        }

        /// <summary>
        /// Resets the identity column of the selected table to <paramref name="value"/>.
        /// <para>
        /// If the selected table has no identity column, nothing happens.
        /// </para>
        /// </summary>
        public void ResetIdentity(object value)
        {
            try
            {
                ResetIdentityInternal(value, true);
            }
            catch (Exception ex)
            {
                Fail(ex);
            }
        }

        /// <summary>
        /// Gets the current value of the identity column. Returns <see langword="null"/> if there is no identity column.
        /// </summary>
        public object GetCurrentIdentity()
        {
            try
            {
                return GetCurrentIdentityInternal(true);
            }
            catch (Exception ex)
            {
                Fail(ex);
            }

            return null;
        }

        /// <summary>
        /// Gets the seed value of the identity column of the selected table. Returns <see langword="null"/> if there is no identity column.
        /// </summary>
        public object GetIdentitySeed()
        {
            try
            {
                return GetSeedInternal(true);
            }
            catch (Exception ex)
            {
                Fail(ex);
            }

            return null;
        }

        /// <summary>
        /// Gets the increment value of the identity column of the selected table. Returns <see langword="null"/> if there is no identity column.
        /// </summary>
        public object GetIncrement()
        {
            try
            {
                return GetIncrementInternal(true);
            }
            catch (Exception ex)
            {
                Fail(ex);
            }

            return null;
        }

        /// <summary>
        /// Executes a query based on the execution type.
        /// <para>
        /// The type of the return value coincides with the execution type.
        /// </para>
        /// </summary>
        public object Execute(string query, SqlExecutionType execution)
        {
            try
            {
                //Set the command
                command.CommandText = query;

                //Execute
                if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
                object result = null;
                switch (execution)
                {
                    case SqlExecutionType.Scalar:
                        result = command.ExecuteScalar();
                        break;

                    case SqlExecutionType.Reader:
                        result = command.ExecuteReader();
                        break;

                    case SqlExecutionType.XmlReader:
                        result = command.ExecuteXmlReader();
                        break;

                    case SqlExecutionType.NonQuery:
                        result = command.ExecuteNonQuery();
                        break;
                }

                //Update
                Update(true);

                return result;
            }
            catch (Exception ex)
            {
                Fail(ex);
            }

            return null;
        }

        /// <summary>
        /// Releases all resources used by this <see cref="SqlDatabase"/>.
        /// </summary>
        public void Dispose()
        {
            command.Dispose();
            connection.Close();
            connection.Dispose();
        }

        #region Private Methods

        /// <summary>
        /// Gets all the columns in the selected table.
        /// </summary>
        void GetColumns(bool close)
        {
            //Check if any table is selected
            if (string.IsNullOrEmpty(SelectedTable)) throw new InvalidOperationException("No table is selected.");

            //Get the column schema for the selected table
            if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
            var schema = connection.GetSchema("Columns", new string[] { null, null, SelectedTable, null }).Rows;
            if (close) connection.Close();

            //Assign
            currentCols = new SqlDbColumn[schema.Count];
            for (int i = 0; i < schema.Count; i++)
            {
                currentCols[((int)schema[i][4]) - 1] = new SqlDbColumn(name: (string)schema[i][3],
                                                                       type: (SqlDbType)Enum.Parse(typeof(SqlDbType), (string)schema[i][7], true),
                                                                       data: new SqlDbTypeData(length: schema[i][8].GetType() == typeof(DBNull) ? null : new int?((int)schema[i][8]),
                                                                       scale: schema[i][12].GetType() == typeof(DBNull) || schema[i][12].Equals(0) ? null : new int?((int)schema[i][12]),
                                                                       canBeNull: (string)schema[i][6] == "YES"));
            }

            colDict = currentCols.ToDictionary(x => x.Name, x => x);
        }

        /// <summary>
        /// Renames a table.
        /// </summary>
        void Rename(string tableName, string newName, bool close)
        {
            //Set the command
            command.CommandText = "EXECUTE sp_rename '" + tableName + "', '" + newName + "';";

            //Execute
            if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
            command.ExecuteNonQuery();
            if (close) connection.Close();
        }

        /// <summary>
        /// Renames a column of the selected table.
        /// </summary>
        void RenameColumn(string name, string newName, bool close)
        {
            //Set the command
            command.CommandText = "EXECUTE sp_rename '" + SelectedTable + "." + name + "', '" + newName + "', 'COLUMN';";

            //Execute
            if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
            command.ExecuteNonQuery();
            if (close) connection.Close();
        } 

        /// <summary>
        /// Returns the primary key constraint name, or, if there is no such constraint, returns <see langword="null"/>.
        /// </summary>
        string GetPrimaryKeyConstraint(bool close)
        {
            //Check if any table is selected
            if (string.IsNullOrEmpty(SelectedTable)) throw new InvalidOperationException("No table is selected.");

            //Check for primary key existance
            if (!HasPrimaryKey()) return null;

            //Set the command
            command.CommandText = "SELECT CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_TYPE = 'PRIMARY KEY' AND TABLE_NAME = '" + SelectedTable + "';";

            //Execute
            if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
            string constraint = (string)command.ExecuteScalar();
            if (close) connection.Close();

            return constraint;
        }

        /// <summary>
        /// Removes the primary key from the selected table, specifying if the primary key removal event is to be invoked.
        /// </summary>
        void RemovePrimaryKeyInternal(bool invokeEvent, bool close)
        {
            //Check if any table is selected
            if (string.IsNullOrEmpty(SelectedTable)) throw new InvalidOperationException("No table is selected.");

            //Check for primary key existance
            if (!HasPrimaryKey()) return;
            string oldKey = GetPrimaryKey();

            //Set the command
            command.CommandText = "ALTER TABLE [" + SelectedTable + "] DROP CONSTRAINT " + GetPrimaryKeyConstraint(false);

            //Execute
            if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
            command.ExecuteNonQuery();
            if (close) connection.Close();

            //Invoke if specified
            if (invokeEvent) PrimaryKeyRemoved?.Invoke(this, new PrimaryKeyRemovedEventArgs(SelectedTable, colDict[oldKey]));
        }

        /// <summary>
        /// Resets the identity column of the selected table to <paramref name="value"/>.
        /// <para>
        /// If the selected table has no identity column, nothing happens.
        /// </para>
        /// </summary>
        void ResetIdentityInternal(object value, bool close)
        {
            //Check for identity existance
            if (!HasIdentity()) return;

            //Set the command
            command.CommandText = "DBCC CHECKIDENT('" + SelectedTable + "', RESEED, @value) WITH NO_INFOMSGS;";

            //Set the paramater
            command.Parameters.AddWithValue("@value", value);

            //Execute
            if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
            command.ExecuteNonQuery();
            if (close) connection.Close();
        }

        /// <summary>
        /// Gets the identity column's seed value from the selected table. Returns <see langword="null"/> is there is no identity column.
        /// </summary>
        object GetSeedInternal(bool close)
        {
            //Check if any table is selected
            if (string.IsNullOrEmpty(SelectedTable)) throw new InvalidOperationException("No table is selected.");

            //Set the command
            command.CommandText = "SELECT IDENT_SEED('" + SelectedTable + "');";

            //Execute
            if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
            object result = command.ExecuteScalar();
            if (close) connection.Close();

            return result;
        }

        /// <summary>
        /// Gets the increment value of the identity column of the selected table. Returns <see langword="null"/> if there is no identity column.
        /// </summary>
        object GetIncrementInternal(bool close)
        {
            //Check if any table is selected
            if (string.IsNullOrEmpty(SelectedTable)) throw new InvalidOperationException("No table is selected.");

            //Set the command
            command.CommandText = "SELECT IDENT_INCR('" + SelectedTable + "');";

            //Execute
            if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
            object result = command.ExecuteScalar();
            if (close) connection.Close();

            return result;
        }

        /// <summary>
        /// Gets the current value of the identity column. Returns <see langword="null"/> if there is no identity column.
        /// </summary>
        object GetCurrentIdentityInternal(bool close)
        {
            //Check if any table is selected
            if (string.IsNullOrEmpty(SelectedTable)) throw new InvalidOperationException("No table is selected.");

            //Set the command
            command.CommandText = "SELECT IDENT_CURRENT('" + SelectedTable + "');";

            //Execute
            if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
            object result = command.ExecuteScalar();
            if (close) connection.Close();

            return result;
        }

        /// <summary>
        /// Adds a column to the selected table.
        /// </summary>
        public void AddColumnInternal(SqlDbColumn column, bool update)
        {
            //Check if any table is selected
            if (string.IsNullOrEmpty(SelectedTable)) throw new InvalidOperationException("No table is selected.");

            //Set the command
            bool hasIdent = HasIdentity();
            command.CommandText = "ALTER TABLE [" + SelectedTable + "] ADD [" + column.Name + "] " + column.Type.ToString().ToLower() + (column.Data.Length.HasValue ? "(" + column.Data.Length + (column.Data.Scale.HasValue ? ", " + column.Data.Scale.Value : "") + ")" : "") + (hasIdent ? "" : " IDENTITY(@seed, @increment)") + (column.Data.CanBeNull ? "" : " NOT") + " NULL" + ";";

            //Execute
            if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
            command.ExecuteNonQuery();

            //Any alteration of any column should refresh the current columns
            if (update) GetColumns(true);
            command.Parameters.Clear();
        }

        /// <summary>
        /// Adds a row to the selected table with the specified values. The values will be inserted in all the table's columns.
        /// </summary>
        public void AddRowInternal(bool close, params object[] values)
        {
            //Check if any table is selected
            if (string.IsNullOrEmpty(SelectedTable)) throw new InvalidOperationException("No table is selected.");

            //Check for values length
            if (values.Length > currentCols.Length) throw new ArgumentException("The length of the values cannot be higher than the column count.", "values"); ;

            //Set the command
            command.CommandText = "INSERT INTO [" + SelectedTable + "] VALUES (" + string.Join(", ", Enumerable.Range(0, values.Length).Select(x => "@value" + x)) + ");";

            //Add the parameters
            for (int i = 0; i < values.Length; i++) command.Parameters.AddWithValue("@value" + i, values[i]);

            //Execute
            if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
            command.ExecuteNonQuery();
            if (close) connection.Close();

            //Clear parameters
            command.Parameters.Clear();
        }

        /// <summary>
        /// Adds a row to the selected table with the specified values and column names.
        /// </summary>
        public void AddRowInternal(bool close, Dictionary<string, object> values)
        {
            //Check if any table is selected and if all columns are present
            if (string.IsNullOrEmpty(SelectedTable)) throw new InvalidOperationException("No table is selected.");
            if (values.Where(x => !colDict.ContainsKey(x.Key)).Any()) throw new ArgumentException($"No column exists with the name {values.SkipWhile(x => colDict.ContainsKey(x.Key)).FirstOrDefault().Key}", "values");

            //Set the command
            command.CommandText = "INSERT INTO [" + SelectedTable + "] ([" + string.Join("], [", values.Select(x => x.Key)) + "]) VALUES (" + string.Join(", ", Enumerable.Range(0, values.Count).Select(x => "@value" + x)) + ");";

            //Add the parameters
            var valueArr = values.Select(x => x.Value).ToArray();
            for (int i = 0; i < values.Count; i++) command.Parameters.AddWithValue("@value" + i, valueArr[i]);

            //Execute
            if (connection.State.Equals(ConnectionState.Closed)) connection.Open();
            command.ExecuteNonQuery();
            if (close) connection.Close();

            //Clear parameters
            command.Parameters.Clear();
        }

        /// <summary>
        /// Marks a failed operation.
        /// </summary>
        public void Fail(Exception ex)
        {
            connection.Close();
            OperationFailed?.Invoke(this, new OperationFailedEventArgs(ex));
        }

        /// <summary>
        /// Updates this <see cref="SqlDatabase"/>.
        /// </summary>
        public void Update(bool close)
        {
            //Open connection
            if (connection.State.Equals(ConnectionState.Closed)) connection.Open();

            //Get tables
            tables = connection.GetSchema("Tables").Rows.Cast<DataRow>().Select(x => x[2].ToString()).ToList();
            hashNames = tables.ToHashSet();

            if (hashNames.Contains(SelectedTable))
            {
                //Get columns
                GetColumns(close);
                return;
            }

            //If the previously selected table doesn't exist anymore, set it to null
            selectTable = null;
            if (close) connection.Close();
        }

        #endregion
    }
}