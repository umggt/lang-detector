namespace LangDetector.Core.Modelos
{
    /// <summary>
    /// Clase Documento que representa los datos almacenados en la tabla DOCUMENTOS
    /// para una tupla específica.
    /// </summary>
    class Documento
    {
        public string Hash { get; set; }
        public string Idioma { get; set; }

        public int Letras { get; set; }
        public int Palabras { get; set; }
        public int Signos { get; set; }
        public int Simbolos { get; set; }

        public int LetrasDistintas { get; set; }
        public int PalabrasDistintas { get; set; }
        public int SignosDistintos { get; set; }
        public int SimbolosDistintos { get; set; }

    }
}
