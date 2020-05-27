using System.ComponentModel;

namespace FluentCommand.Batch
{
    /// <summary>
    /// How to handle batch errors
    /// </summary>
    public enum BatchError
    {
        /// <summary>
        /// Skip the error and move to next row
        /// </summary>
        [Description("Skip error and continue")]
        Skip,
        /// <summary>
        /// Quit processing batch
        /// </summary>
        [Description("Stop processing")]
        Quit
    }
}