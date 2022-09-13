#region Warnings

#pragma warning disable CS1591 //Contructor of struct left undescribed

#endregion

namespace System.Data.SqlDatabase
{
    /// <summary>
    /// Represents additional data for a <see cref="SqlDbType"/>.
    /// </summary>
    public struct SqlDbTypeData
    {
        /// <summary>
        /// The scale of a <see cref="SqlDbType"/> which represents the number of digits to the right of the decimal point of a number.
        /// <para>
        /// If this property is <see langword="null"/>, the type doesn't have a scale.
        /// </para>
        /// </summary>
        public int? Scale { get; }

        /// <summary>
        /// The length of a <see cref="SqlDbType"/>.
        /// <para>
        /// If this property is <see langword="null"/>, the type doesn't have a length.
        /// </para>
        /// </summary>
        public int? Length { get; }

        /// <summary>
        /// Determines if a <see cref="SqlDbType"/> can accept <see langword="null"/> values.
        /// </summary>
        public bool CanBeNull { get; }

        #region Constructors

        public SqlDbTypeData(bool canBeNull)
        {
            CanBeNull = canBeNull;
            Scale = Length = null;
        }

        public SqlDbTypeData(int? length, bool canBeNull)
        {
            Length = length;
            CanBeNull = canBeNull;
            Scale = null;
        }

        public SqlDbTypeData(int? length, int? scale, bool canBeNull)
        {
            Scale = scale;
            Length = length;
            CanBeNull = canBeNull;
        }

        #endregion
    }

    /// <summary>
    /// A column that belongs to a <see cref="SqlDatabase"/> table.
    /// </summary>
    public struct SqlDbColumn
    {
        /// <summary>
        /// The name of the column.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The type of the column.
        /// </summary>
        public SqlDbType Type { get; }

        /// <summary>
        /// Additional data for the column's type.
        /// </summary>
        public SqlDbTypeData Data { get; }

        public SqlDbColumn(string name, SqlDbType type, SqlDbTypeData data)
        {
            Name = name;
            Type = type;
            Data = data;
        }
    }

    public enum SqlExecutionType
    {
        Scalar,
        Reader,
        XmlReader,
        NonQuery
    }
}
