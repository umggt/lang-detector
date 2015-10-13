using System;

namespace LangDetector.Core.Events
{
    /// <summary>
    /// Argumentos que se enviarán a las funciones que estén suscritas al evento BytesLeidos de la case Parser.
    /// </summary>
    /// <seealso cref="Parser"/>
    public class BytesLeidosEventArgs : EventArgs
    {
        /// <summary>
        /// Cantidad de bytes que se han leído del documento.
        /// </summary>
        public long BytesLeidos { get; set; }

        /// <summary>
        /// Cantidad de bytes que posee en total el documento.
        /// </summary>
        public long BytesTotales { get; set; }
    }
}
