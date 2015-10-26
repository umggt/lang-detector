using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LangDetector.Core.Modelos
{
    /// <summary>
    /// Clase Documento que representa los datos almacenados en la tabla DOCUMENTOS
    /// para una tupla específica.
    /// </summary>
    class Documento
    {
        public string Hash { get; set; }

        /// <summary>
        /// Identificador único del documento.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Idioma en el que está escrito el documento
        /// </summary>
        public long? IdiomaId { get; set; }

        /// <summary>
        /// Cantidad de letras que posee el documento.
        /// </summary>
        public int Letras { get; set; }

        public int Palabras { get; set; }
        public int Signos { get; set; }
        public int Simbolos { get; set; }
        public long? Confianza { get; set; }
    }
}
