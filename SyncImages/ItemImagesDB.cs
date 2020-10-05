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
        async public Task<List<ItemImage>> GetItemsImages(bool isThumb) {
            string connectionString = ConfigurationManager.ConnectionStrings["B2BConnectionString"].ConnectionString;

            var itemImageList = new List<ItemImage>();
            using (var conn = new SqlConnection(connectionString))
            using (var command = new SqlCommand("sp_B2B_GetItemsImages", conn)
            {
                CommandType = CommandType.StoredProcedure
                
            })
            {
                command.Parameters.Add(new SqlParameter("@isthumb", isThumb));
                conn.Open();
                var reader = await command.ExecuteReaderAsync();
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
    }
}
