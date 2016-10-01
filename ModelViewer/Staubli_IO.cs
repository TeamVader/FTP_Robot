using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaubliEasyFTPClient
{
    class Staubli_IO
    {
       // public Staubli_IO(string name, string type, string link, string access) //: this(name, type, DateTime.Now)
//{}

        public Staubli_IO(string Name, string Type, string Link, string Access)
        {
            name = Name;
            type = Type;
            link = Link;
            access = Access;

        }

        public string name { get; set; }
        public string type { get; set; }
        public string link { get; set; }
        public string access { get; set; }
    }
}
