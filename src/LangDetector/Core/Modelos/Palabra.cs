using System.Collections.Generic;

namespace LangDetector.Core.Modelos
{
    class Palabra
    {
        public string Texto { get; set; }
        public int Documentos { get; set; }

        public SortedDictionary<string, int> Idiomas { get; set; }

        public Palabra()
        {
            Idiomas = new SortedDictionary<string, int>();
        }
    }
}
