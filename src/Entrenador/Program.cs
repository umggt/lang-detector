using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LangDetector.Entrenador
{
    class Program
    {
        const int TOTAL_ARTICULOS_DESCARGAR = 100;

        static IDictionary<string, string> idiomas = new Dictionary<string, string>() {
            { "es", "español" },
            { "de", "alemán" },
            { "it", "italiano" },
            { "fr", "francés" },
            { "en", "inglés" },
            { "ru", "ruso" },
            { "pt", "portugés" },
            { "ja",  "japonés" }
        };

        static void Main(string[] args)
        {
            Task.Factory.StartNew(Entrenar);
            Console.WriteLine("Presione [Enter] para salir");
            Console.ReadLine();    
        }

        private static async Task Entrenar()
        {
            var langs = new[] {
                "es", //español
                "de", //alemán
                "it", //italiano
                "fr", //francés
                "en", //inglés
                "ru", //ruso
                "pt", //portugés
                "ja"  //japonés
            };

            do
            {
                foreach (var lang in langs)
                {
                    if (!Directory.Exists(lang))
                    {
                        Directory.CreateDirectory(lang);
                    }

                    try
                    {
                        await Entrenar(lang);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    
                }

            } while (langs.Any(x => TOTAL_ARTICULOS_DESCARGAR - Directory.GetFiles(x, "*.txt").Length > 0));

        }

        private static async Task Entrenar(string lang)
        {

            Console.WriteLine("Entrenando {0}...", lang);
            var cantidadArticulos = TOTAL_ARTICULOS_DESCARGAR - Directory.GetFiles(lang, "*.txt").Length;
            if (cantidadArticulos <= 0)
            {
                Console.WriteLine("Entrenado.");
                return;
            }
            var wikiUrl = $"https://{lang}.wikipedia.org/w/api.php?";

            using (var cliente = new WebClient())
            {
                cliente.Encoding = Encoding.UTF8;

                Console.WriteLine("Obteniendo ids de articulos aleatorios");
                var wikiRandoms = JsonConvert.DeserializeObject<dynamic>(await cliente.DownloadStringTaskAsync(wikiUrl + "action=query&list=random&format=json&rnlimit=" + cantidadArticulos));

                foreach (var infoArticulo in wikiRandoms.query.random)
                {
                    string id = infoArticulo.id;
                    string titulo = infoArticulo.title;
                    string articulo = null;

                    using (var cliente2 = new WebClient())
                    {
                        var wikiArticulo = JsonConvert.DeserializeObject<dynamic>(await cliente.DownloadStringTaskAsync(wikiUrl + $"action=query&prop=revisions&format=json&rvprop=content&rvparse=&rvcontentformat=text%2Fplain&pageids={id}"));

                        foreach (var pages in wikiArticulo.query.pages)
                        {
                            foreach (var page in pages)
                            {
                                Console.WriteLine("Titulo: {0}", page.title);

                                foreach (var revision in page.revisions)
                                {
                                    foreach (var contenidos in revision)
                                    {

                                        foreach (var contenido in contenidos)
                                        {
                                            articulo = contenido.ToString();
                                            if (!string.IsNullOrWhiteSpace(articulo))
                                            {
                                                break;
                                            }
                                        }

                                        break;

                                    }

                                    break;

                                }
                                break;

                            }
                            break;
                        }
                    }
                    

                    if (!string.IsNullOrWhiteSpace(articulo))
                    {
                        foreach (var item in Path.GetInvalidFileNameChars())
                        {
                            titulo = titulo.Replace(item, '-');
                        }

                        var path = Path.Combine(lang, titulo + ".html");
                        File.WriteAllText(path, articulo, Encoding.UTF8);

                        var x = new HtmlAgilityPack.HtmlDocument();
                        x.Load(path);
                        File.WriteAllText(path + ".txt", x.DocumentNode.InnerText);

                        File.Delete(path);

                        var agente = new Agente(path + ".txt", true);
                        agente.SolicitarIdioma += (o, e) => e.Idioma = idiomas[lang];
                        await agente.IdentificarIdioma();
                    }
                }

            }

            Console.WriteLine("Entrenamiento {0} completo.", lang);

        }

        private class WikiArticulo
        {

        }

    }
}
