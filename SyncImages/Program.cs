using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SyncImages
{
   
    class Program
    {
        static void Main(string[] args)
        {
            ProcessFiles p = new ProcessFiles();
            ItemImagesDB db = new ItemImagesDB();

            if (ConfigurationManager.AppSettings["CallUpdateOnly"] == "true")
            {
                p.CallUpdateImages().Wait();
            }
            else
            {
                db.UpdateItemsLog(0);
                p.PreProcessImages().Wait();
                p.ProcessFilesImages(ConfigurationManager.AppSettings["ImageFolder"], ConfigurationManager.AppSettings["LinuxImagesFolder"], false).Wait();
                p.ProcessFilesImages(ConfigurationManager.AppSettings["BannerFolder"], "img", false).Wait();
                p.ProcessFilesImages(ConfigurationManager.AppSettings["ThumbFolder"], ConfigurationManager.AppSettings["LinuxThumbsFolder"], true).Wait();
                db.UpdateItemsLog(1);
                
            }
            //Console.ReadLine();
        }
    }
}
