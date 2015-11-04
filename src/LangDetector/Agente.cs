using LangDetector.Core;
using LangDetector.Core.Events;
using LangDetector.Core.Modelos;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LangDetector
{
    public class Agente 
    {
        private const double totalPasos = 6;

        private readonly bool modoEntrenamiento;
        private readonly string archivo;

        private static readonly Cerebro cerebro = new Cerebro();
        
        public event EventHandler<SolicitarIdiomaEventArgs> SolicitarIdioma;
        public event EventHandler<AvanceEventArgs> AvanceGlobal;
        public event EventHandler<AvanceEventArgs> AvanceParcial;

        public Agente(string archivo, bool modoEntrenamiento)
        {
            if (!File.Exists(archivo))
            {
                throw new FileNotFoundException("El archivo especificado no existe.", archivo);
            }

            this.archivo = archivo;
            this.modoEntrenamiento = modoEntrenamiento;
            
        }

        public async Task<IEnumerable<IdentificacionResultado>> IdentificarIdioma()
        {

            var documentoProcesado = await ParsearDocumento();

            if (documentoProcesado.CantidadLetras + documentoProcesado.CantidadSignos + documentoProcesado.CantidadSimbolos == 0)
            {
                throw new InvalidOperationException("El documento está vacío o contiene signos que el agente no puede procesar.");
            }

            var cantidadDeIdiomasQueConozco = cerebro.CuantosIdiomasConozco();

            if (cantidadDeIdiomasQueConozco == 0)
            {
                Aprender(documentoProcesado);
            }
            else if (modoEntrenamiento)
            {
                Entrenar(documentoProcesado);
            }

            if (!string.IsNullOrWhiteSpace(documentoProcesado.Idioma))
            {
                return new[] {
                    new IdentificacionResultado
                    {
                        Idioma = documentoProcesado.Idioma,
                        Certeza = 1
                    }
                };
            }

            return cerebro.Identifica(documentoProcesado);
           
        }

        private void CuandoProgresaIdentificacion(object sender, AvanceEventArgs e)
        {
            NotificarAvanceParcial(e.Porcentaje);
        }

        private void Entrenar(DocumentoProcesado documentoProcesado)
        {
            if (SolicitarIdioma == null)
            {
                throw new InvalidOperationException("En modo entrenamiento el agente necesita saber como preguntar el dioma de un documento.");
            }

            var parametros = new SolicitarIdiomaEventArgs {
                Mensaje = "El agente está en modo entrenamiento, por lo que necesita que le digas en qué idioma está escrito este documento.",
                Idiomas = cerebro.Memoria.Idiomas.Select(x => x.Key).ToArray()
            };

            SolicitarIdioma(this, parametros);

            if (string.IsNullOrWhiteSpace(parametros.Idioma))
            {
                throw new InvalidOperationException("El agente está en modo entrenamiento, preguntó pero no le indicaron ningún idioma de referencia para el documento.");
            }

            cerebro.Entrena(documentoProcesado, parametros.Idioma);
        }

        private void Aprender(DocumentoProcesado documentoProcesado)
        {
            if (SolicitarIdioma == null)
            {
                throw new InvalidOperationException("El agente no sabe ningún idioma y no sabe a quién preguntarle.");
            }

            var parametros = new SolicitarIdiomaEventArgs {
                Mensaje = "El agente no sabe ningún idioma, por lo que necesita que le digas en qué idioma está escrito este documento.",
                Idiomas = cerebro.Memoria.Idiomas.Select(x => x.Key).ToArray()
            };

            SolicitarIdioma(this, parametros);

            if (string.IsNullOrWhiteSpace(parametros.Idioma))
            {
                throw new InvalidOperationException("El agente no sabe ningún idioma, preguntó pero no le indicaron ningún idioma de referencia.");
            }

            cerebro.Entrena(documentoProcesado, parametros.Idioma);
        }

        private async Task<DocumentoProcesado> ParsearDocumento()
        {
            var cronometro = Stopwatch.StartNew();

            var parser = new Parser(archivo);

            parser.BytesLeidos += CuandoElParserProceseBytes;
            var documentoProcesado = await parser.Procesar();
            parser.BytesLeidos -= CuandoElParserProceseBytes;

            NotificarAvanceGlobal(1);

            Debug.WriteLine("Documento procesado en {0} milisegundos", cronometro.ElapsedMilliseconds);
            cronometro.Restart();

            var hash = CancularHash(documentoProcesado.Palabras);
            var idioma = cerebro.RecordarDocumento(hash);

            if (idioma != null)
            {
                documentoProcesado.Idioma = idioma;
            }

            documentoProcesado.Hash = hash;

            Debug.WriteLine("Documento buscando en la memoria del agente en {0} milisegundos", cronometro.ElapsedMilliseconds);
            cronometro.Stop();

            return documentoProcesado;
        }

        private string CancularHash(IDictionary<string, int> palabras)
        {

            byte[] hash = null;
            using (var encriptador = RIPEMD160.Create())
            {
                hash = encriptador.ComputeHash(Encoding.UTF8.GetBytes(string.Join(",", palabras.Select(x => x.Key))));
            }

            var sb = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
            {
                sb.AppendFormat("{0:X2}", hash[i]);
                if ((i % 4) == 3) sb.Append("-");
            }

            return sb.ToString().Trim('-');
        }

        private void CuandoElParserProceseBytes(object sender, BytesLeidosEventArgs e)
        {
            NotificarAvanceParcial(e.BytesLeidos, e.BytesTotales);
        }

        private void NotificarAvanceParcial(double valor, double total)
        {
            if (AvanceParcial != null)
            {
                AvanceParcial(this, new AvanceEventArgs { Porcentaje = (valor / total * 100) });
            }
        }

        private void NotificarAvanceParcial(double porcentaje)
        {
            if (AvanceParcial != null)
            {
                AvanceParcial(this, new AvanceEventArgs { Porcentaje = porcentaje });
            }
        }

        private void NotificarAvanceGlobal(double paso)
        {
            if (AvanceGlobal != null)
            {
                AvanceGlobal(this, new AvanceEventArgs { Porcentaje = (paso / totalPasos * 100) });
            }
        }

        
    }
}
