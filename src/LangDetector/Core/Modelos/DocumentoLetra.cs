using System.Collections.Generic;

namespace LangDetector.Core.Modelos
{
    class DocumentoLetra
    {
        public int Id { get; set; }
        public long DocumentoId { get; set; }
        public int Cantidad { get; set; }
        public double Porcentaje { get; set; }
        public DocumentoTipoLetra Tipo { get; set; }

        public DocumentoLetra()
        {
        }

        public DocumentoLetra(KeyValuePair<char, int> valor, Documento documento, DocumentoTipoLetra tipo = DocumentoTipoLetra.Letra)
        {
            Id = valor.Key;
            DocumentoId = documento.Id;
            Cantidad = valor.Value;
            Tipo = tipo;
            if (tipo == DocumentoTipoLetra.Letra)
            {
                Porcentaje = (double)Cantidad / documento.Letras * 100;
            }
            else if (tipo == DocumentoTipoLetra.Simbolo)
            {
                Porcentaje = (double)Cantidad / documento.Simbolos * 100;
            }
            else
            {
                Porcentaje = (double)Cantidad / documento.Signos * 100;
            }
        }
    }

    class DocumentoSimbolo : DocumentoLetra
    {
        public DocumentoSimbolo(KeyValuePair<char, int> valor, Documento documento) : base(valor, documento, DocumentoTipoLetra.Simbolo)
        {

        }
    }

    class DocumentoSigno : DocumentoLetra
    {
        public DocumentoSigno(KeyValuePair<char, int> valor, Documento documento) : base(valor, documento, DocumentoTipoLetra.SignoDePuntuacion)
        {

        }
    }
}
