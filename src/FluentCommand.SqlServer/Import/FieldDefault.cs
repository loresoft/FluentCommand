namespace FluentCommand.Import
{
    /// <summary>
    /// Default field value
    /// </summary>
    public enum FieldDefault
    {
        /// <summary>
        /// Use the current username as the default value.
        /// </summary>
        UserName,
        /// <summary>
        /// Use the current UTC date time as the default value.
        /// </summary>
        CurrentDate,
        /// <summary>
        /// Use a static default value.
        /// </summary>
        Static
    }
}