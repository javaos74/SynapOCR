using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Http;
using System.Globalization;
using System.IO;
using System.Activities;

namespace UiPathTeams.Synap.OCR
{
    public class SynapOCRPair
    {
        public HttpStatusCode status { get; set; }
        public string body { get; set; }
    }
    class UiPathHttpClient
    {
        public string ApiKey { get; set; }

        public UiPathHttpClient() :
            this("https://ailab.synap.co.kr")
        { 
        }
        public UiPathHttpClient( string server)
        {
            this.url = server + "/sdk/ocr";
            this.client = new HttpClient();
            this.content = new MultipartFormDataContent("synap----" + DateTime.Now.ToString(CultureInfo.InvariantCulture));
        }

        public void setEndpoint( string server)
        {
            if (!string.IsNullOrEmpty(server))
            {
                if (server.EndsWith("/"))
                {
                    this.url = server + "sdk/ocr";
                }
                else
                {
                    this.url = server + "/sdk/ocr";
                }
            }
        }

        public void AddFile(string fileName)
        {
            var fstream = System.IO.File.OpenRead(fileName);
            byte[] buf = new byte[fstream.Length];
            int read_bytes = 0;
            int offset = 0;
            int remains = (int)fstream.Length;
            do {
                read_bytes += fstream.Read(buf, offset, remains);
                offset += read_bytes;
                remains -= read_bytes;
            } while (remains != 0);

            this.content.Add(new StreamContent(new MemoryStream(buf)), "image", System.IO.Path.GetFileNameWithoutExtension(fileName));
        }
        public void AddField( string name, string value)
        {
            this.content.Add(new StringContent(value), name);
        }

        public void Clear()
        {
            this.content.Dispose();
        }

        public async Task<SynapOCRPair> Upload()
        {
#if DEBUG
            Console.WriteLine("http content count :" + this.content.Count());
#endif
            using (var message = this.client.PostAsync(this.url, this.content))
            {
                SynapOCRPair resp = new SynapOCRPair();
                resp.status = message.Result.StatusCode;
                resp.body = await message.Result.Content.ReadAsStringAsync();
                return resp;
            }
        }

        private HttpClient client;
        private string url;
        private MultipartFormDataContent content;
    }
}
