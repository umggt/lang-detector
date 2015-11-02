using System.Collections.Generic;

namespace LangDetector.Core.Modelos
{
    class Letra
    {
        public char Caracter { get; set; }
        public int Cantidad { get; set; }
        public int Documentos { get; set; }

        public SortedDictionary<string, int> Idiomas { get; set; }

        public TipoLetra Tipo { get; set; }

        public Letra(TipoLetra tipo = TipoLetra.Letra)
        {
            Tipo = tipo;
            Idiomas = new SortedDictionary<string, int>();
        }
        
    }

    class Simbolo : Letra
    {
        public Simbolo() : base(TipoLetra.Simbolo)
        {

        }
    }

    class Signo : Letra
    {
        public Signo() : base(TipoLetra.SignoDePuntuacion)
        {

        }
    }
}
