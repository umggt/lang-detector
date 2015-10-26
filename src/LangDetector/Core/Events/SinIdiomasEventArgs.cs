using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LangDetector.Core.Events
{
    public class SinIdiomasEventArgs : EventArgs
    {
        public string Mensaje { get; set; }
        public string NombreIdioma { get; set; }
    }
}
