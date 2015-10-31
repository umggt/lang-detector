using System;

namespace LangDetector.Core.Events
{
    public class SolicitarIdiomaEventArgs : EventArgs
    {

        public string Mensaje { get; set; }
        public long? IdiomaId { get; set; }
        public string IdiomaNombre { get; set; }
        internal Repositorio Repositorio { get; set; }
    }
}
