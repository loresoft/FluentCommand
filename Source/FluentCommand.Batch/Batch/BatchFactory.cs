using System;
using System.Collections.Concurrent;
using FluentCommand.Batch.Validation;

namespace FluentCommand.Batch
{
    public class BatchFactory : IBatchFactory
    {

        private readonly ConcurrentDictionary<string, IBatchTranslator> _translatorCache;
        private readonly ConcurrentDictionary<string, Func<IBatchTranslator>> _translators;

        private readonly ConcurrentDictionary<string, IBatchValidator> _validatorCache;
        private readonly ConcurrentDictionary<string, Func<IBatchValidator>> _validators;


        private readonly ConcurrentDictionary<string, Func<IBatchReader>> _readers;


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

            return _translatorCache.GetOrAdd(name, key =>
            {
                Func<IBatchTranslator> factory;
                return _translators.TryGetValue(key, out factory)
                    ? factory()
                    : null;
            });
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

            return _validatorCache.GetOrAdd(name, key =>
            {
                Func<IBatchValidator> factory;
                return _validators.TryGetValue(name, out factory) 
                    ? factory() 
                    : null;
            });
        }

        /// <summary>
        /// Resolves the reader for the specified name.
        /// </summary>
        /// <param name="name">The reader name.</param>
        /// <returns>
        /// An instance of <see cref="T:IBatchReader" /> if found; otherwise null.
        /// </returns>
        public IBatchReader ResolveReader(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            Func<IBatchReader> factory;
            if (_readers.TryGetValue(name, out factory))
                return factory();

            return null;
        }


        /// <summary>
        /// Resets all cached instances.
        /// </summary>
        public void Reset()
        {
            _translatorCache.Clear();
            _validatorCache.Clear();
        }


        /// <summary>
        /// Registers the translator of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of translator to register.</typeparam>
        /// <returns></returns>
        public void RegisterTranslator<T>() where T : IBatchTranslator, new()
        {
            RegisterTranslator(typeof(T).FullName, () => new T());
        }

        /// <summary>
        /// Registers the translator factory with the specified name.
        /// </summary>
        /// <param name="name">The name of the translator.</param>
        /// <param name="translator">The translator factory to register.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">name</exception>
        public void RegisterTranslator(string name, Func<IBatchTranslator> translator)
        {
            if (name == null)
                throw new ArgumentNullException("name");


            _translators.AddOrUpdate(name, translator, (k, v) => translator);
        }


        /// <summary>
        /// Registers the validator of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of validator to register.</typeparam>
        /// <returns></returns>
        public void RegisterValidator<T>() where T : IBatchValidator, new()
        {
            RegisterValidator(typeof(T).FullName, () => new T());
        }

        /// <summary>
        /// Registers the validator factory with the specified name.
        /// </summary>
        /// <param name="name">The name of the validator.</param>
        /// <param name="validator">The validator factory to register.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">name</exception>
        public void RegisterValidator(string name, Func<IBatchValidator> validator)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            _validators.AddOrUpdate(name, validator, (k, v) => validator);
        }


        /// <summary>
        /// Registers the reader of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of reader to register.</typeparam>
        /// <returns></returns>
        public void RegisterReader<T>() where T : IBatchReader, new()
        {
            RegisterReader(typeof(T).FullName, () => new T());
        }
        
        /// <summary>
        /// Registers the reader factory with the specified name.
        /// </summary>
        /// <param name="name">The reader name.</param>
        /// <param name="reader">The reader factory to register.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">fileExtension</exception>
        public void RegisterReader(string name, Func<IBatchReader> reader)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            _readers.AddOrUpdate(name, reader, (k, v) => reader);
        }


        #region Singleton

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchFactory"/> class.
        /// </summary>
        protected BatchFactory()
        {
            _translatorCache = new ConcurrentDictionary<string, IBatchTranslator>();
            _validatorCache = new ConcurrentDictionary<string, IBatchValidator>();
            _translators = new ConcurrentDictionary<string, Func<IBatchTranslator>>();
            _validators = new ConcurrentDictionary<string, Func<IBatchValidator>>();
            _readers = new ConcurrentDictionary<string, Func<IBatchReader>>(StringComparer.OrdinalIgnoreCase);

            RegisterValidator<BatchValidator>();
        }

        private static readonly Lazy<BatchFactory> _current = new Lazy<BatchFactory>(() => new BatchFactory());

        /// <summary>
        /// Gets the current singleton instance of BatchFactory.
        /// </summary>
        /// <value>The current singleton instance.</value>
        /// <remarks>
        /// An instance of BatchFactory wont be created until the very first 
        /// call to the sealed class. This is a CLR optimization that
        /// provides a properly lazy-loading singleton. 
        /// </remarks>
        public static BatchFactory Default
        {
            get { return _current.Value; }
        }
        #endregion
    }
}