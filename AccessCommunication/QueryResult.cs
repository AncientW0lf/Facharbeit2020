using System;
using System.Collections.Generic;
using System.Data.Common;

namespace AccessCommunication
{
    /// <summary>
    /// This object fetches the returned data of an executed query and saves it.
    /// </summary>
    public struct QueryResult
    {
        /// <summary>
        /// <see cref="bool"/> whether or not the query was successfully executed.
        /// </summary>
        public bool Success { get; internal set; }

        /// <summary>
        /// The underlying query that was executed.
        /// </summary>
        public string ExecutedQuery { get; internal set; }

        /// <summary>
        /// A count of database records that got affected by the underlying query.
        /// </summary>
        public int RecordsAffected { get; internal set; }

        /// <summary>
        /// A list of objects returned by the query.
        /// </summary>
        public List<object[]> ReturnedRows { get; internal set; }

        /// <summary>
        /// Contains an array of returned data columns, if available.
        /// Makes identifying <see cref="ReturnedRows"/> easier.
        /// </summary>
        public string[] ColumnNames { get; internal set; }

        /// <summary>
        /// Fetches the data associated with the query.
        /// </summary>
        /// <param name="query">The query that was executed.</param>
        /// <param name="dataReader">The result reader that was returned by a database command.</param>
        public QueryResult(string query, DbDataReader dataReader)
        {
            //Throws an exception if the reader is already closed
            if(dataReader.IsClosed) throw new ArgumentException("Data reader is closed!");

            //Saves the data
            ExecutedQuery = query;
            RecordsAffected = dataReader.RecordsAffected;
            
            ColumnNames = null;

            //Fetches the data that was returned by the query
            if(dataReader.HasRows)
            {
                ReturnedRows = new List<object[]>();

                int rowCounter = 0;
                while(dataReader.Read())
                {
                    var columns = new List<string>();
                    int counter = 0;
                    while(true)
                    {
	                    try
	                    {
                            columns.Add(dataReader.GetName(counter++));
	                    }
	                    catch(IndexOutOfRangeException)
	                    {
		                    break;
	                    }
                    }

                    ColumnNames = columns.ToArray();

                    ReturnedRows.Add(new object[dataReader.FieldCount]);
                    for(int i = 0; i < dataReader.FieldCount; i++)
                    {
                        ReturnedRows[rowCounter][i] = dataReader[i];
                    }

                    rowCounter++;
                }
            }
            else
            {
                ReturnedRows = null;
            }

            Success = true;
        }
    }
}
