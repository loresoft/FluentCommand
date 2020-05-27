using System;
using System.Data;
using System.Linq;

namespace FluentCommand.Batch.Fluent
{
    public class BatchFieldBuilder
    {
        private readonly FieldMapping _fieldMapping;

        public BatchFieldBuilder(FieldMapping fieldMapping)
        {
            _fieldMapping = fieldMapping;
        }

        public BatchFieldBuilder Index(int? index)
        {
            _fieldMapping.Index = index;
            return this;
        }

        public BatchFieldBuilder Name(string value)
        {
            _fieldMapping.Name = value;
            return this;
        }

        public BatchFieldBuilder DisplayName(string value)
        {
            _fieldMapping.DisplayName = value;
            return this;
        }

        public BatchFieldBuilder NativeType(string value)
        {
            _fieldMapping.NativeType = value;
            return this;
        }
        
        public BatchFieldBuilder DataType<T>()
        {
            _fieldMapping.DataType = typeof(T);
            return this;
        }

        public BatchFieldBuilder DataType(Type value)
        {
            _fieldMapping.DataType = value;
            return this;
        }


        public BatchFieldBuilder IsKey(bool value = true)
        {
            _fieldMapping.IsKey = value;

            if (!value) 
                return this;

            // defaults for a key field
            _fieldMapping.CanBeKey = true;
            _fieldMapping.CanUpdate = false;

            return this;
        }

        public BatchFieldBuilder CanBeKey(bool value = true)
        {
            _fieldMapping.CanBeKey = value;
            return this;
        }

        public BatchFieldBuilder CanInsert(bool value = true)
        {
            _fieldMapping.CanInsert = value;
            return this;
        }

        public BatchFieldBuilder CanUpdate(bool value = true)
        {
            _fieldMapping.CanUpdate = value;
            return this;
        }

        public BatchFieldBuilder CanMap(bool value = true)
        {
            _fieldMapping.CanMap = value;
            return this;
        }

        public BatchFieldBuilder CanBeNull(bool value = true)
        {
            _fieldMapping.CanBeNull = value;
            return this;
        }


        public BatchFieldBuilder Required(bool value = true)
        {
            _fieldMapping.Required = value;
            return this;
        }


        public BatchFieldBuilder Default(FieldDefault? value)
        {
            _fieldMapping.Default = value;
            if (value.HasValue && value != FieldDefault.Static) 
                _fieldMapping.CanMap = false;

            return this;
        }

        public BatchFieldBuilder Default(object value)
        {
            _fieldMapping.DefaultValue = value;
            _fieldMapping.Default = FieldDefault.Static;

            if (!Equals(value, null))
            {
                _fieldMapping.Required = true;
                _fieldMapping.CanMap = false;
            }

            return this;
        }


        public BatchFieldBuilder Translator<TTranslator>()
            where TTranslator : IBatchTranslator, new()
        {
            _fieldMapping.TranslatorType = typeof(TTranslator).FullName;

            var instance = new TTranslator();
            _fieldMapping.TranslatorSources = instance.Sources.ToList();

            return this;
        }

        public BatchFieldBuilder TranslatorSource(string value)
        {
            _fieldMapping.TranslatorSource = value;
            return this;
        }


        public BatchFieldBuilder Expression(string text)
        {
            Match(match => match.Text(text).UseRegex());
            return this;
        }

        public BatchFieldBuilder Match(string text)
        {
            Match(match => match.Text(text));
            return this;
        }

        public BatchFieldBuilder Match(Action<BatchMatchBuilder> builder)
        {
            var fieldMatch = new FieldMatch();
            var fieldBuilder = new BatchMatchBuilder(fieldMatch);
            builder(fieldBuilder);

            _fieldMapping.MatchDefinitions.Add(fieldMatch);

            return this;
        }

    }
}