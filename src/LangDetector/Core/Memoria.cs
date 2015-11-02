using LangDetector.Core.Modelos;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LangDetector.Core
{
    class Memoria
    {

        private string pathMemoria;
        private string pathDocumentos;
        private string pathIdiomas;
        private string pathPalabras;
        private string pathLetras;
        private string pathSignos;
        private string pathSimbolos;

        public int CantidadIdiomas { get; set; }
        public int CantidadDocumentos { get; set; }
        public int CantidadPalabras { get; set; }
        public int CantidadLetras { get; set; }
        public int CantidadSignos { get; set; }
        public int CantidadSimbolos { get; set; }

        public SortedDictionary<string, Documento> Documentos { get; set; }
        public SortedDictionary<string, Idioma> Idiomas { get; set; }
        public SortedDictionary<string, Palabra> Palabras { get; set; }
        public SortedDictionary<char, Letra> Letras { get; set; }
        public SortedDictionary<char, Signo> Signos { get; set; }
        public SortedDictionary<char, Simbolo> Simbolos { get; set; }

        public Memoria()
        {
            Documentos = new SortedDictionary<string, Documento>();
            Idiomas = new SortedDictionary<string, Idioma>();
            Palabras = new SortedDictionary<string, Palabra>();
            Letras = new SortedDictionary<char, Letra>();
            Signos = new SortedDictionary<char, Signo>();
            Simbolos = new SortedDictionary<char, Simbolo>();

            pathMemoria = Path.Combine("datos", "memoria.json");
            pathDocumentos = Path.Combine("datos", "documentos.json");
            pathIdiomas = Path.Combine("datos", "idiomas.json");
            pathPalabras = Path.Combine("datos", "palabras.json");
            pathLetras = Path.Combine("datos", "letras.json");
            pathSignos = Path.Combine("datos", "signos.json");
            pathSimbolos = Path.Combine("datos", "simbolos.json");

            CargarDesdeArchivo();
        }

        private void CargarDesdeArchivo()
        {
            if (!Directory.Exists("datos"))
            {
                Directory.CreateDirectory("datos");
            }

            CargarMemoria();
            Documentos = Cargar<SortedDictionary<string, Documento>>(pathDocumentos) ?? new SortedDictionary<string, Documento>();
            Idiomas = Cargar<SortedDictionary<string, Idioma>>(pathIdiomas) ?? new SortedDictionary<string, Idioma>();
            Palabras = Cargar<SortedDictionary<string, Palabra>>(pathPalabras) ?? new SortedDictionary<string, Palabra>();
            Letras = Cargar<SortedDictionary<char, Letra>>(pathLetras) ?? new SortedDictionary<char, Letra>();
            Signos = Cargar<SortedDictionary<char, Signo>>(pathSignos) ?? new SortedDictionary<char, Signo>();
            Simbolos = Cargar<SortedDictionary<char, Simbolo>>(pathSimbolos) ?? new SortedDictionary<char, Simbolo>();
        }


        public void RecordarALargoPlazo()
        {
            GuardarMemoria();
            Guardar(pathDocumentos, Documentos);
            Guardar(pathIdiomas, Idiomas);
            Guardar(pathPalabras, Palabras);
            Guardar(pathLetras, Letras);
            Guardar(pathSignos, Signos);
            Guardar(pathSimbolos, Simbolos);
        }

        private void CargarMemoria()
        {
            var memo = Cargar<dynamic>(pathMemoria);

            if (memo != null)
            {
                CantidadIdiomas = memo.CantidadIdiomas;
                CantidadDocumentos = memo.CantidadDocumentos;
                CantidadPalabras = memo.CantidadPalabras;
                CantidadLetras = memo.CantidadLetras;
                CantidadSignos = memo.CantidadSignos;
                CantidadSimbolos = memo.CantidadSimbolos;
            }
        }

        private void GuardarMemoria()
        {
            Guardar(pathMemoria, new
            {
                CantidadIdiomas,
                CantidadDocumentos,
                CantidadPalabras,
                CantidadLetras,
                CantidadSignos,
                CantidadSimbolos
            });
        }

        private static T Cargar<T>(string path)
        {
            if (File.Exists(path))
            {
                using (var streamReader = new StreamReader(path, Encoding.UTF8))
                {
                    using (var reader = new JsonTextReader(streamReader))
                    {
                        var serializer = new JsonSerializer();
                        return serializer.Deserialize<T>(reader);
                    }
                }
            }

            return default(T);
        }

        private static void Guardar(string path, object objeto)
        {
            using (var streamWriter = new StreamWriter(path, append: false, encoding: Encoding.UTF8))
            {
                using (var writer = new JsonTextWriter(streamWriter))
                {
                    var serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(writer, objeto);
                }
            }
        }
    }
}
