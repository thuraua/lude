using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class Owner
    {
        public int ONr { get; set; }
        public string OName { get; set; }

        public Owner(int id, string name)
        {
            ONr = id;
            OName = name;
        }

        public override string ToString()
        {
            return "{" + ONr + ", " + OName + "}";
        }
    }
}
