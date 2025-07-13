namespace FluentCommand.Import;

/// <summary>
/// Specifies the default value behavior for a field during import operations.
/// </summary>
public enum FieldDefault
{
    /// <summary>
    /// Use the current user's name as the default value for the field.
    /// </summary>
    UserName,
    /// <summary>
    /// Use the current UTC date and time as the default value for the field.
    /// </summary>
    CurrentDate,
    /// <summary>
    /// Use a static, predefined value as the default for the field.
    /// </summary>
    Static
}
