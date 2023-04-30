using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace HomeTaskService2
{
    [DataContract]
    internal class NewsJson
    {
        [DataMember]
        public string Id;
        [DataMember]
        public string TextPhotosHref;
    }
}
