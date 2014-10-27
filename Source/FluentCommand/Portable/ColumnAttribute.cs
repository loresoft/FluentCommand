using System;

#if !NET45

// ReSharper disable once CheckNamespace
namespace System.ComponentModel.DataAnnotations.Schema
{
    /// <summary>
    /// Represents a column attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ColumnAttribute : Attribute
    {
        private int _order = -1;
        private readonly string _name;
        private string _typeName;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.Schema.ColumnAttribute"/> class.
        /// </summary>
        public ColumnAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.Schema.ColumnAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of the column attribute.</param>
        public ColumnAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name can not be null or empty.", "name");

            _name = name;
        }

        /// <summary>
        /// Gets the name of the attribute.
        /// </summary>
        /// <returns>
        /// The name of the attribute.
        /// </returns>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets or sets the order of the column.
        /// </summary>
        /// <returns>
        /// The order of the column.
        /// </returns>
        public int Order
        {
            get { return _order; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value");
                _order = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the class that the <see cref="T:System.ComponentModel.DataAnnotations.Schema.ColumnAttribute"/> represents.
        /// </summary>
        /// <returns>
        /// The name of the class that the <see cref="T:System.ComponentModel.DataAnnotations.Schema.ColumnAttribute"/> represents.
        /// </returns>
        public string TypeName
        {
            get { return _typeName; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("TypeName can not be null or empty.");

                _typeName = value;
            }
        }
    }
}

#endif
