using System;

#if !NET45

// ReSharper disable once CheckNamespace
namespace System.ComponentModel.DataAnnotations.Schema
{
    /// <summary>
    /// Denotes that a property or class should be excluded from database mapping.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class NotMappedAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute"/> class.
        /// </summary>
        public NotMappedAttribute()
        {
        }
    }
}

#endif
