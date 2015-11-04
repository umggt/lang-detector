using System.Collections.Generic;

namespace LangDetector.Core.Modelos
{
    class Idioma
    {
        public string Nombre { get; set; }
        public int CantidadDocumentos { get; set; }
        public int CantidadPalabras { get; set; }
        public int CantidadLetras { get; set; }
    }
}
