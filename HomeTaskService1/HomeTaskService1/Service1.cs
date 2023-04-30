using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace HomeTaskService1
{
    public partial class Service1 : ServiceBase
    {
        ChromeDriver chromeDriver;
        static string ourDirectory = @"C:\Users\kosus\Desktop\";
        static Semaphore semForWrite = new Semaphore(1, 1);
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Write(DateTime.Now + "| Служба запущена;\n");
            //C:\Windows\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe C:\Users\kosus\Desktop\HomeTaskService1\HomeTaskService1\bin\Debug\HomeTaskService1.exe
            //C:\Windows\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe C:\Users\kosus\Desktop\HomeTaskService1\HomeTaskService1\bin\Debug\HomeTaskService1.exe /u
            Thread thread0 = new Thread(Thread0);
            thread0.Start();
        }

        protected override void OnStop()
        {
            chromeDriver.Quit();
        }
        private void Thread0()
        {
            Write(DateTime.Now + "| Поток0 начал работу;\n");

            Timer timer1 = new Timer(new TimerCallback(SHAREDWorker.SetCircle), "Process_1_start", 0, 1000);

            DateTime hisDateGet = DateTime.Now;

            DateTime hisDateStart = DateTime.Now;

            BoolAndString iGet = new BoolAndString("Process_1_get");
            Timer timer2 = new Timer(new TimerCallback(SHAREDWorker.SetCircleStop), iGet, 0, 1000);

            HelpSemaphore semCheckReady = new HelpSemaphore("Process_2_ready", DateTime.Now);
            Timer timer3 = new Timer(new TimerCallback(SHAREDWorker.CheckWithSemaphore), semCheckReady, 0, 1000);

            HelpSemaphore semCheckGet = new HelpSemaphore("Process_2_get", DateTime.Now);
            Timer timer4 = new Timer(new TimerCallback(SHAREDWorker.CheckWithSemaphore), semCheckGet, 0, 4000);

            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument(@"user-data-dir=C:\Users\kosus\AppData\Local\Google\Chrome\User Data");
            //chromeOptions.AddArgument(@"user-data-dir=C:\Users\student\AppData\Local\Google\Chrome\User Data");
            try
            {
                chromeDriver = new ChromeDriver(chromeOptions);
            }
            catch (Exception exception)
            {
                Write(DateTime.Now + "| Ошибка:\n" + exception.ToString() + ";\n");
                Stop();
            }
            int countPlane = 0;
            Semaphore endSemaphore = new Semaphore(0, 3);
            Semaphore startSemaphore1 = new Semaphore(0, 1);
            Semaphore startSemaphore2 = new Semaphore(0, 1);
            Semaphore startSemaphore3 = new Semaphore(0, 1);
            Semaphore startSemaphore4 = new Semaphore(0, 1);
            /*
            HelpSharedName json1Name = new HelpSharedName("1.JSON");
            HelpSharedName json2Name = new HelpSharedName("2.JSON");
            HelpSharedName json3Name = new HelpSharedName("3.JSON");*/

            List<NewsVk> newsVk = new List<NewsVk>();
            Thread thread1 = new Thread(() => JSONWorker.SetData(ref newsVk, ourDirectory + "1.json", endSemaphore, startSemaphore1));
            thread1.Start();
            Write(DateTime.Now + "| Поток1 создан и отправлен на запуск;\n");

            Thread thread2 = new Thread(() => JSONWorker.SetData(ref newsVk, ourDirectory + "2.json", endSemaphore, startSemaphore2));
            thread2.Start();
            Write(DateTime.Now + "| Поток2 создан и отправлен на запуск;\n");

            Thread thread3 = new Thread(() => JSONWorker.SetData(ref newsVk, ourDirectory + "3.json", endSemaphore, startSemaphore3));
            thread3.Start();
            Write(DateTime.Now + "| Поток3 создан и отправлен на запуск;\n");

            Thread thread4 = new Thread(() => JSONWorker.ReadData(ourDirectory, endSemaphore, startSemaphore4));
            thread4.Start();
            Write(DateTime.Now + "| Поток4 создан и отправлен на запуск;\n");

            int countError = 0;
            while (true)
            {
                if (countError > 100)
                {
                    Stop();
                }
                Write(DateTime.Now + "| Началась новая итерация;\n" + new string('-', 45) + "\n");
                iGet.start = true;
                Write(DateTime.Now + "| Процесс 1 берёт управление;\n");
                if (SHAREDWorker.CheckOnce(ref hisDateGet, "Process_2_get"))
                {
                    Write(DateTime.Now + "| Процесс 2 занял файлы, ожидание передачи;\n");
                    semCheckGet.waitOne();
                }
                hisDateStart = DateTime.Now;

                newsVk.Clear();
                try
                {
                    chromeDriver.Navigate().GoToUrl("https://vk.com/feed");
                }
                catch (Exception exception)
                {
                    countError++;
                    Write(DateTime.Now + "| Ошибка:\n" + exception.ToString() + ";\n");
                    continue;
                }
                List<IWebElement> webElements;
                try
                {
                    webElements = chromeDriver.FindElements(By.ClassName("feed_row")).ToList();
                }
                catch (Exception exception)
                {
                    countError++;
                    Write(DateTime.Now + "| Ошибка:\n" + exception.ToString() + ";\n");
                    continue;
                }
                Write(DateTime.Now + "| Новости получены (" + webElements.Count.ToString() + ");\n");
                countError = 0;
                foreach (IWebElement item in webElements)
                {
                    NewsVk news = new NewsVk();
                    IWebElement newItem;
                    try
                    {
                        newItem = item.FindElement(By.TagName("div"));
                    }
                    catch (Exception exception)
                    {
                        Write(DateTime.Now + "| Ошибка " + exception.ToString() + "; ");
                        continue;
                    }
                    if (!newItem.Displayed)
                    {
                        continue;
                    }
                    if (newItem.GetAttribute("data-ad-block-uid") != null)
                    { //реклама
                        continue;
                    }
                    string tmp = newItem.GetAttribute("class");
                    if (tmp == null || tmp.Contains("feed_friends_recomm") || tmp.Contains("feed_groups_recomm"))
                    {
                        continue;
                    }
                    tmp = newItem.GetAttribute("id");
                    if (tmp == null || !tmp.Contains("post") || tmp.Contains("postadsite"))
                    {
                        continue;
                    }
                    news.Id = tmp;
                    try
                    {//к этому моменту все новости должны быть правильными
                        newItem = item.FindElement(By.ClassName("wall_text"));
                    }
                    catch (Exception exception)
                    {
                        Write(DateTime.Now + "| Ошибка:\n" + exception.ToString() + ";\n");
                        continue;
                    }
                    news.Text = newItem.Text;
                    List<IWebElement> newItems;
                    try
                    {
                        newItems = newItem.FindElements(By.TagName("a")).ToList();
                    }
                    catch (Exception exception)
                    {
                        Write(DateTime.Now + "| Ошибка:\n" + exception.ToString() + ";\n");
                        continue;
                    }
                    string Photo = "";
                    foreach (IWebElement photo in newItems)
                    {
                        string str = photo.GetAttribute("onclick");
                        if (str == null || !str.Contains(".jpg"))
                        {
                            continue;
                        }
                        if (str.Contains("\"w\":\"https:"))
                        { //хочу взять фото наибольшего расширения
                            Photo += str.Substring(str.IndexOf("\"w\":\"https:"), str.IndexOf(",\"x_\"") - str.IndexOf("\"w\":\"https:")).Remove(0, 4);
                        }
                        else
                        { //это по меньше
                            if (str.Contains("\"z\":\"https:"))
                            {
                                Photo += str.Substring(str.IndexOf("\"z\":\"https:"), str.IndexOf(",\"x_\"") - str.IndexOf("\"z\":\"https:")).Remove(0, 4);
                            }
                            else
                            { //ещё меньше
                                if (str.Contains("\"y\":\"https:"))
                                {
                                    Photo += str.Substring(str.IndexOf("\"y\":\"https:"), str.IndexOf(",\"x_\"") - str.IndexOf("\"y\":\"https:")).Remove(0, 4);
                                }
                                else
                                { //минимум, который всегда есть
                                    Photo += str.Substring(str.IndexOf("\"x\":\"https:"), str.IndexOf(",\"x_\"") - str.IndexOf("\"x\":\"https:")).Remove(0, 4);
                                }
                            }
                        }
                        Photo += "; ";
                    }
                    news.Photos = Photo;
                    string Href = "";
                    foreach (IWebElement href in newItems)
                    {
                        string str = href.GetAttribute("href");
                        if (str == null || str.Contains(@"https:\/\/vk.com\/photo") || Href.Contains(str))
                        {
                            continue;
                        }
                        Href += str + "; ";
                    }
                    news.Href = Href;
                    newsVk.Add(news);
                }

                Write(DateTime.Now + "| Новости обработаны, осталось " + newsVk.Count + ";\n");
                if (countPlane != 1)
                {
                    startSemaphore1.Release();
                    Write(DateTime.Now + "| Поток1 получил разрешение на запись;\n");
                }
                else
                {
                    startSemaphore4.Release();
                    Write(DateTime.Now + "| Поток4 получил разрешение на чтение 1.JSON;\n");
                }
                if (countPlane != 2)
                {
                    startSemaphore2.Release();
                    Write(DateTime.Now + "| Поток2 получил разрешение на запись;\n");
                }
                else
                {
                    startSemaphore4.Release();
                    Write(DateTime.Now + "| Поток4 получил разрешение на чтение 2.JSON;\n");
                }
                if (countPlane != 3)
                {
                    startSemaphore3.Release();
                    Write(DateTime.Now + "| Поток3 получил разрешение на запись;\n");
                }
                else
                {
                    startSemaphore4.Release();
                    Write(DateTime.Now + "| Поток4 получил разрешение на чтение 3.JSON;\n");
                }
                endSemaphore.WaitOne();
                endSemaphore.WaitOne();
                endSemaphore.WaitOne();
                if (countPlane == 3)
                {
                    countPlane = 0;
                }
                else
                {
                    countPlane++;
                }
                Write(DateTime.Now + "| Новости дозаписаны/прочитаны в/из JSON файлы;\n");
                if (SHAREDWorker.CheckOnce(ref hisDateStart, "Process_2_start"))
                {
                    //Я воспринял передачу файлов буквально
                    //однако если создавать копии, то смысла в
                    //планировании нет. Т.е. процессы видят друг
                    //друга, а общая память используется, как
                    //разделяемая для исключения ошибок доступа
                    {/*старая версия
                    List<NewsVk1> newsVk1 = new List<NewsVk1>();
                    Thread thread1 = new Thread(() => JSONWorker.readFile1(newsVk1, semaphore));
                    thread1.Start(semaphore);

                    List<NewsVk2> newsVk2 = new List<NewsVk2>();
                    Thread thread2 = new Thread(() => JSONWorker.readFile1(newsVk1, semaphore));
                    thread2.Start(semaphore);

                    List<NewsVk3> newsVk3 = new List<NewsVk3>();
                    Thread thread3 = new Thread(() => JSONWorker.readFile1(newsVk1, semaphore));
                    thread3.Start(semaphore);

                    semaphore.WaitOne();
                    semaphore.WaitOne();
                    semaphore.WaitOne();
                    
                        {
                            long capacity = sizeof(int);
                            foreach (NewsVk1 news in newsVk1)
                            {
                                capacity += sizeof(int) + sizeof(char) * news.Id.Length;
                            }
                            try
                            {
                                memoryMappedFile = MemoryMappedFile.CreateNew("1_id_JSON", capacity);
                            }
                            catch
                            {
                                memoryMappedFile = MemoryMappedFile.OpenExisting("1_id_JSON");
                            }
                            using (MemoryMappedViewAccessor memoryMappedViewAccessor = memoryMappedFile.CreateViewAccessor(0, sizeof(int)))
                            {
                                memoryMappedViewAccessor.Write(0, newsVk1.Count);
                            }
                            long k = sizeof(int);
                            foreach (NewsVk1 news in newsVk1) {
                                using (MemoryMappedViewAccessor memoryMappedViewAccessor = memoryMappedFile.CreateViewAccessor(k, sizeof(int) + sizeof(char) * news.Id.Length))
                                {
                                    memoryMappedViewAccessor.Write(0, news.Id.Length);
                                    memoryMappedViewAccessor.WriteArray(sizeof(int), news.Id.ToCharArray(), 0, news.Id.Length);
                                }
                                k += sizeof(int) + sizeof(char) * news.Id.Length;
                            }
                        }
                        {
                            long capacity = sizeof(int);
                            foreach (NewsVk1 news in newsVk1)
                            {
                                capacity += sizeof(int) + sizeof(char) * news.Text.Length;
                            }
                            try
                            {
                                memoryMappedFile = MemoryMappedFile.CreateNew("1_text_JSON", capacity);
                            }
                            catch
                            {
                                memoryMappedFile = MemoryMappedFile.OpenExisting("1_text_JSON");
                            }
                            using (MemoryMappedViewAccessor memoryMappedViewAccessor = memoryMappedFile.CreateViewAccessor(0, sizeof(int)))
                            {
                                memoryMappedViewAccessor.Write(0, newsVk1.Count);
                            }
                            long k = sizeof(int);
                            foreach (NewsVk1 news in newsVk1)
                            {
                                using (MemoryMappedViewAccessor memoryMappedViewAccessor = memoryMappedFile.CreateViewAccessor(k, sizeof(int) + sizeof(char) * news.Text.Length))
                                {
                                    memoryMappedViewAccessor.Write(0, news.Text.Length);
                                    memoryMappedViewAccessor.WriteArray(sizeof(int), news.Text.ToCharArray(), 0, news.Text.Length);
                                }
                                k += sizeof(int) + sizeof(char) * news.Text.Length;
                            }
                        }

                        {
                            long capacity = sizeof(int);
                            foreach (NewsVk2 news in newsVk2)
                            {
                                capacity += sizeof(int) + sizeof(char) * news.Id.Length;
                            }
                            try
                            {
                                memoryMappedFile = MemoryMappedFile.CreateNew("2_id_JSON", capacity);
                            }
                            catch
                            {
                                memoryMappedFile = MemoryMappedFile.OpenExisting("2_id_JSON");
                            }
                            using (MemoryMappedViewAccessor memoryMappedViewAccessor = memoryMappedFile.CreateViewAccessor(0, sizeof(int)))
                            {
                                memoryMappedViewAccessor.Write(0, newsVk2.Count);
                            }
                            long k = sizeof(int);
                            foreach (NewsVk2 news in newsVk2)
                            {
                                using (MemoryMappedViewAccessor memoryMappedViewAccessor = memoryMappedFile.CreateViewAccessor(k, sizeof(int) + sizeof(char) * news.Id.Length))
                                {
                                    memoryMappedViewAccessor.Write(0, news.Id.Length);
                                    memoryMappedViewAccessor.WriteArray(sizeof(int), news.Id.ToCharArray(), 0, news.Id.Length);
                                }
                                k += sizeof(int) + sizeof(char) * news.Id.Length;
                            }
                        }
                        {
                            long capacity = sizeof(int);
                            foreach (NewsVk2 news in newsVk2)
                            {
                                capacity += sizeof(int);
                                foreach (string photo in news.Photos)
                                {
                                    capacity += sizeof(int) + sizeof(char) * photo.Length;
                                }
                            }
                            try
                            {
                                memoryMappedFile = MemoryMappedFile.CreateNew("2_photos_JSON", capacity);
                            }
                            catch
                            {
                                memoryMappedFile = MemoryMappedFile.OpenExisting("2_photos_JSON");
                            }
                            using (MemoryMappedViewAccessor memoryMappedViewAccessor = memoryMappedFile.CreateViewAccessor(0, sizeof(int)))
                            {
                                memoryMappedViewAccessor.Write(0, newsVk2.Count);
                            }
                            long k = sizeof(int);
                            foreach (NewsVk2 news in newsVk2)
                            {
                                using (MemoryMappedViewAccessor memoryMappedViewAccessor = memoryMappedFile.CreateViewAccessor(k, sizeof(int)))
                                {
                                    memoryMappedViewAccessor.Write(0, news.Photos.Length);
                                }
                                k += sizeof(int);
                                foreach (string photo in news.Photos)
                                {
                                    using (MemoryMappedViewAccessor memoryMappedViewAccessor = memoryMappedFile.CreateViewAccessor(k, sizeof(int) + sizeof(char) * photo.Length))
                                    {
                                        memoryMappedViewAccessor.Write(0, photo.Length);
                                        memoryMappedViewAccessor.WriteArray(sizeof(int), photo.ToCharArray(), 0, photo.Length);
                                    }
                                    k += sizeof(int) + sizeof(char) * photo.Length;
                                }
                            }
                        }

                        {
                            long capacity = sizeof(int);
                            foreach (NewsVk3 news in newsVk3)
                            {
                                capacity += sizeof(int) + sizeof(char) * news.Id.Length;
                            }
                            try
                            {
                                memoryMappedFile = MemoryMappedFile.CreateNew("3_id_JSON", capacity);
                            }
                            catch
                            {
                                memoryMappedFile = MemoryMappedFile.OpenExisting("3_id_JSON");
                            }
                            using (MemoryMappedViewAccessor memoryMappedViewAccessor = memoryMappedFile.CreateViewAccessor(0, sizeof(int)))
                            {
                                memoryMappedViewAccessor.Write(0, newsVk3.Count);
                            }
                            long k = sizeof(int);
                            foreach (NewsVk3 news in newsVk3)
                            {
                                using (MemoryMappedViewAccessor memoryMappedViewAccessor = memoryMappedFile.CreateViewAccessor(k, sizeof(int) + sizeof(char) * news.Id.Length))
                                {
                                    memoryMappedViewAccessor.Write(0, news.Id.Length);
                                    memoryMappedViewAccessor.WriteArray(sizeof(int), news.Id.ToCharArray(), 0, news.Id.Length);
                                }
                                k += sizeof(int) + sizeof(char) * news.Id.Length;
                            }
                        }
                        {
                            long capacity = sizeof(int);
                            foreach (NewsVk3 news in newsVk3)
                            {
                                capacity += sizeof(int);
                                foreach (string hr in news.Href)
                                {
                                    capacity += sizeof(int) + sizeof(char) * hr.Length;
                                }
                            }
                            try
                            {
                                memoryMappedFile = MemoryMappedFile.CreateNew("3_href_JSON", capacity);
                            }
                            catch
                            {
                                memoryMappedFile = MemoryMappedFile.OpenExisting("3_href_JSON");
                            }
                            using (MemoryMappedViewAccessor memoryMappedViewAccessor = memoryMappedFile.CreateViewAccessor(0, sizeof(int)))
                            {
                                memoryMappedViewAccessor.Write(0, newsVk3.Count);
                            }
                            long k = sizeof(int);
                            foreach (NewsVk3 news in newsVk3)
                            {
                                using (MemoryMappedViewAccessor memoryMappedViewAccessor = memoryMappedFile.CreateViewAccessor(k, sizeof(int)))
                                {
                                    memoryMappedViewAccessor.Write(0, news.Href.Length);
                                }
                                k += sizeof(int);
                                foreach (string hr in news.Href)
                                {
                                    using (MemoryMappedViewAccessor memoryMappedViewAccessor = memoryMappedFile.CreateViewAccessor(k, sizeof(int) + sizeof(char) * hr.Length))
                                    {
                                        memoryMappedViewAccessor.Write(0, hr.Length);
                                        memoryMappedViewAccessor.WriteArray(sizeof(int), hr.ToCharArray(), 0, hr.Length);
                                    }
                                    k += sizeof(int) + sizeof(char) * hr.Length;
                                }
                            }
                        }
                    */
                    }
                    /*
                    //Ограничений на создание новых поток не было
                    Semaphore semaphore = new Semaphore(0, 3);
                    Thread threadShared1 = new Thread(() => SHAREDWorker.AddJsonFile(directory, json1Name, semaphore));
                    threadShared1.Start();
                    Thread threadShared2 = new Thread(() => SHAREDWorker.AddJsonFile(directory, json2Name, semaphore));
                    threadShared2.Start();
                    Thread threadShared3 = new Thread(() => SHAREDWorker.AddJsonFile(directory, json3Name, semaphore));
                    threadShared3.Start();
                    semaphore.WaitOne();
                    semaphore.WaitOne();
                    semaphore.WaitOne();
                    if (stop)
                    {
                        Stop();
                    }
                    if (!SHAREDWorker.AddFileName(new[] { json1Name.GetFileName(), json2Name.GetFileName(), json3Name.GetFileName() }))
                    {
                        Stop();
                    }
                    */
                    iGet.start = false;
                    Write(DateTime.Now + "| Процесс 2 работает, передаём управление;\n");
                    MemoryMappedFile process1Ready;
                    DateTime dateReady1 = DateTime.Now;
                    int sizeReady1 = dateReady1.ToString().Length;
                    int capacity2 = sizeof(int) + sizeReady1 * sizeof(char);
                    process1Ready = MemoryMappedFile.CreateOrOpen("Process_1_ready", capacity2);
                    process1Ready.CreateViewAccessor().Write(0, sizeReady1);
                    process1Ready.CreateViewAccessor().WriteArray(sizeof(int), dateReady1.ToString().ToCharArray(), 0, sizeReady1);
                    semCheckReady.lastDate = DateTime.Now;
                    semCheckReady.waitOne();
                    Write(DateTime.Now + "| Управление от 2го процесса получено;\n");
                }
                else
                {
                    Write(DateTime.Now + "| Процесс 2 не работает;\n");
                }
                hisDateGet = DateTime.Now;
            }
        }
        public static void Write(string text)
        {//https://i.ibb.co/193DXmp/image.png
         //и ещё теперь можно спокойно записывать "параллельно"
            semForWrite.WaitOne();
            File.AppendAllText(ourDirectory + "Работа службы1.txt", text);
            semForWrite.Release();
        }
    }
}
