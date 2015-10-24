using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LangDetector.Core.Modelos
{
    class Letra
    {
        public int Id { get; set; }
        public long DocumentoId { get; set; }
        public int Cantidad { get; set; }
        public double Porcentaje { get; set; }

        public Letra()
        {

        }

        public Letra(Documento documento, KeyValuePair<char, int> valor)
        {
            Id = valor.Key;
            Cantidad = valor.Value;
            Porcentaje = Cantidad / documento.Letras;
            DocumentoId = documento.Id;
        }
    }
}
