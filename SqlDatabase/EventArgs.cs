using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace System.Data.SqlDatabase
{
    /// <summary>
    /// Represents data for a table which was created.
    /// </summary>
    public class TableCreatedEventArgs : EventArgs
    {
        /// <summary>
        /// The name of the new table.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The columns of the new table.
        /// </summary>
        public SqlDbColumn[] Columns { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="TableCreatedEventArgs"/> class from the table's columns.
        /// </summary>
        internal TableCreatedEventArgs(string name, SqlDbColumn[] cols)
        {
            (Name, Columns) = (name, cols);
        }
    }

    /// <summary>
    /// Represents data for a table which was dropped.
    /// </summary>
    public class TableDroppedEventArgs : EventArgs
    {
        /// <summary>
        /// The state of the table before it was dropped.
        /// </summary>
        public DataTable Table { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="TableDroppedEventArgs"/> class with specified table.
        /// </summary>
        internal TableDroppedEventArgs(DataTable table)
        {
            Table = table;
        }
    }

    /// <summary>
    /// Represents data for a newly selected table.
    /// </summary>
    public class TableSelectedEventArgs : EventArgs
    {
        /// <summary>
        /// The name of the selected table.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The columns of the selected table.
        /// </summary>
        public SqlDbColumn[] Columns { get; }

        /// <summary>
        /// Gets the column with name <paramref name="name"/>.
        /// </summary>
        public SqlDbColumn this[string name]
        {
            get => colDict[name];
        }

        #region Fields

        Dictionary<string, SqlDbColumn> colDict;

        #endregion

        /// <summary>
        /// Creates a new instance of the <see cref="TableSelectedEventArgs"/> class from the selected table's columns and a dictionary for identification.
        /// </summary>
        internal TableSelectedEventArgs(string tableName, SqlDbColumn[] columns, Dictionary<string, SqlDbColumn> columnDictionary)
        {
            (Name, Columns, colDict) = (tableName, columns, columnDictionary);
        }
    }

    /// <summary>
    /// Represents data for one or multiple rows which were added to a table.
    /// </summary>
    public class RowsAddedEventArgs : EventArgs
    {
        /// <summary>
        /// The table to which the rows were added.
        /// </summary>
        public string Table { get; }

        /// <summary>
        /// Represents the names of the columns to which the values were added, and the values themselves, for each row.
        /// </summary>
        public Dictionary<string, object>[] Contents { get; }

        /// <summary>
        /// Creates a new instace of the <see cref="RowsAddedEventArgs"/> class from the row content.
        /// </summary>
        internal RowsAddedEventArgs(string table, params Dictionary<string, object>[] rowContents)
        {
            Table = table;
			Contents = rowContents;
        }
    }

    /// <summary>
    /// Represents data for one or multiple rows which were removed from a table.
    /// </summary>
    public class RowsRemovedEventArgs : EventArgs
    {
        /// <summary>
        /// The table from which the rows were removed.
        /// </summary>
        public string Table { get; }

        /// <summary>
        /// The number of rows removed.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Creates a new instace of the <see cref="RowsRemovedEventArgs"/> class with the number of rows removed.
        /// </summary>
        internal RowsRemovedEventArgs(string table, int count)
        {
            Table = table;
			Count = count;
        }
    }

    /// <summary>
    /// Represents data for one or multiple rows which were altered.
    /// </summary>
    public class RowsAlteredEventArgs : EventArgs
    {
        /// <summary>
        /// The table where the rows were altered.
        /// </summary>
        public string Table { get; }

        /// <summary>
        /// Represents the names of the changed columns, and their respective new values.
        /// </summary>
        public Dictionary<string, object> Changes { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="RowsAlteredEventArgs"/> class, with the specified changes.
        /// </summary>
        internal RowsAlteredEventArgs(string table, Dictionary<string, object> changes)
        {
            Table = table;
			Changes = changes;
        }
    }

    /// <summary>
    /// Represents data for one or multiple columns which were added to a table.
    /// </summary>
    public class ColumnsAddedEventArgs : EventArgs
    {
        /// <summary>
        /// The table to which the columns were added.
        /// </summary>
        public string Table { get; }

        /// <summary>
        /// The new columns.
        /// </summary>
        public SqlDbColumn[] Columns { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="ColumnsAddedEventArgs"/> class from the new columns' names, data types and additional data for the types.
        /// </summary>
        internal ColumnsAddedEventArgs(string table, params SqlDbColumn[] cols)
        {
            Table = table;
			Columns = cols;
        }
    }

    /// <summary>
    /// Represents data for one or multiple columns which were removed from a table.
    /// </summary>
    public class ColumnsRemovedEventArgs : EventArgs
    {
        /// <summary>
        /// The table from which the columns were removed.
        /// </summary>
        public string Table { get; }

        /// <summary>
        /// The number of removed columns.
        /// </summary>
        public int Count 
        {
            get => Columns.Length;
        }

        /// <summary>
        /// The removed columns.
        /// </summary>
        public SqlDbColumn[] Columns { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="ColumnsRemovedEventArgs"/> class from the number of removed columns and the removed columns themselves.
        /// </summary>
        internal ColumnsRemovedEventArgs(string table, params SqlDbColumn[] cols)
        {
            Table = table;
			Columns = cols;
        }
    }

    /// <summary>
    /// Represents data for one or multiple rows which were altered.
    /// </summary>
    public class ColumnsAlteredEventArgs : EventArgs
    {
        /// <summary>
        /// The table where the columns were altered.
        /// </summary>
        public string Table { get; }

        /// <summary>
        /// The number of altered columns.
        /// </summary>
        public int Changes 
        { 
            get => OldColumns.Length; 
        }

        /// <summary>
        /// The new column.
        /// </summary>
        public SqlDbColumn NewColumn { get; }

        /// <summary>
        /// The columns before the alteration.
        /// </summary>
        public SqlDbColumn[] OldColumns { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="ColumnsAlteredEventArgs"/> class with the specified number of changes and new columns.
        /// </summary>
        internal ColumnsAlteredEventArgs(string table, SqlDbColumn col, params SqlDbColumn[] oldCols)
        {
            Table = table;
			OldColumns = oldCols;
            NewColumn = col;
        }
    }

    /// <summary>
    /// Represents data for a column from which a primary key was removed.
    /// </summary>
    public class PrimaryKeyRemovedEventArgs : EventArgs
    {
        /// <summary>
        /// The table from which the primary key was removed.
        /// </summary>
        public string Table { get; }

        /// <summary>
        /// The column from which the primary key was removed.
        /// </summary>
        public SqlDbColumn Column { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="PrimaryKeyRemovedEventArgs"/> class, specifiying the column from which the primary key was removed.
        /// </summary>
        internal PrimaryKeyRemovedEventArgs(string table, SqlDbColumn col)
        {
            Table = table;
			Column = col;
        }
    }

    /// <summary>
    /// Represents data for a changed primary key.
    /// </summary>
    public class PrimaryKeyChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The table from which the primary key was removed.
        /// </summary>
        public string Table { get; }

        /// <summary>
        /// The column to which the primary key was set.
        /// </summary>
        public SqlDbColumn Column { get; }

        /// <summary>
        /// The column from which the primary key was removed.
        /// <para>
        /// If the primary key wasn't taken from any column, this property will be <see langword="null"/>.
        /// </para>
        /// </summary>
        public SqlDbColumn? OldColumn { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="PrimaryKeyChangedEventArgs"/> class, specifiying the new column of the primary key and its predecessor.
        /// </summary>
        internal PrimaryKeyChangedEventArgs(string table, SqlDbColumn col, SqlDbColumn? oldCol)
        {
            Table = table;
            Column = col;
            OldColumn = oldCol;
        }
    }

    /// <summary>
    /// Represents data for a failed operation.
    /// </summary>
    public class OperationFailedEventArgs : EventArgs
    {
        /// <summary>
        /// The exception that was supposed to be thrown.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// The name of the method
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="OperationFailedEventArgs"/> class from the original exception.
        /// </summary>
        internal OperationFailedEventArgs(Exception ex)
        {
            Exception = ex;
            MethodName = Regex.Match(ex.StackTrace, @"(?<=^at ).+?(?= \[0x[0-9A-Fa-f]\].*$)").Value;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="OperationFailedEventArgs"/> class.
        /// </summary>
        internal OperationFailedEventArgs(Exception ex, string method)
        {
            Exception = ex;
            MethodName = method;
        }
    }
}