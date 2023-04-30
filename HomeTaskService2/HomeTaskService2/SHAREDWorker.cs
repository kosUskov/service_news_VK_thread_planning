using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.MemoryMappedFiles;

namespace HomeTaskService2
{
    internal class SHAREDWorker
    {/*
        public static string[] TakeFileName()
        {
            long position = 0;
            MemoryMappedFile memoryMappedFile = MemoryMappedFile.OpenExisting("fileNames");
            int size1 = memoryMappedFile.CreateViewAccessor().ReadInt32(position);
            position += sizeof(int);
            char[] name1 = new char[size1];
            memoryMappedFile.CreateViewAccessor().ReadArray(position, name1, 0, size1);
            position += sizeof(char) * size1;
            int size2 = memoryMappedFile.CreateViewAccessor().ReadInt32(position);
            position += sizeof(int);
            char[] name2 = new char[size2];
            memoryMappedFile.CreateViewAccessor().ReadArray(position, name2, 0, size2);
            position += sizeof(char) * size2;
            int size3 = memoryMappedFile.CreateViewAccessor().ReadInt32(position);
            position += sizeof(int);
            char[] name3 = new char[size3];
            memoryMappedFile.CreateViewAccessor().ReadArray(position, name3, 0, size3);
            return new[] { new string(name1), new string(name2), new string(name3) };
        }
        public static void TakeAndSetJsonFile(string fileName, string directory)
        {
            List<NewsJson> newsVk = new List<NewsJson>();
            MemoryMappedFile memoryMapped = MemoryMappedFile.OpenExisting(fileName);
            long position = 0;
            int newsCount = memoryMapped.CreateViewAccessor().ReadInt32(position);
            position += sizeof(int);
            for (int i = 0; i < newsCount; i++)
            {
                int idCount = memoryMapped.CreateViewAccessor().ReadInt32(position);
                position += sizeof(int);
                char[] id = new char[idCount];
                memoryMapped.CreateViewAccessor().ReadArray(position, id, 0, idCount);
                position += sizeof(char) * idCount;
                int THFCount = memoryMapped.CreateViewAccessor().ReadInt32(position);
                position += sizeof(int);
                char[] THF = new char[THFCount];
                memoryMapped.CreateViewAccessor().ReadArray(position, THF, 0, THFCount);
                position += sizeof(char) * THFCount;
                newsVk.Add(new NewsJson() { Id = new string(id), TextPhotosHref = new string(THF) });
            }
            JSONWorker.SetData(directory + fileName.Remove(6), newsVk);
        }*/
        public static void SetCircle(object fileName)
        {
            DateTime dateTime = DateTime.Now;
            int size = dateTime.ToString().Length; ;
            int capacity = sizeof(int) + size * sizeof(char);
            MemoryMappedFile process1start = MemoryMappedFile.CreateOrOpen(fileName.ToString(), capacity);
            process1start.CreateViewAccessor().Write(0, size);
            process1start.CreateViewAccessor().WriteArray(sizeof(int), dateTime.ToString().ToCharArray(), 0, size);
            //System.IO.IOException: "Недостаточно ресурсов памяти для обработки этой команды." - маловероятно, обрабатывать не буду
        }
        public static void SetCircleStop(object boolAndString)
        {
            if (((BoolAndString)boolAndString).start)
            {
                SetCircle(((BoolAndString)boolAndString).fileName);
            }
        }
        public static bool CheckOnce(ref DateTime last, string fileName)
        {
            MemoryMappedFile memoryMappedFile;
            try
            {
                memoryMappedFile = MemoryMappedFile.OpenExisting(fileName);
            }
            catch
            {
                return false;
            }
            int size = memoryMappedFile.CreateViewAccessor().ReadInt32(0);
            char[] date = new char[size];
            memoryMappedFile.CreateViewAccessor().ReadArray(sizeof(int), date, 0, size);
            DateTime dateTime = DateTime.Now;
            if (ConvertToDate(new string(date), ref dateTime) && IsTimeNew(last, dateTime))
            {
                last = dateTime;
                return true;
            }
            return false;
        }
        public static bool IsTimeNew(DateTime last, DateTime next)
        {
            return last.Subtract(next).TotalDays < 0;
        }
        public static bool ConvertToDate(string input, ref DateTime output)
        {
            bool ret;
            int[] date = { 1, 1, 1, 1, 1, 1 };
            ret = int.TryParse(input.Remove(10).Split('.')[0], out date[2]);
            ret = ret && int.TryParse(input.Remove(10).Split('.')[1], out date[1]);
            ret = ret && int.TryParse(input.Remove(10).Split('.')[2], out date[0]);
            ret = ret && int.TryParse(input.Remove(0, 10).Split(':')[0], out date[3]);
            ret = ret && int.TryParse(input.Remove(0, 10).Split(':')[1], out date[4]);
            ret = ret && int.TryParse(input.Remove(0, 10).Split(':')[2], out date[5]);
            output = new DateTime(date[0], date[1], date[2], date[3], date[4], date[5]);
            return ret;
            //даже не знаю зачем я сделал эту проверку, когда данные записывает
            //программа и ошибок в ней нет
        }
        public static void CheckWithSemaphore(object helpSemaphore)
        {
            if (((HelpSemaphore)helpSemaphore).open)
            {
                return;
            }
            MemoryMappedFile memoryMappedFile;
            try
            {
                memoryMappedFile = MemoryMappedFile.OpenExisting(((HelpSemaphore)helpSemaphore).sharedName);
            }
            catch
            {
                return;
            }
            int size = memoryMappedFile.CreateViewAccessor().ReadInt32(0);
            char[] date = new char[size];
            memoryMappedFile.CreateViewAccessor().ReadArray(sizeof(int), date, 0, size);
            DateTime dateTime = DateTime.Now;

            if (ConvertToDate(new string(date), ref dateTime) && IsTimeNew(((HelpSemaphore)helpSemaphore).lastDate, dateTime))
            {
                ((HelpSemaphore)helpSemaphore).lastDate = dateTime;
                ((HelpSemaphore)helpSemaphore).release();
            }
        }
    }
}
