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

namespace LangDetector.Core
{
    class Agente : IDisposable
    {
        private const double totalPasos = 6;
        private readonly string archivo;
        private readonly Repositorio repositorio;

        public event EventHandler<SinIdiomasEventArgs> SolicitarIdioma;
        public event EventHandler<AvanceEventArgs> AvanceGlobal;
        public event EventHandler<AvanceEventArgs> AvanceParcial;

        public Agente(string archivo)
        {
            if (!File.Exists(archivo))
            {
                throw new FileNotFoundException("El archivo especificado no existe.", archivo);
            }

            this.archivo = archivo;
            repositorio = new Repositorio();
        }

        public async Task<Documento> IdentificarIdioma()
        {

            var documento = await ParsearDocumento();

            if (documento.Letras + documento.Signos + documento.Simbolos == 0)
            {
                throw new InvalidOperationException("El documento está vacío o contiene signos que el agente no puede procesar.");
            }

            var cantidadDeIdiomasQueConozco = await repositorio.ContarIdiomasConocidos();

            if (cantidadDeIdiomasQueConozco == 0)
            {
                if (SolicitarIdioma == null)
                {
                    throw new InvalidOperationException("El agente no sabe ningún idioma y no sabe a quién preguntarle.");
                }

                var parametros = new SinIdiomasEventArgs { Mensaje = "El agente no sabe ningún idioma, por lo que necesita que le digas en qué idioma está escrito este documento." };
                SolicitarIdioma(this, parametros);

                if (string.IsNullOrWhiteSpace(parametros.NombreIdioma))
                {
                    throw new InvalidOperationException("El agente no sabe ningún idioma, preguntó pero no le indicaron ningún idioma de referencia.");
                }

                var idioma = new Idioma { Nombre = parametros.NombreIdioma };
                await Recordar(idioma);
                await Recordar(documento, idioma, cienPorcientoSeguro: true);

            }
            else if (documento.IdiomaId == null)
            {
                // TODO: Analizar
            }

            return documento;
            
        }

        private async Task Recordar(Idioma idioma)
        {
            idioma.Id = await repositorio.Insertar(idioma);
        }

        private async Task Recordar(Documento documento, Idioma idioma, bool cienPorcientoSeguro = false)
        {
            await repositorio.ActualizarLetrasExistentes(documento, idioma);
            await repositorio.ActualizarPalabrasExistentes(documento, idioma);
            await repositorio.CopiarLetrasNuevas(documento, idioma);
            await repositorio.CopiarPalabrasNuevas(documento, idioma);
            await repositorio.ActualizarTotales(idioma);
            await repositorio.ActualizarPorcentajeEnLetras(idioma);
            await repositorio.ActualizarPorcentajeEnPalabras(idioma);

            if (cienPorcientoSeguro)
            {
                documento.Confianza = 100;
                await repositorio.EliminarLetras(documento);
                await repositorio.EliminarPalabras(documento);
            }

            documento.IdiomaId = idioma.Id;

            await repositorio.Actualizar(documento);

        }

        private async Task<Documento> ParsearDocumento()
        {
            var cronometro = Stopwatch.StartNew();

            var parser = new Parser(archivo);

            parser.BytesLeidos += CuandoElParserProceseBytes;
            var documentoProcesado = await parser.Procesar();
            parser.BytesLeidos -= CuandoElParserProceseBytes;

            NotificarAvanceGlobal(1);
            cronometro.Stop();

            Debug.WriteLine("Documento procesado en {0} milisegundos", cronometro.ElapsedMilliseconds);

            cronometro.Start();

            var hash = CancularHash(documentoProcesado.Palabras);
            var palabrasDistintas = documentoProcesado.Palabras.Count;
            var documento = await repositorio.ObtenerDocumento(hash, palabrasDistintas);

            NotificarAvanceGlobal(2);

            if (documento == null)
            {
                documento = new Documento
                {
                    Letras = documentoProcesado.LetrasCantidad,
                    LetrasDistintas = documentoProcesado.Letras.Count,
                    Palabras = documentoProcesado.PalabrasCantidad,
                    PalabrasDistintas = documentoProcesado.Palabras.Count,
                    Signos = documentoProcesado.SignosCantidad,
                    SignosDistintos = documentoProcesado.Signos.Count,
                    Simbolos = documentoProcesado.SimbolosCantidad,
                    SimbolosDistintos = documentoProcesado.Simbolos.Count,
                    Hash = hash
                };

                documento.Id = await repositorio.Insertar(documento);

                int x = 0;

                foreach (var letra in documentoProcesado.Letras)
                {
                    await repositorio.Insertar(new DocumentoLetra(letra, documento));
                    x++;
                    NotificarAvanceParcial(x, documento.LetrasDistintas);
                }

                NotificarAvanceGlobal(3);
                x = 0;

                foreach (var signo in documentoProcesado.Signos)
                {
                    await repositorio.Insertar(new DocumentoSigno(signo, documento));
                    x++;
                    NotificarAvanceParcial(x, documento.SignosDistintos);
                }

                NotificarAvanceGlobal(4);
                x = 0;

                foreach (var simbolo in documentoProcesado.Simbolos)
                {
                    await repositorio.Insertar(new DocumentoSimbolo(simbolo, documento));
                    x++;
                    NotificarAvanceParcial(x, documento.SimbolosDistintos);
                }

                NotificarAvanceGlobal(5);
                x = 0;

                foreach (var palabra in documentoProcesado.Palabras)
                {
                    await repositorio.Insertar(new DocumentoPalabra(palabra, documento));
                    x++;
                    NotificarAvanceParcial(x, documento.PalabrasDistintas);
                }

            }

            NotificarAvanceGlobal(6);


            cronometro.Stop();

            Debug.WriteLine("Documento guardado en la base de datos en {0} milisegundos", cronometro.ElapsedMilliseconds);

            return documento;
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

        private void NotificarAvanceGlobal(double paso)
        {
            if (AvanceGlobal != null)
            {
                AvanceGlobal(this, new AvanceEventArgs { Porcentaje = (paso / totalPasos * 100) });
            }
        }

        public void Dispose()
        {
            repositorio.Dispose();
        }
    }
}
