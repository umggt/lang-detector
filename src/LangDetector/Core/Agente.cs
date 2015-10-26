using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LangDetector.Core.Events;
using LangDetector.Core.Modelos;
using System.Transactions;
using System.Security.Cryptography;
using System.Diagnostics;

namespace LangDetector.Core
{
    class Agente
    {
        private readonly string archivo;

        public event EventHandler<SinIdiomasEventArgs> SolicitarIdioma;

        public Agente(string archivo)
        {
            if (!File.Exists(archivo))
            {
                throw new FileNotFoundException("El archivo especificado no existe.", archivo);
            }

            this.archivo = archivo;
        }

        public async Task<Documento> IdentificarIdioma()
        {

            var documento = await ParsearDocumento();

            if (documento.Letras + documento.Signos + documento.Simbolos == 0)
            {
                throw new InvalidOperationException("El documento está vacío o contiene signos que el agente no puede procesar.");
            }

            var cantidadDeIdiomasQueConozco = await Repositorio.ContarIdiomasConocidos();

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
            else
            {
                // TODO: Analizar
            }

            return documento;
            
        }

        private async Task Recordar(Idioma idioma)
        {
            idioma.Id = await Repositorio.Insertar(idioma);
        }

        private async Task Recordar(Documento documento, Idioma idioma, bool cienPorcientoSeguro = false)
        {
            using (var transaccion = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await Repositorio.CopiarLetrasNuevas(documento, idioma);
                await Repositorio.ActualizarLetrasExistentes(documento, idioma);
                await Repositorio.ActualizarTotales(idioma);
                await Repositorio.ActualizarPorcentajeEnLetras(idioma);
                await Repositorio.ActualizarPorcentajeEnPalabras(idioma);
                await Repositorio.Asociar(documento, idioma);
                
                if (cienPorcientoSeguro)
                {
                    documento.Confianza = 100;
                    await Repositorio.Actualizar(documento);
                    await Repositorio.EliminarLetras(documento);
                }

                transaccion.Complete();
            }
            
        }

        private async Task<Documento> ParsearDocumento()
        {
            var cronometro = Stopwatch.StartNew();

            var parser = new Parser(archivo);

            parser.BytesLeidos += CuandoElParserProceseBytes;
            var documentoProcesado = await parser.Procesar();
            parser.BytesLeidos -= CuandoElParserProceseBytes;

            cronometro.Stop();

            Debug.WriteLine("Documento procesado en {0} milisegundos", cronometro.ElapsedMilliseconds);

            cronometro.Start();

            var documento = new Documento
            {
                Letras = documentoProcesado.LetrasCantidad,
                Palabras = documentoProcesado.PalabrasCantidad,
                Signos = documentoProcesado.SignosCantidad,
                Simbolos = documentoProcesado.SimbolosCantidad,
                Hash = CancularHash(documentoProcesado.Palabras)
            };
            documento.Id = await Repositorio.Insertar(documento);
            await Repositorio.Insertar(documentoProcesado.Letras.Select(valor => new DocumentoLetra(valor, documento)));
            await Repositorio.Insertar(documentoProcesado.Signos.Select(valor => new DocumentoSigno(valor, documento)));
            await Repositorio.Insertar(documentoProcesado.Simbolos.Select(valor => new DocumentoSimbolo(valor, documento)));
            await Repositorio.Insertar(documentoProcesado.Palabras.Select(valor => new DocumentoPalabra(valor, documento)));

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
            
        }
    }
}
