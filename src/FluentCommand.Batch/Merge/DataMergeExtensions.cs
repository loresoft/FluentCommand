using System;

namespace FluentCommand.Merge
{
    public static class DataMergeExtensions
    {
        /// <summary>
        /// Starts a data merge operation with the specified destination table name.
        /// </summary>
        /// <param name="session">The session to use for the merge.</param>
        /// <param name="destinationTable">Name of the destination table on the server.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="DataMerge " /> operation.
        /// </returns>
        public static IDataMerge MergeData(this IDataSession session, string destinationTable)
        {
            var definition = new DataMergeDefinition();
            definition.TargetTable = destinationTable;

            return MergeData(session, definition);
        }

        /// <summary>
        /// Starts a data merge operation with the specified destination table name.
        /// </summary>
        /// <param name="session">The session to use for the merge.</param>
        /// <param name="mergeDefinition">The data merge definition.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="DataMerge " /> operation.
        /// </returns>
        public static IDataMerge MergeData(this IDataSession session, DataMergeDefinition mergeDefinition)
        {
            var dataMerge = new DataMerge(session, mergeDefinition);
            return dataMerge;
        }

    }
}
