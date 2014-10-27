using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCommand.Batch
{
    /// <summary>
    /// BatchJob definition class
    /// </summary>
    public class BatchJob
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BatchJob"/> class.
        /// </summary>
        public BatchJob()
        {
            SourceFields = new List<FieldIndex>();
            SourceMapping = new List<FieldMapping>();
            UserName = Security.UserHelper.Current();

            MaxErrors = 10;
            DuplicateHandling = BatchError.Skip;
            ErrorHandling = BatchError.Skip;
        }

        /// <summary>
        /// Gets or sets the duplicate handling.
        /// </summary>
        /// <value>
        /// The duplicate handling.
        /// </value>
        public BatchError DuplicateHandling { get; set; }

        /// <summary>
        /// Gets or sets the error handling.
        /// </summary>
        /// <value>
        /// The error handling.
        /// </value>
        public BatchError ErrorHandling { get; set; }

        /// <summary>
        /// Gets or sets the maximum errors.
        /// </summary>
        /// <value>
        /// The maximum errors.
        /// </value>
        public int MaxErrors { get; set; }


        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the name of the file uploaded.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the working file path. This is the file on disk that will be read.
        /// </summary>
        public string WorkingFile { get; set; }

        /// <summary>
        /// Gets or sets the name of the entity type.  This is used for audit logs.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        public string EntityType { get; set; }

        /// <summary>
        /// Gets or sets the type of the validator used for data validation. If null, then no validation will be done.
        /// </summary>
        /// <value>
        /// The type of the validator.
        /// </value>
        public string ValidatorType { get; set; }

        /// <summary>
        /// Gets or sets the name of the user who started the batch job.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the target table to merge the uploaded data into.
        /// </summary>
        /// <value>
        /// The target table name.
        /// </value>
        public string TargetTable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether data can be inserted that doesn't exists in the <see cref="TargetTable"/>.
        /// </summary>
        /// <value>
        /// <c>true</c> if data can be inserted; otherwise, <c>false</c>.
        /// </value>
        public bool CanInsert { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether data can be updated when it exists in the <see cref="TargetTable"/>.
        /// </summary>
        /// <value>
        /// <c>true</c> if data can be updated; otherwise, <c>false</c>.
        /// </value>
        public bool CanUpdate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether data can be deleted when it is in the <see cref="TargetTable"/> and not in the source.
        /// </summary>
        /// <value>
        /// <c>true</c> if data can be deleted; otherwise, <c>false</c>.
        /// </value>
        public bool CanDelete { get; set; }


        /// <summary>
        /// Gets or sets the source fields from the uploaded file.
        /// </summary>
        /// <value>
        /// The source field names and indexes.
        /// </value>
        public List<FieldIndex> SourceFields { get; set; }

        /// <summary>
        /// Gets or sets the source mapping. The mappings define how the uploaded data should be translated into the target format.
        /// </summary>
        /// <value>
        /// The source mapping.
        /// </value>
        public List<FieldMapping> SourceMapping { get; set; }


        internal int Errors { get; set; }

        internal int Duplicates { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("FileName: {0}, TargetTable: {1}, UserName: {2}", FileName, TargetTable, UserName);
        }
    }
}
