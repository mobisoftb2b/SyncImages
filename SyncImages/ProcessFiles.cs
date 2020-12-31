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
using NLog;

namespace SyncImages
{
    public class ImageFile { 
        public String Name { get; set; }
        public DateTime Date { get; set; }
    }


    public class ProcessFiles
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        NetworkHelpers helper = new NetworkHelpers();

        static Task<int> CallBatchNoArgsAsync(string batch)
        {
            var tcs = new TaskCompletionSource<int>();
            try
            {
                var process = new Process
                {
                    StartInfo = { FileName = batch,
                    WindowStyle = ProcessWindowStyle.Hidden
                    },
                    EnableRaisingEvents = true

                };

                process.Exited += (sender, args) =>
                {
                    logger.Info($"Batch {batch} ExitCode {process.ExitCode}");
                    tcs.SetResult(process.ExitCode);
                    process.Dispose();
                };

                process.Start();

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Console.WriteLine(ex.Message);
            }
            return tcs.Task;
        }
        static Task<int> CallCopyImagesBatchAsync (string imageName, string folder, string batch)
        {
            var tcs = new TaskCompletionSource<int>();
            try
            {
                var argsProcess = string.Format("{0} {1}", imageName, folder);
                logger.Info(argsProcess);
                var process = new Process
                {
                    StartInfo = { FileName = batch,
                  WindowStyle = ProcessWindowStyle.Hidden,
                  Arguments = argsProcess},
                    EnableRaisingEvents = true

                };

                process.Exited += (sender, args) =>
                {
                    logger.Info($"Args {args} ExitCode {process.ExitCode}");
                    tcs.SetResult(process.ExitCode);
                    process.Dispose();
                };

                process.Start();

                
            }
            catch (Exception ex) {
                logger.Error(ex);
                Console.WriteLine(ex.Message);
            }
            return tcs.Task;
        }
        
      
        async Task<int> ProcessFile(String imagename, String folder, String batch) {
            Console.WriteLine(imagename);
            return await CallCopyImagesBatchAsync(imagename, folder, batch);
        }

        public static Task DeleteAsync(string path)
        {
             return Task.Run(() => { 
                 if (File.Exists(path))
                 { 
                   File.Delete(path);
                   logger.Info("Deleting " + path);
                   System.Threading.Thread.Sleep(100); 
                 }
             });
        }

        async public Task CallUpdateImages() {
            String UpdateImagesURL = ConfigurationManager.AppSettings["UpdateImagesURL"];
            String result1 = await helper.CallGetService(UpdateImagesURL);
            Console.WriteLine("UpdateImageService " + result1);
        }

        async public Task PreProcessImages() {
            String prebatch = ConfigurationManager.AppSettings["PreBatch"];
            if (!String.IsNullOrEmpty(prebatch))
            {
                await CallBatchNoArgsAsync(prebatch);
            }
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
                    String batch = ConfigurationManager.AppSettings["Batch"];
                    String updateFile = ConfigurationManager.AppSettings["UpdateFile"];
                    
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
                        logger.Info("isthumb");
                        result = await helper.CallGetService(getImagesWithDatesURL + "?isthumb=true");
                    }
                    else
                    {
                        logger.Info("not thumb");
                        result = await helper.CallGetService(getImagesWithDatesURL + "?isthumb=true");
                    }

                    if (result == null)
                    {
                        logger.Info("Error getting files");
                        Console.WriteLine("Error getting files");
                        return;
                    }
                    else
                    {
                        logger.Info(result);
                        Console.WriteLine(result);
                    }
                    
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
                    logger.Info("files count " + fileEntries.Length);
                    Console.WriteLine("files count " + fileEntries.Length);
                    foreach (string fileName in fileList)
                    {
                        DateTime diskFileTime = File.GetLastWriteTime(fileName);

                        String fileNameNoPath = Path.GetFileName(fileName);
                        //linux does not like spaces in names
                        //do not copy gibberish names and (1), (2) images copies
                        if (fileNameNoPath.Contains('?') || fileNameNoPath.Contains('(') || fileNameNoPath.Contains(' '))
                        {
                            Console.WriteLine($"file {fileNameNoPath} is not going to be copied");
                            logger.Info($"file {fileNameNoPath} is not going to be copied");
                            continue;
                        }
                        bool doUpdate = false;
                        bool isNew = false;
                        if (linuxImageDictionary.ContainsKey(fileNameNoPath))
                        {
                            if (updateFile == "true" && DateTime.Compare(linuxImageDictionary[fileNameNoPath].Date, diskFileTime) < 0 && linuxImageDictionary[fileNameNoPath].Date.ToLongDateString() != diskFileTime.ToLongDateString())
                                doUpdate = true;
                        }
                        else
                        {
                            doUpdate = true;
                            newfileFound = true;
                            isNew = true;
                        }
                        if (doUpdate)
                            await ProcessFile(fileName, folder, batch);
                        if (ConfigurationManager.AppSettings["DeleteFile"] == "true")
                        {
                            await DeleteAsync(fileName);
                            logger.Info(fileName + " deleted");
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
                            logger.Info("updated " + fileName + add);
                        }

                    }
                    fileEntries = Directory.GetFiles(targetDirectory);
                    Console.WriteLine("files count after" + fileEntries.Length);
                    logger.Info("files count after" + fileEntries.Length);

                    if (newfileFound)
                    {
                        await CallUpdateImages();
                    }
                    
                }
                logger.Info("done");
                Console.WriteLine("done");
            }
            catch (Exception ex) {
                logger.Error(ex);
                Console.WriteLine(ex.Message);
            }
        }
    }
}
