using System.Collections.Generic;

namespace LangDetector.Core.Modelos
{
    class DocumentoProcesado
    {
        public IDictionary<string, int> Palabras { get; set; }
        public IDictionary<char, int> Letras { get; set; }
        public IDictionary<char, int> Signos { get; set; }
        public IDictionary<char, int> Simbolos { get; set; }

        public int CantidadLetras { get; set; }
        public int CantidadSignos { get; set; }
        public int CantidadSimbolos { get; set; }
        public int CantidadPalabras { get; set; }

        public string Idioma { get; set; }
        public string Hash { get; set; }
    }
}
