using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using FluentCommand.Batch.Validation;

namespace FluentCommand.Batch
{
    public class BatchFactory : IBatchFactory
    {
        private readonly List<IBatchValidator> _validators;
        private readonly List<IBatchTranslator> _translators;


        public BatchFactory(IEnumerable<IBatchValidator> validators, IEnumerable<IBatchTranslator> translators)
        {
            _validators = validators?.ToList() ?? new List<IBatchValidator>();
            _translators = translators?.ToList() ?? new List<IBatchTranslator>();
        }

        /// <summary>
        /// Resolves the field translator for the specified name.
        /// </summary>
        /// <param name="name">The name of the translator.</param>
        /// <returns>
        /// An instance of <see cref="T:IBatchTranslator" /> if found; otherwise null.
        /// </returns>
        public IBatchTranslator ResolveTranslator(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return _translators
                .FirstOrDefault(t => t.GetType().Name == name);
        }

        /// <summary>
        /// Resolves the row validator for the specified name.
        /// </summary>
        /// <param name="name">The name of the validator.</param>
        /// <returns>
        /// An instance of <see cref="T:IBatchValidator" /> if found; otherwise null.
        /// </returns>
        public IBatchValidator ResolveValidator(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return _validators
                .FirstOrDefault(t => t.GetType().Name == name);

        }
    }
}