using System;

namespace LangDetector.Core.Events
{
    public class SolicitarIdiomaEventArgs : EventArgs
    {

        public string Mensaje { get; set; }
        public string Idioma { get; set; }
        public string[] Idiomas { get; set; }
    }
}
