using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace HomeTaskService1
{
    internal class HelpSemaphore
    {
        public Semaphore semaphore;
        public DateTime lastDate;
        public bool open;
        public string sharedName;
        public HelpSemaphore(string sharedName, DateTime dateTime)
        {
            this.sharedName = sharedName;
            lastDate = dateTime;
            open = true;
            semaphore = new Semaphore(0, 1);
        }
        public void release()
        {
            open = true;
            semaphore.Release();
        }
        public void waitOne()
        {
            open = false;
            semaphore.WaitOne();
        }
    }
}
