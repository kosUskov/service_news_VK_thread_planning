using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeTaskService1
{/*
    internal class HelpSharedName
    {
        //для имён в нашем случае нет ограничений, как для файлов
        //на жёстком диске, разве только \ не стоит использовать
        //другие же символы допустимы и длина может быть сколь
        //угодно большой. Int32 для длины вполне будет достаточно
        //учитывая столь большой перебор имён
        private const string alphavit = " ячсмитьбюфывапролджэйцукенгшщзхъёЯЧСМИТЬБЮФЫВАПРОЛДЖЭЙЦУКЕНГШЩЗХЪЁzxcvbnmasdfghjklqwertyuiopZXCVBNMASDFGHJKLQWERTYUIOP`~1234567890-=!@#$%^&*()_+№;:?{}[]|/,<>.";
        private string mainName;
        private string addName;
        private int positionInAddName;
        public long capacity;

        public HelpSharedName(string mainName)
        {
            this.mainName = mainName;
            positionInAddName = 0;
            capacity = 0;
            addName = " ";
        }
        public string GetFileName()
        {
            return String.Concat(mainName, addName);
        }
        public string GetMainName()
        {
            return mainName;
        }
        private int GetNumAlphavit(int position)
        {
            return alphavit.IndexOf(addName[position]);
        }
        private bool CheckNewChange(int position)
        {
            if (positionInAddName == -1)
            {
                addName = new string(' ', addName.Length + 1);
                positionInAddName = addName.Length - 1;
                return false;
            }
            if (GetNumAlphavit(position) == alphavit.Length - 1)
            {
                return true;
            }
            return false;
        }
        public void ChangeAddName()
        {
            while (CheckNewChange(positionInAddName))
            {
                positionInAddName--;
            }
            char[] tmp = addName.ToCharArray();
            tmp[positionInAddName] = alphavit[GetNumAlphavit(positionInAddName) + 1];
            for (int i = positionInAddName + 1; i < addName.Length - 1; i++)
            {
                tmp[i] = ' ';
            }
            addName = new string(tmp);
            positionInAddName = addName.Length - 1;
        }
    }*/
}
