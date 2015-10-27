using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LangDetector.Core.Events
{
    public class AvanceGlobalEventArgs : EventArgs
    {
        public int TotalPasos { get; set; }
        public int Paso { get; set; }
    }
}
