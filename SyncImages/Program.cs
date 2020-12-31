using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncImages
{
   
    class Program
    {
        static void Main(string[] args)
        {
            ProcessFiles p = new ProcessFiles();

            if (ConfigurationManager.AppSettings["CallUpdateOnly"] == "true")
            {
                p.CallUpdateImages().Wait();
            }
            else
            {
                p.PreProcessImages().Wait();
                p.ProcessFilesImages(ConfigurationManager.AppSettings["ImageFolder"], ConfigurationManager.AppSettings["LinuxImagesFolder"], false).Wait();
                //p.ProcessFilesImages(ConfigurationManager.AppSettings["BannerFolder"], "img", false).Wait();
                p.ProcessFilesImages(ConfigurationManager.AppSettings["ThumbFolder"], ConfigurationManager.AppSettings["LinuxThumbsFolder"], true).Wait();
            }
            //Console.ReadLine();
        }
    }
}
