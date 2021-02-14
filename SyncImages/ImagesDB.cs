using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncImages
{
    public class ImagesDBType {
        public string ItemImageMap;
        public string ItemThumbMap;
    }

    public class ImagesDB
    {
        string connectionString = ConfigurationManager.ConnectionStrings["B2BConnectionString"].ConnectionString;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public List<ImagesDBType> GetImagesNames()
        {
            try
            {
                List<ImagesDBType> result = new List<ImagesDBType>();
                using (var conn = new SqlConnection(connectionString))
                using (var command = new SqlCommand("B2B_Items_GetImages", conn)
                {
                    CommandType = CommandType.StoredProcedure

                })
                {
                    conn.Open();
                    var reader = command.ExecuteReader();
                    // iterate through results, printing each to console
                    while (reader.Read())
                    {
                        var imagesDBType = new ImagesDBType()
                        {
                            ItemImageMap = reader.SafeGetString("ItemImageMap"),
                            ItemThumbMap = reader.SafeGetString("ItemThumbMap"),
                        };
                        result.Add(imagesDBType);
                        //Console.WriteLine((String)reader["ItemCode"] + ' ' + (String)reader["Image"] + ' ' + ((DateTime)reader["Date"]).ToLongDateString());
                    }
                    conn.Close();
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return null;
        }


        public int UpdateImagesLog(int Type)
        {
            try
            {
                int result = -1;
                using (var conn = new SqlConnection(connectionString))
                using (var command = new SqlCommand("sp_B2B_UpdateItemsImages_Log", conn)
                {
                    CommandType = CommandType.StoredProcedure

                })
                {
                    command.Parameters.Add(new SqlParameter("@Type", Type));

                    conn.Open();
                    result = command.ExecuteNonQuery();
                    conn.Close();
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return -1;
        }
    }
}
