namespace LangDetector.Core.Modelos
{
    class Idioma
    {
        public long Id { get; set; }
        public string Nombre { get; set; }
        public int Letras { get; set; }
        public int Signos { get; set; }
        public int Simbolos { get; set; }
        public int Palabras { get; set; }
        public int LetrasDistintas { get; set; }
        public int SignosDistintos { get; set; }
        public int SimbolosDistintos { get; set; }
        public int PalabrasDistintas { get; set; }
    }
}
