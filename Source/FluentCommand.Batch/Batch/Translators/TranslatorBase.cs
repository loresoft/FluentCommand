using System;
using FluentCommand.Extensions;
using FluentCommand.Logging;

namespace FluentCommand.Batch.Translators
{
    public abstract class TranslatorBase : IBatchTranslator
    {
        public const string SourceNone = "";

        private bool _isLoaded = false;

        public abstract string[] Sources { get; }

        public virtual object Translate(string source, object original)
        {
            if (original == null || Convert.IsDBNull(original))
                return null;

            if (source.IsNullOrEmpty() || source == SourceNone)
                return original;

            Load();

            return TranslateCore(source, original);
        }


        public virtual void Load()
        {
            if (_isLoaded)
                return;

            Logger.Trace()
                .Message("Loading translator data for '{0}'...", this)
                .Write();

            LoadData();

            _isLoaded = true;
        }


        protected string ToString(object value)
        {
            if (value == null || Convert.IsDBNull(value))
                return null;

            var s = value as string;
            if (s != null)
                return s.Trim();

            return Convert.ToString(value);
        }

        protected int ToInt32(object value)
        {
            if (value == null || Convert.IsDBNull(value))
                return 0;

            if (value is int)
                return (int)value;

            var s = value as string;
            if (s != null)
                return s.Trim().ToInt32();

            return Convert.ToInt32(value);
        }

        protected long ToInt64(object value)
        {
            if (value == null || Convert.IsDBNull(value))
                return 0;

            if (value is long)
                return (long)value;

            var s = value as string;
            if (s != null)
                return s.Trim().ToInt64();

            return Convert.ToInt64(value);
        }

        protected Guid ToGuid(object value)
        {
            if (value == null || Convert.IsDBNull(value))
                return Guid.Empty;

            if (value is Guid)
                return (Guid)value;

            var s = value as string;
            if (s != null)
                return s.Trim().ToGuid();

            var bytes = value as byte[];
            if (bytes != null)
                return new Guid(bytes);

            return Guid.Empty;
        }

        protected DateTime ToDateTime(object value)
        {
            if (value == null || Convert.IsDBNull(value))
                return default(DateTime);

            if (value is DateTime)
                return (DateTime)value;

            var s = value as string;
            if (s != null)
                return s.Trim().ToDateTime();

            return default(DateTime);
        }


        protected abstract object TranslateCore(string source, object original);

        protected abstract void LoadData();
    }
}