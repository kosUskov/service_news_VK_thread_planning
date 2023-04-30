using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Threading;

namespace HomeTaskService1
{
    internal class JSONWorker
    {
        public static void SetData(ref List<NewsVk> newsVk, string fileName, Semaphore semaphoreOut, Semaphore semaphoreIn)
        {
            List<NewsJson> newsVkFile = new List<NewsJson>();
            while (true)
            {
                semaphoreIn.WaitOne();
                newsVkFile.Clear();
                if (File.Exists(fileName))
                {
                    DataContractJsonSerializer dataContractJsonSerializer1 = new DataContractJsonSerializer(typeof(List<NewsJson>));
                    using (FileStream fileStream = new FileStream(fileName, FileMode.Open))
                    {
                        newsVkFile = (List<NewsJson>)dataContractJsonSerializer1.ReadObject(fileStream);
                    }
                }
                foreach (NewsVk news in newsVk)
                {
                    if (newsVkFile.Count != 0)
                    {
                        /*if (news.Id.ToString().Contains("feed_repost"))
                        { //у перепосщенных записей полный id не уникален
                            if (newsVkFile.Exists(newsFile => newsFile.Id.Contains(news.Id.Remove(news.Id.Length - 6, 6))))
                            {
                                continue;
                            }
                        }
                        else*/
                        {
                            if (newsVkFile.Exists(newsFile => newsFile.Id == news.Id))
                            {
                                continue;
                            }
                        }
                    }
                    newsVkFile.Add(new NewsJson(news, fileName));
                }
                DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(List<NewsJson>));
                using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
                {
                    dataContractJsonSerializer.WriteObject(fileStream, newsVkFile);
                }
                Service1.Write(DateTime.Now + "| Данные в файл " + fileName + " записаны;\n");
                semaphoreOut.Release();
            }
        }
        public static void ReadData(string directory, Semaphore semaphoreOut, Semaphore semaphoreIn)
        {
            List<NewsJson> newsJson = new List<NewsJson>();
            while (true)
            {
                semaphoreIn.WaitOne();
                newsJson.Clear();
                if (File.Exists(directory + "1.JSON"))
                {
                    DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(List<NewsJson>));
                    using (FileStream fileStream = new FileStream(directory + "1.JSON", FileMode.Open))
                    {
                        newsJson = (List<NewsJson>)dataContractJsonSerializer.ReadObject(fileStream);
                    }
                    Service1.Write(DateTime.Now + "| Данные из файла 1.JSON прочитаны;\n");
                }
                semaphoreOut.Release();
                semaphoreIn.WaitOne();
                newsJson.Clear();
                if (File.Exists(directory + "2.JSON"))
                {
                    DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(List<NewsJson>));
                    using (FileStream fileStream = new FileStream(directory + "2.JSON", FileMode.Open))
                    {
                        newsJson = (List<NewsJson>)dataContractJsonSerializer.ReadObject(fileStream);
                    }
                    Service1.Write(DateTime.Now + "| Данные из файла 2.JSON прочитаны;\n");
                }
                semaphoreOut.Release();
                semaphoreIn.WaitOne();
                newsJson.Clear();
                if (File.Exists(directory + "3.JSON"))
                {
                    DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(List<NewsJson>));
                    using (FileStream fileStream = new FileStream(directory + "3.JSON", FileMode.Open))
                    {
                        newsJson = (List<NewsJson>)dataContractJsonSerializer.ReadObject(fileStream);
                    }
                    Service1.Write(DateTime.Now + "| Данные из файла 3.JSON прочитаны;\n");
                }
                semaphoreOut.Release();
            }
        }
        /*public static void ReadDataFrom1File(ref List<NewsJson> newsVk1, string fileName)
        {
            if (File.Exists(fileName))
            {
                DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(List<NewsJson>));
                using (FileStream fileStream = new FileStream(fileName, FileMode.Open))
                {
                    newsVk1 = (List<NewsJson>)dataContractJsonSerializer.ReadObject(fileStream);
                }
            }
        }*/
    }
}
