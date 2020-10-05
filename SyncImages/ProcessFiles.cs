using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Configuration;
using Newtonsoft.Json;

namespace SyncImages
{
    public class ImageFile { 
        public String Name { get; set; }
        public DateTime Date { get; set; }
    }


    public class ProcessFiles
    {
        NetworkHelpers helper = new NetworkHelpers();
        static Task<int> CallCopyImagesBatchAsync (string imageName, string folder)
        {
            var tcs = new TaskCompletionSource<int>();
            try
            {

                var process = new Process
                {
                    StartInfo = { FileName = "CopyImages.bat",
                  WindowStyle = ProcessWindowStyle.Hidden,
                  Arguments = string.Format("{0} {1}",
                                                imageName,
                                               folder)},
                    EnableRaisingEvents = true

                };

                process.Exited += (sender, args) =>
                {
                    tcs.SetResult(process.ExitCode);
                    process.Dispose();
                };

                process.Start();

                
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
            return tcs.Task;
        }
        
      
        async Task<int> ProcessFile(String imagename, String folder) {
            Console.WriteLine(imagename);
            return await CallCopyImagesBatchAsync(imagename, folder);
        }

        public static Task DeleteAsync(string path)
        {
             return Task.Run(() => { 
                 if (File.Exists(path))
                 { File.Delete(path); System.Threading.Thread.Sleep(100); }
             });
        }

        async public Task CallUpdateImages() {
            String UpdateImagesURL = ConfigurationManager.AppSettings["UpdateImagesURL"];
            String result1 = await helper.CallGetService(UpdateImagesURL);
            Console.WriteLine("UpdateImageService " + result1);
        }
        async public Task ProcessFilesImages(String targetDirectory, String folder, bool isthumb)
        {
            try
            {
                NetworkHelpers helper = new NetworkHelpers();

                Console.WriteLine(targetDirectory);
                
                bool newfileFound = false;
                if (!String.IsNullOrEmpty(targetDirectory) && Directory.Exists(targetDirectory))
                {

                    // update firstly anyway
                    await CallUpdateImages();
                    
                    
                    //get linux files
                    String result = "";
                    String getImagesWithDatesURL = ConfigurationManager.AppSettings["GetImagesWithDatesURL"];
                    /*if (isthumb)
                    {
                        result = await helper.CallPostService(getImagesWithDatesURL, @"{""isthumb"": true }");
                    }
                    else
                    {
                        result = await helper.CallPostService(getImagesWithDatesURL, @"{""isthumb"": false }");
                    }
                    */
                    if (isthumb)
                    {
                        result = await helper.CallGetService(getImagesWithDatesURL + "?isthumb=true");
                    }
                    else
                    {
                        result = await helper.CallGetService(getImagesWithDatesURL + "?isthumb=true");
                    }

                    if (result == null)
                    {
                        Console.WriteLine("Error getting files");
                        return;
                    }
                    else
                        Console.WriteLine(result);
                    
                    dynamic linuxFiles = JsonConvert.DeserializeObject(result);

                    Dictionary<String, ImageFile> linuxImageDictionary = new Dictionary<string, ImageFile>();

                    foreach (dynamic file in linuxFiles)
                    {
                        String fileName = (String)file.name;
                        ImageFile f = new ImageFile() { Name = fileName, Date = (DateTime)file.date };
                        if (!linuxImageDictionary.ContainsKey(fileName))
                            linuxImageDictionary.Add(fileName, f);
                        //Console.WriteLine((String)file.name);
                        //Console.WriteLine((String)file.date);
                    }


                    string[] fileEntries = Directory.GetFiles(targetDirectory);
                    List<String> fileList = fileEntries.OrderBy(file => file).ToList();
                    Console.WriteLine("files count " + fileEntries.Length);
                    foreach (string fileName in fileList)
                    {
                        DateTime diskFileTime = File.GetLastWriteTime(fileName);

                        String fileNameNoPath = Path.GetFileName(fileName);

                        bool doUpdate = false;
                        bool isNew = false;
                        if (linuxImageDictionary.ContainsKey(fileNameNoPath))
                        {
                            if (linuxImageDictionary[fileNameNoPath].Date < diskFileTime)
                                doUpdate = true;
                        }
                        else
                        {
                            doUpdate = true;
                            newfileFound = true;
                            isNew = true;
                        }
                        if (doUpdate)
                            await ProcessFile(fileName, folder);
                        if (ConfigurationManager.AppSettings["DeleteFile"] == "true")
                        {
                            await DeleteAsync(fileName);
                            Console.WriteLine(fileName + " deleted");
                        }
                        if (doUpdate)
                        {
                            String add = null;
                            if (isNew)
                                add = " added";
                            else
                                add = " linux date " + linuxImageDictionary[fileNameNoPath].Date.ToLongDateString() + " windows date " + diskFileTime.ToLongDateString();
                            Console.WriteLine("updated " + fileName + add);
                        }

                    }
                    fileEntries = Directory.GetFiles(targetDirectory);
                    Console.WriteLine("files count after" + fileEntries.Length);

                    if (newfileFound)
                    {
                        await CallUpdateImages();
                    }
                    
                }
                Console.WriteLine("done");
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
