using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Threading;

namespace HomeTaskService2
{
    internal class JSONWorker
    {
        public static void ReadData(ref List<NewsJson> newsVk, string fileName, Semaphore semaphoreOut, Semaphore semaphoreIn)
        {
            while (true)
            {
                semaphoreIn.WaitOne();
                newsVk.Clear();
                if (File.Exists(fileName))
                {
                    DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(List<NewsJson>));
                    using (FileStream fileStream = new FileStream(fileName, FileMode.Open))
                    {
                        newsVk = (List<NewsJson>)dataContractJsonSerializer.ReadObject(fileStream);
                    }
                    Service1.Write(DateTime.Now + "| Данные из файла " + fileName + " прочитаны;\n");
                }
                else
                {
                    Service1.Write(DateTime.Now + " | Файла " + fileName + "не возможно открыть, данные не прочитаны;\n");
                }
                semaphoreOut.Release();
            }
        }
        /*public static void SetData(string fileName, List<NewsJson> newsJson)
        {
            DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(List<NewsJson>));
            using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
            {
                dataContractJsonSerializer.WriteObject(fileStream, newsJson);
            }
        }*/
    }
}
