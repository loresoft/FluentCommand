namespace FluentCommand.Query;

/// <summary>
/// Query filter operators
/// </summary>
public enum FilterOperators
{
    /// <summary>Starts with query operator</summary>
    StartsWith,
    /// <summary>Ends with query operator</summary>
    EndsWith,
    /// <summary>Contains query operator</summary>
    Contains,
    /// <summary>Equal query operator</summary>
    Equal,
    /// <summary>Not equal query operator</summary>
    NotEqual,
    /// <summary>Less than query operator</summary>
    LessThan,
    /// <summary>Less than or equal query operator</summary>
    LessThanOrEqual,
    /// <summary>Greater than query operator</summary>
    GreaterThan,
    /// <summary>Greater than or equal query operator</summary>
    GreaterThanOrEqual,
    /// <summary>Is null query operator</summary>
    IsNull,
    /// <summary>Is not null query operator</summary>
    IsNotNull,
    /// <summary>In query operator</summary>
    In
}
