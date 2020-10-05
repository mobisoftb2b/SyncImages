using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SyncImages
{
    class NetworkHelpers
    {
        public String PostJSON(String baseAddress, string JSONString) {
            try
            {
                var http = (System.Net.HttpWebRequest)WebRequest.Create(new Uri(baseAddress));
                http.Accept = "application/json";
                http.ContentType = "application/json";
                http.Method = "POST";

                string parsedContent = JSONString;
                ASCIIEncoding encoding = new ASCIIEncoding();
                Byte[] bytes = encoding.GetBytes(parsedContent);

                Stream newStream = http.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Close();

                var response = http.GetResponse();

                var stream = response.GetResponseStream();
                var sr = new StreamReader(stream);
                var content = sr.ReadToEnd();

                return content;
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public async Task<string> CallGetService(String url)
        {
            var client = new WebClient();
            if (url.StartsWith("https"))
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }
            string result = await client.DownloadStringTaskAsync(url);
            return result;
        }

        public async Task<string> CallPostService(String url, string JSONString)
        {
            try
            {
                Task<string> result = null;
                HttpClient client = new HttpClient();
                if (url.StartsWith("https"))
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                }


                var content = new StringContent(JSONString, Encoding.UTF8, "application/json");
                HttpResponseMessage postResult = await client.PostAsync(url, content);

                if (postResult.StatusCode == HttpStatusCode.OK)
                    result = postResult.Content.ReadAsStringAsync();
                Console.WriteLine(result.Result);
                return result.Result;
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
            return null;
        }
    }
}
