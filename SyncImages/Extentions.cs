using System;
using System.Data.SqlClient;


namespace SyncImages
{
    public static class Extentions
    {
        public static Nullable<int> SafeGetInt(this SqlDataReader reader, string columnName)
        {
            if (reader[columnName] == DBNull.Value)
                return null;
            return (int)reader[columnName];
        }

        public static Nullable<long> SafeGetLong(this SqlDataReader reader, string columnName)
        {
            if (reader[columnName] == DBNull.Value)
                return null;
            return (long)reader[columnName];
        }

        public static string SafeGetString(this SqlDataReader reader, string columnName)
        {
            if (reader[columnName] == DBNull.Value)
                return null;
            return (string)reader[columnName];
        }
    }

}
