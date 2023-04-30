using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO.MemoryMappedFiles;
using System.IO;

namespace HomeTaskService1
{
    internal class SHAREDWorker
    {/*public static void AddJsonFile(string directory, HelpSharedName jsonName, Semaphore semaphore)
        {
            List<NewsJson> newsVk = new List<NewsJson>();
            JSONWorker.ReadDataFrom1File(ref newsVk, directory + jsonName.GetMainName());
            long capacity = sizeof(int);
            for (int i = 0; i < newsVk.Count; i++)
            {
                capacity += sizeof(int) * 2;
                capacity += newsVk[i].Id.Length * sizeof(char);
                capacity += newsVk[i].TextPhotosHref.Length * sizeof(char);
            }
            if (capacity > jsonName.capacity)
            {
                jsonName.capacity = capacity + capacity / newsVk.Count * 100;
                jsonName.ChangeAddName();
            }
            MemoryMappedFile memoryMapped;
            try
            {
                memoryMapped = MemoryMappedFile.CreateOrOpen(jsonName.GetFileName(), jsonName.capacity);
            }
            catch
            {
                Service1.onStop(semaphore);
                return;
            }
            memoryMapped.CreateViewAccessor().Write(0, newsVk.Count);
            long position = sizeof(int);
            for (int i = 0; i < newsVk.Count; i++)
            {
                memoryMapped.CreateViewAccessor().Write(position, newsVk[i].Id.Length);
                memoryMapped.CreateViewAccessor().WriteArray(position + sizeof(int), newsVk[i].Id.ToCharArray(), 0, newsVk[i].Id.Length);
                position += sizeof(int) + sizeof(char) * newsVk[i].Id.Length;
                memoryMapped.CreateViewAccessor().Write(position, newsVk[i].TextPhotosHref.Length);
                memoryMapped.CreateViewAccessor().WriteArray(position + sizeof(int), newsVk[i].TextPhotosHref.ToCharArray(), 0, newsVk[i].TextPhotosHref.Length);
                position += sizeof(int) + sizeof(char) * newsVk[i].TextPhotosHref.Length;
            }
            semaphore.Release();
        }
        public static bool AddFileName(string[] jsonNames)
        {
            long position = 0;
            int capacity = 3 * sizeof(int) + sizeof(char) * (jsonNames[0].Length + jsonNames[1].Length + jsonNames[2].Length);
            MemoryMappedFile fileNames;
            try
            {
                fileNames = MemoryMappedFile.CreateOrOpen("fileNames", capacity);
            }
            catch
            {
                return false;
            }
            fileNames.CreateViewAccessor().Write(position, jsonNames[0].Length);
            position += sizeof(int);
            fileNames.CreateViewAccessor().WriteArray(position, jsonNames[0].ToCharArray(), 0, jsonNames[0].Length);
            position += sizeof(char) * jsonNames[0].Length;
            fileNames.CreateViewAccessor().Write(position, jsonNames[1].Length);
            position += sizeof(int);
            fileNames.CreateViewAccessor().WriteArray(position, jsonNames[1].ToCharArray(), 0, jsonNames[1].Length);
            position += sizeof(char) * jsonNames[1].Length;
            fileNames.CreateViewAccessor().Write(position, jsonNames[2].Length);
            position += sizeof(int);
            fileNames.CreateViewAccessor().WriteArray(position, jsonNames[2].ToCharArray(), 0, jsonNames[2].Length);
            return true;
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
                if (((HelpSemaphore)helpSemaphore).sharedName.Contains("get"))
                {
                    ((HelpSemaphore)helpSemaphore).release();
                }
                return;
            }
            int size = memoryMappedFile.CreateViewAccessor().ReadInt32(0);
            char[] date = new char[size];
            memoryMappedFile.CreateViewAccessor().ReadArray(sizeof(int), date, 0, size);
            DateTime dateTime = DateTime.Now;
            if (((HelpSemaphore)helpSemaphore).sharedName.Contains("get"))
            {
                if (ConvertToDate(new string(date), ref dateTime) && !IsTimeNew(((HelpSemaphore)helpSemaphore).lastDate, dateTime))
                {
                    ((HelpSemaphore)helpSemaphore).lastDate = dateTime;
                    ((HelpSemaphore)helpSemaphore).release();
                }
            }
            else
            {
                if (ConvertToDate(new string(date), ref dateTime) && IsTimeNew(((HelpSemaphore)helpSemaphore).lastDate, dateTime))
                {
                    ((HelpSemaphore)helpSemaphore).lastDate = dateTime;
                    ((HelpSemaphore)helpSemaphore).release();
                }
            }
        }
    }
}
