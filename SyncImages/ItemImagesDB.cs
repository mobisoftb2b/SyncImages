using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace SyncImages
{
    public class ItemImage { 
        public String ItemCode { get; set; }
        public String Image { get; set; }
        public DateTime Date { get; set; }

        public ItemImage(String itemCode, String image, DateTime date) {
            ItemCode = itemCode;
            Image = image;
            Date = date;
        }
    }
    public class ItemImagesDB
    {
        string connectionString = ConfigurationManager.ConnectionStrings["B2BConnectionString"].ConnectionString;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public List<ItemImage> GetItemsImages(bool isThumb) {

            try
            {
                var itemImageList = new List<ItemImage>();
                using (var conn = new SqlConnection(connectionString))
                using (var command = new SqlCommand("sp_B2B_GetItemsImages", conn)
                {
                    CommandType = CommandType.StoredProcedure

                })
                {
                    command.Parameters.Add(new SqlParameter("@isthumb", isThumb));
                    conn.Open();
                    var reader = command.ExecuteReader();
                    // iterate through results, printing each to console
                    while (reader.Read())
                    {
                        var itemImage = new ItemImage((String)reader["ItemCode"], (String)reader["Image"], (DateTime)reader["Date"]);
                        itemImageList.Add(itemImage);
                        //Console.WriteLine((String)reader["ItemCode"] + ' ' + (String)reader["Image"] + ' ' + ((DateTime)reader["Date"]).ToLongDateString());
                    }
                    conn.Close();
                }

                return itemImageList;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return null;
        }

        public int UpdateItemsLog (int Type)
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
