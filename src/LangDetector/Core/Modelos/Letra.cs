using System.Collections.Generic;

namespace LangDetector.Core.Modelos
{
    class Letra
    {
        public char Caracter { get; set; }
        public int Cantidad { get; set; }
        public int Documentos { get; set; }

        public SortedDictionary<string, int> Idiomas { get; set; }

        public Letra()
        {
            Idiomas = new SortedDictionary<string, int>();
        }
        
    }
}
