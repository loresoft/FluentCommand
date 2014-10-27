using System;

namespace FluentCommand.Batch
{
    /// <summary>
    /// Default field value
    /// </summary>
    public enum FieldDefault
    {
        /// <summary>
        /// Use the <see cref="P:BatchJob.UserName"/> as the default value.
        /// </summary>
        UserName,
        /// <summary>
        /// Use the current date time as the default value.
        /// </summary>
        CurrentDate,
        /// <summary>
        /// Use a static default value.
        /// </summary>
        Static
    }
}