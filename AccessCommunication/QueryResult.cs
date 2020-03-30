using System;
using System.Data.OleDb;

namespace AccessCommunication
{
    public struct QueryResult
    {
        public bool Success { get; internal set; }

        public string ExecutedQuery { get; internal set; }

        public int RecordsAffected { get; internal set; }

        public object[,] ReturnedRows { get; internal set; }

        public QueryResult(string query, OleDbDataReader dataReader)
        {
            if(dataReader.IsClosed) throw new ArgumentException("Data reader is closed!");

            ExecutedQuery = query;
            RecordsAffected = dataReader.RecordsAffected;

            if(dataReader.HasRows)
            {
                ReturnedRows = new object[dataReader.FieldCount,dataReader.RecordsAffected];

                int rowCounter = 0;
                while(dataReader.Read())
                {
                    for(int i = 0; i < dataReader.FieldCount; i++)
                    {
                        ReturnedRows[i, rowCounter] = dataReader[i];
                    }
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
