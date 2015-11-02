using System.Collections.Generic;

namespace LangDetector.Core.Modelos
{
    class Idioma
    {
        public string Nombre { get; set; }
        public int CantidadDocumentos { get; set; }
        public int CantidadPalabras { get; set; }
        public int CantidadLetras { get; set; }
        public int CantidadSignos { get; set; }
        public int CantidadSimbolos { get; set; }

        public int CantidadInversaDocumentos { get; set; }
        public int CantidadInversaPalabras { get; set; }
        public int CantidadInversaLetras { get; set; }
        public int CantidadInversaSignos { get; set; }
        public int CantidadInversaSimbolos { get; set; }

        public SortedDictionary<string, Documento> Documentos { get; set; }
        public SortedDictionary<string, Palabra> Palabras { get; set; }
        public SortedDictionary<char, Letra> Letras { get; set; }
        public SortedDictionary<char, Simbolo> Simbolos { get; set; }
        public SortedDictionary<char, Signo> Signos { get; set; }


        public Idioma()
        {
            Documentos = new SortedDictionary<string, Documento>();
            Palabras = new SortedDictionary<string, Palabra>();
            Letras = new SortedDictionary<char, Letra>();
            Simbolos = new SortedDictionary<char, Simbolo>();
            Signos = new SortedDictionary<char, Signo>();
        }
    }
}
