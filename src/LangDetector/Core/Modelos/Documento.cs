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
        /// <summary>
        /// Identificador único del documento.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Cantidad de letras que posee el documento.
        /// </summary>
        public int Letras { get; set; }

    }
}
