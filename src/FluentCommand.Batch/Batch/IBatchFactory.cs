using System;

namespace FluentCommand.Batch
{
    /// <summary>
    /// An <see langword="interface"/> for resolving named type
    /// </summary>
    public interface IBatchFactory
    {
        /// <summary>
        /// Resolves the field translator for the specified name.
        /// </summary>
        /// <param name="name">The name of the translator.</param>
        /// <returns>An instance of <see cref="T:IBatchTranslator"/> if found; otherwise null.</returns>
        IBatchTranslator ResolveTranslator(string name);

        /// <summary>
        /// Resolves the row validator for the specified name.
        /// </summary>
        /// <param name="name">The name of the validator.</param>
        /// <returns>An instance of <see cref="T:IBatchValidator"/> if found; otherwise null.</returns>
        IBatchValidator ResolveValidator(string name);

        /// <summary>
        /// Resolves the reader for the specified name.
        /// </summary>
        /// <param name="name">The reader name.</param>
        /// <returns>
        /// An instance of <see cref="T:IBatchReader" /> if found; otherwise null.
        /// </returns>
        IBatchReader ResolveReader(string name);

        /// <summary>
        /// Resets all cached instances.
        /// </summary>
        void Reset();
    }
}