using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LangDetector.Core.Modelos
{
    public class DocumentoProcesado
    {
        public IDictionary<char, int> Letras { get; set; }
        public IDictionary<char, int> Signos { get; set; }
        public IDictionary<char, int> Simbolos { get; set; }
        public IDictionary<string, int> Palabras { get; set; }

        public int LetrasCantidad { get; set; }
        public int SignosCantidad { get; set; }
        public int SimbolosCantidad { get; set; }
        public int PalabrasCantidad { get; set; }
    }
}
