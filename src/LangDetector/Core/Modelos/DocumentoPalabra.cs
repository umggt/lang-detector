using System.Collections.Generic;

namespace LangDetector.Core.Modelos
{
    class DocumentoPalabra
    {
        public string Palabra { get; set; }
        public long DocumentoId { get; set; }
        public int Cantidad { get; set; }
        public double Porcentaje { get; set; }

        public DocumentoPalabra()
        {
        }

        public DocumentoPalabra(KeyValuePair<string, int> valor, Documento documento)
        {
            Palabra = valor.Key;
            Cantidad = valor.Value;
            DocumentoId = documento.Id;
            Porcentaje = (double)Cantidad / documento.Palabras * 100;
        }
    }
}
