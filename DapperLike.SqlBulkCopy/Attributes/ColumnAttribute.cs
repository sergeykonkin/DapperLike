using System;

namespace DapperLike
{
    /// <summary>
    /// Allows specifying the SQL column name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        /// <summary>
        /// Gets the column name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes new instance of <see cref="ColumnAttribute" />
        /// </summary>
        /// <param name="name">Column name.</param>
        public ColumnAttribute(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }
}
