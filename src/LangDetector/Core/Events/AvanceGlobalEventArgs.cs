using System;

namespace LangDetector.Core.Events
{
    public class AvanceGlobalEventArgs : EventArgs
    {
        public int TotalPasos { get; set; }
        public int Paso { get; set; }
    }
}
