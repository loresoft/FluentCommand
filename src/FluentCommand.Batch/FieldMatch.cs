using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCommand.Batch;

/// <summary>
/// A class to define a field match for field mapping.
/// </summary>
public class FieldMatch
{
    /// <summary>
    /// Gets or sets the text used to match a field name. If <see cref="P:UseRegex"/> is true, this will be used as a regular expression.
    /// </summary>
    /// <value>
    /// The text used to match a field name.
    /// </value>
    public string Text { get; set; }

    /// <summary>
    /// Gets or sets the default translator source when matched.
    /// </summary>
    /// <value>
    /// The translator source.
    /// </value>
    public string TranslatorSource { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use <see cref="P:Text"/> as regular expression.
    /// </summary>
    /// <value>
    ///   <c>true</c> to use regex; otherwise, <c>false</c>.
    /// </value>
    public bool UseRegex { get; set; }
}
