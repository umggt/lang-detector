namespace LangDetector.Core.Modelos
{
    class Letra
    {
        public char Caracter { get; set; }
        public int CantidadEnIdioma { get; set; }
        public int CantidadEnDocumentos { get; set; }
        public int CantidadOtrosIdiomas { get; set; }
        public int CantidadOtrosDocumentos { get; set; }

        public TipoLetra Tipo { get; set; }

        public Letra(TipoLetra tipo = TipoLetra.Letra)
        {
            Tipo = tipo;
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
