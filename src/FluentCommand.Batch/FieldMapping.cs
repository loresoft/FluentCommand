using System;
using System.Collections.Generic;
using System.Data;

namespace FluentCommand.Batch
{
    /// <summary>
    /// Batch field mapping definition
    /// </summary>
    public class FieldMapping : FieldIndex
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldMapping"/> class.
        /// </summary>
        public FieldMapping()
        {
            TranslatorSources = new List<string>();
            MatchDefinitions = new List<FieldMatch>();

            CanInsert = true;
            CanUpdate = true;
            CanMap = true;
        }

        /// <summary>
        /// Gets or sets the field display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the database native type for this column.  Used to build the temporary table during import.
        /// </summary>
        /// <value>
        /// The type of the native.
        /// </value>
        public string NativeType { get; set; }

        /// <summary>
        /// Gets or sets the field data type. Used to build the temporary table during import.
        /// </summary>
        /// <value>
        /// The type of the data.
        /// </value>
        public Type DataType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this field is a key.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this field is a key; otherwise, <c>false</c>.
        /// </value>
        public bool IsKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this field can be key. This is used by the 
        /// GUI to allow the user to select which field are used as key when merging the data.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can be key; otherwise, <c>false</c>.
        /// </value>
        public bool CanBeKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this field can insert.
        /// </summary>
        /// <value>
        /// <c>true</c> if this field can insert; otherwise, <c>false</c>.
        /// </value>
        public bool CanInsert { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this field can update.
        /// </summary>
        /// <value>
        /// <c>true</c> if this field can update; otherwise, <c>false</c>.
        /// </value>
        public bool CanUpdate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this field can be mapped by the users.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this field can be mapped; otherwise, <c>false</c>.
        /// </value>
        public bool CanMap { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this field can be null.
        /// </summary>
        /// <value>
        /// <c>true</c> if this field can be null; otherwise, <c>false</c>.
        /// </value>
        public bool CanBeNull { get; set; }


        /// <summary>
        /// Gets or sets the default value generation.
        /// </summary>
        /// <value>
        /// The default value generation.
        /// </value>
        public FieldDefault? Default { get; set; }

        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        /// <value>
        /// The default value.
        /// </value>
        public object DefaultValue { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether this field is required.
        /// </summary>
        /// <value>
        /// <c>true</c> if this field is required; otherwise, <c>false</c>.
        /// </value>
        public bool Required { get; set; }
        

        /// <summary>
        /// Gets or sets the list of available translator sources.
        /// </summary>
        /// <value>
        /// The list of available translator sources.
        /// </value>
        public List<string> TranslatorSources { get; set; }

        /// <summary>
        /// Gets or sets the user selected translator source.
        /// </summary>
        /// <value>
        /// The translator source.
        /// </value>
        public string TranslatorSource { get; set; }

        /// <summary>
        /// Gets or sets the full type name of data translator for this field. Must be of type <see cref="IBatchTranslator"/>.
        /// </summary>
        /// <value>
        /// The type of the translator.
        /// </value>
        public string TranslatorType { get; set; }


        /// <summary>
        /// Gets or sets the field match definitions for this <see cref="T:FieldMapping"/>.
        /// </summary>
        public List<FieldMatch> MatchDefinitions { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{base.ToString()}, DataType: {DataType}";
        }
    }
}