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
            Fields = new List<FieldMapping>();

            Id = Guid.NewGuid().ToString();

            MaxErrors = 10;
            DuplicateHandling = BatchError.Skip;
            ErrorHandling = BatchError.Skip;
        }

        /// <summary>
        /// Gets or sets the identifier for the BatchJob.
        /// </summary>
        /// <value>
        /// The identifier for the BatchJob.
        /// </value>
        public string Id { get; set; }

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
        /// Gets or sets the source mapping. The mappings define how the uploaded data should be translated into the target format.
        /// </summary>
        /// <value>
        /// The source mapping.
        /// </value>
        public List<FieldMapping> Fields { get; set; }


        /// <summary>
        /// Gets or sets the data to be imported.
        /// </summary>
        /// <value>
        /// The data to be imported.
        /// </value>
        public string[][] Data { get; set; }


        /// <summary>
        /// Gets or sets the number of errors.
        /// </summary>
        /// <value>
        /// The number of errors.
        /// </value>
        public int Errors { get; set; }

        /// <summary>
        /// Gets or sets the number of duplicates.
        /// </summary>
        /// <value>
        /// The number of duplicates.
        /// </value>
        public int Duplicates { get; set; }

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
