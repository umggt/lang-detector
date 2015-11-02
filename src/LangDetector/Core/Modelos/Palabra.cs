namespace LangDetector.Core.Modelos
{
    class Palabra
    {
        public string Texto { get; set; }
        public int CantidadEnIdioma { get; set; }
        public int CantidadEnDocumentos { get; set; }
        public int CantidadOtrosIdiomas { get; set; }
        public int CantidadOtrosDocumentos { get; set; }

        public Palabra()
        {
        }
    }
}
