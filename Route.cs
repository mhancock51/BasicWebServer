using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicWebServer
{
    public class Route
    {
        public Route(string path, string relativeFilePath)
        {
            this.path = path;
            this.relativeFilePath = relativeFilePath;
        }

        public string path { get; set; }
        public string relativeFilePath { get; set; }
    }
}
