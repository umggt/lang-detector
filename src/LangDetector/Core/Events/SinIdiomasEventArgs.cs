using System;

namespace LangDetector.Core.Events
{
    public class SinIdiomasEventArgs : EventArgs
    {
        public string Mensaje { get; set; }
        public string NombreIdioma { get; set; }
    }
}
