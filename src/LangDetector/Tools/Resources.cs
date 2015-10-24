using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LangDetector.Tools
{
    class Resources
    {
        public static string ObtenerTexto(string resourceName)
        {
            using (var sr = new StreamReader(typeof(Resources).Assembly.GetManifestResourceStream(resourceName)))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
