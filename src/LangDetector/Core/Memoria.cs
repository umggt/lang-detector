using LangDetector.Core.Modelos;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LangDetector.Core
{
    class Memoria
    {
        public int CantidadIdiomas { get; set; }
        public int CantidadDocumentos { get; set; }
        public int CantidadPalabras { get; set; }
        public int CantidadLetras { get; set; }
        public int CantidadSignos { get; set; }
        public int CantidadSimbolos { get; set; }

        public SortedDictionary<string, Idioma> Idiomas { get; set; }
        public SortedDictionary<string, int> Palabras { get; set; }

        public Memoria()
        {
            Idiomas = new SortedDictionary<string, Idioma>();
            Palabras = new SortedDictionary<string, int>();
        }

        public Memoria(bool cargar)
        {
            Idiomas = new SortedDictionary<string, Idioma>();
            Palabras = new SortedDictionary<string, int>();
            if (cargar)
            {
                CargarDesdeArchivo();
            }
        }

        private void CargarDesdeArchivo()
        {
            if (File.Exists("datos.json"))
            {
                Memoria datos;

                using (var streamReader = new StreamReader("datos.json", Encoding.UTF8))
                {
                    using (var reader = new JsonTextReader(streamReader))
                    {
                        var serializer = new JsonSerializer();
                        datos = serializer.Deserialize<Memoria>(reader);
                    }
                }
                
                if (datos != null)
                {
                    CantidadIdiomas = datos.CantidadIdiomas;
                    CantidadDocumentos = datos.CantidadDocumentos;
                    CantidadPalabras = datos.CantidadPalabras;
                    CantidadLetras = datos.CantidadLetras;
                    CantidadSignos = datos.CantidadSignos;
                    CantidadSimbolos = datos.CantidadSimbolos;
                    Idiomas = datos.Idiomas;
                    Palabras = datos.Palabras;
                }
            }
        }

        public void RecordarALargoPlazo()
        {

            using (var streamWriter = new StreamWriter("datos.json", append: false, encoding:  Encoding.UTF8))
            {
                using (var writer = new JsonTextWriter(streamWriter))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(writer, this);
                }
            }
        }
    }
}
