using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Data.SQLite;
using System.Data;

namespace HomeTaskService2
{
    public partial class Service1 : ServiceBase
    {
        static string ourDirectory = @"C:\Users\kosus\Desktop\";
        static Semaphore semForWrite = new Semaphore(1, 1);
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //C:\Windows\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe C:\Users\kosus\Desktop\HomeTaskService2\HomeTaskService2\bin\Debug\HomeTaskService2.exe
            //C:\Windows\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe C:\Users\kosus\Desktop\HomeTaskService2\HomeTaskService2\bin\Debug\HomeTaskService2.exe / u
            Write(DateTime.Now + "| Служба запущена;\n");
            Thread thread0 = new Thread(Thread0);
            thread0.Start();
        }

        protected override void OnStop()
        {
        }
        private void Thread0()
        {
            Write(DateTime.Now + "| Поток0 начал работу\n");

            Timer timer1 = new Timer(new TimerCallback(SHAREDWorker.SetCircle), "Process_2_start", 0, 1000);

            DateTime hisDateGet = DateTime.Now;

            DateTime hisDateStart = DateTime.Now;

            BoolAndString iGet = new BoolAndString("Process_2_get");
            Timer timer2 = new Timer(new TimerCallback(SHAREDWorker.SetCircleStop), iGet, 0, 1000);

            HelpSemaphore semCheckReady = new HelpSemaphore("Process_1_ready", DateTime.Now);
            Timer timer3 = new Timer(new TimerCallback(SHAREDWorker.CheckWithSemaphore), semCheckReady, 0, 1000);

            Semaphore endSemaphore = new Semaphore(0, 3);
            Semaphore start1Semaphore = new Semaphore(0, 1);
            Semaphore start2Semaphore = new Semaphore(0, 1);
            Semaphore start3Semaphore = new Semaphore(0, 1);

            List<NewsJson> newsVk1 = new List<NewsJson>();
            Thread thread1 = new Thread(() => JSONWorker.ReadData(ref newsVk1, ourDirectory + "1.json", endSemaphore, start1Semaphore));
            thread1.Start();
            Write(DateTime.Now + "| Поток1 создан и отправлен на запуск;\n");

            List<NewsJson> newsVk2 = new List<NewsJson>();
            Thread thread2 = new Thread(() => JSONWorker.ReadData(ref newsVk2, ourDirectory + "2.json", endSemaphore, start2Semaphore));
            thread2.Start();
            Write(DateTime.Now + "| Поток2 создан и отправлен на запуск;\n");

            List<NewsJson> newsVk3 = new List<NewsJson>();
            Thread thread3 = new Thread(() => JSONWorker.ReadData(ref newsVk3, ourDirectory + "3.json", endSemaphore, start3Semaphore));
            thread3.Start();
            Write(DateTime.Now + "| Поток3 создан и отправлен на запуск;\n");

            while (true)
            {
                Write(DateTime.Now + "| Началась новая итерация;\n" + new string('-', 45) + "\n");
                iGet.start = true;
                Write(DateTime.Now + "| Процесс 2 берёт управление;\n");
                if (SHAREDWorker.CheckOnce(ref hisDateGet, "Process_1_get"))
                {
                    Write(DateTime.Now + "| Процесс 1 занял файлы, ожидание передачи;\n");
                    iGet.start = false;
                    semCheckReady.waitOne();
                }
                iGet.start = true;
                hisDateStart = DateTime.Now;

                /*try
                {
                    string[] fileNames = SHAREDWorker.TakeFileName();
                    SHAREDWorker.TakeAndSetJsonFile(fileNames[0], directory);
                    SHAREDWorker.TakeAndSetJsonFile(fileNames[1], directory);
                    SHAREDWorker.TakeAndSetJsonFile(fileNames[2], directory);

                }
                catch (Exception exception)
                {
                    if (exception.HResult == -2147024894)
                    {
                        File.AppendAllText(directory + "Работа службы2.txt", DateTime.Now + "| Файлы от процесса 1 были утерены;\n");
                    }
                    File.AppendAllText(directory + "Работа службы2.txt", DateTime.Now + "| Ошибка:\n" + exception.ToString() + ";\n");
                    Stop();
                }
                File.AppendAllText(directory + "Работа службы2.txt", DateTime.Now + "| Данные получены;\n");*/
                start1Semaphore.Release();
                start2Semaphore.Release();
                start3Semaphore.Release();
                Write(DateTime.Now + "| Потоки получили разрешение на чтение;\n");
                endSemaphore.WaitOne();
                endSemaphore.WaitOne();
                endSemaphore.WaitOne();
                SQLiteConnection newsVkDb = new SQLiteConnection(@"Data Source=" + ourDirectory + "newsVk.db");
                newsVkDb.Open();
                Write(DateTime.Now + "| База данных создана;\n");
                SQLiteCommand cmdDb = newsVkDb.CreateCommand();
                cmdDb.CommandText = "DROP TABLE IF EXISTS news1;";
                cmdDb.ExecuteNonQuery();
                cmdDb.CommandText = "DROP TABLE IF EXISTS news2;";
                cmdDb.ExecuteNonQuery();
                cmdDb.CommandText = "DROP TABLE IF EXISTS news3;";
                cmdDb.ExecuteNonQuery();
                cmdDb.CommandText = "CREATE TABLE news1(Id STRING PRIMARY KEY, Text STRING);";
                cmdDb.ExecuteNonQuery();
                cmdDb.CommandText = "CREATE TABLE news2(Id STRING PRIMARY KEY, Photos STRING);";
                cmdDb.ExecuteNonQuery();
                cmdDb.CommandText = "CREATE TABLE news3(Id STRING PRIMARY KEY, Href STRING);";
                cmdDb.ExecuteNonQuery();
                foreach (NewsJson news in newsVk1)
                {
                    cmdDb.CommandText = "INSERT INTO news1(Id, Text) VALUES ( @Id , @Text)";
                    cmdDb.Parameters.Add("@Id", DbType.String).Value = news.Id;
                    cmdDb.Parameters.Add("@Text", DbType.String).Value = news.TextPhotosHref;
                    cmdDb.ExecuteNonQuery();
                }
                Write(DateTime.Now + "| Первая таблица записана;\n");
                foreach (NewsJson news in newsVk2)
                {
                    cmdDb.CommandText = "INSERT INTO news2(Id, Photos) VALUES( @Id , @Photos)";
                    cmdDb.Parameters.Add("@Id", DbType.String).Value = news.Id;
                    cmdDb.Parameters.Add("@Photos", DbType.String).Value = news.TextPhotosHref;
                    cmdDb.ExecuteNonQuery();
                }
                Write(DateTime.Now + "| Вторая таблица записана;\n");
                foreach (NewsJson news in newsVk3)
                {
                    cmdDb.CommandText = "INSERT INTO news3(Id, Href) VALUES( @Id , @Href)";
                    cmdDb.Parameters.Add("@Id", DbType.String).Value = news.Id;
                    cmdDb.Parameters.Add("@Href", DbType.String).Value = news.TextPhotosHref;
                    cmdDb.ExecuteNonQuery();
                }
                Write(DateTime.Now + "| Третья таблица записана;\n");
                newsVkDb.Dispose();

                if (SHAREDWorker.CheckOnce(ref hisDateStart, "Process_1_start"))
                {
                    iGet.start = false;
                    Write(DateTime.Now + "| Процесс 1 работает, передаём управление;\n");
                    DateTime dateReady2 = DateTime.Now;
                    int sizeReady2 = dateReady2.ToString().Length;
                    int capacity2 = sizeof(int) + sizeReady2 * sizeof(char);
                    MemoryMappedFile process2Ready = MemoryMappedFile.CreateOrOpen("Process_2_ready", capacity2);
                    process2Ready.CreateViewAccessor().Write(0, sizeReady2);
                    process2Ready.CreateViewAccessor().WriteArray(sizeof(int), dateReady2.ToString().ToCharArray(), 0, sizeReady2);
                    semCheckReady.lastDate = DateTime.Now;
                    semCheckReady.waitOne();
                    Write(DateTime.Now + "| Управление от 1го процесса получено;\n");
                }
                else
                {
                    Write(DateTime.Now + "| Процесс 1 не работает, управление не передать;\n");
                }
                hisDateGet = DateTime.Now;
            }
        }
        public static void Write(string text)
        {
            semForWrite.WaitOne();
            File.AppendAllText(ourDirectory + "Работа службы2.txt", text);
            semForWrite.Release();
        }
    }
}
