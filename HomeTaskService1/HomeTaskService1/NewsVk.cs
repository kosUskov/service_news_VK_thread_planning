using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeTaskService1
{
    internal class NewsVk
    {
        public string Id;
        public string Text;
        public string Photos;//раньше был массив, но в БД массивы не записываются
        public string Href;//и по факту разницы нет, как хранить.
    }
}
