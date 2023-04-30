using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace HomeTaskService1
{
    [DataContract]
    internal class NewsJson
    {
        [DataMember]
        public string Id;
        [DataMember]
        public string TextPhotosHref;
        public NewsJson(NewsVk newsVk, string fileName)
        {
            Id = newsVk.Id;
            if (fileName.Contains("1.json"))
            {
                TextPhotosHref = newsVk.Text;
            }
            else
            {
                if (fileName.Contains("2.json"))
                {
                    TextPhotosHref = newsVk.Photos;
                }
                else
                {
                    if (fileName.Contains("3.json"))
                    {
                        TextPhotosHref = newsVk.Href;
                    }
                }
            }
        }
    }
}
