using System;
using System.Collections.Generic;
using System.Data.OleDb;

namespace AccessCommunication
{
    public struct QueryResult
    {
        public bool Success { get; internal set; }

        public string ExecutedQuery { get; internal set; }

        public int RecordsAffected { get; internal set; }

        public List<object[]> ReturnedRows { get; internal set; }

        public QueryResult(string query, OleDbDataReader dataReader)
        {
            if(dataReader.IsClosed) throw new ArgumentException("Data reader is closed!");

            ExecutedQuery = query;
            RecordsAffected = dataReader.RecordsAffected;

            if(dataReader.HasRows)
            {
                ReturnedRows = new List<object[]>();

                int rowCounter = 0;
                while(dataReader.Read())
                {
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
