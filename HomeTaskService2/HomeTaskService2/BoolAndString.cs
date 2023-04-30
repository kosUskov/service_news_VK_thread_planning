using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeTaskService2
{
    internal class BoolAndString
    {
        public bool start;
        public string fileName;
        public BoolAndString(string fileName)
        {
            this.fileName = fileName;
            start = false;
        }
    }
}
