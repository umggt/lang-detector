using LangDetector.Core.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LangDetector.Core
{
    class Cerebro
    {
        public Memoria Memoria { get; set; }

        public Cerebro()
        {
            Memoria = new Memoria(cargar: true);
        }

        public Idioma RecordarDocumento(string hash)
        {
            foreach (var idioma in Memoria.Idiomas.Select(x => x.Value))
            {
                foreach (var doc in idioma.Documentos.Select(x => x.Value))
                {
                    if (doc.Hash == hash)
                    {
                        return idioma;
                    }
                }
            }

            return null;
        }

        public int CuantosIdiomasConozco()
        {
            return Memoria.Idiomas.Count;
        }

        public void Entrena(DocumentoProcesado documentoProcesado, string nombreIdioma)
        {
            nombreIdioma = nombreIdioma.ToLowerInvariant();

            Idioma idioma = RegistrarIdiomaSiNoExiste(nombreIdioma);

            idioma.CantidadDocumentos++;

            idioma.CantidadPalabras += documentoProcesado.CantidadPalabras;
            idioma.CantidadLetras += documentoProcesado.CantidadLetras;
            idioma.CantidadSimbolos += documentoProcesado.CantidadSimbolos;
            idioma.CantidadSignos += documentoProcesado.CantidadSignos;

            Memoria.CantidadDocumentos++;
            Memoria.CantidadPalabras += documentoProcesado.CantidadPalabras;
            Memoria.CantidadLetras += documentoProcesado.CantidadLetras;
            Memoria.CantidadSimbolos += documentoProcesado.CantidadSimbolos;
            Memoria.CantidadSignos += documentoProcesado.CantidadSignos;

            foreach (var palabra in documentoProcesado.Palabras)
            {
                RegistrarPalabraSiNoExiste(idioma, palabra.Key, palabra.Value);
            }

            foreach (var letra in documentoProcesado.Letras)
            {
                RegistrarLetraSiNoExiste(idioma, letra.Key, letra.Value);
            }

            foreach (var signo in documentoProcesado.Signos)
            {
                RegistrarSignoSiNoExiste(idioma, signo.Key, signo.Value);
            }

            foreach (var simbolo in documentoProcesado.Simbolos)
            {
                RegistrarSimboloSiNoExiste(idioma, simbolo.Key, simbolo.Value);
            }

            RegistrarDocumentoProcesado(documentoProcesado, idioma);

            foreach (var otroIdioma in Memoria.Idiomas.Select(x => x.Value))
            {
                if (otroIdioma.Nombre != idioma.Nombre)
                {
                    otroIdioma.CantidadInversaDocumentos += idioma.CantidadDocumentos;
                    otroIdioma.CantidadInversaPalabras += idioma.CantidadPalabras;
                    otroIdioma.CantidadInversaLetras += idioma.CantidadLetras;
                    otroIdioma.CantidadInversaSignos += idioma.CantidadSignos;
                    otroIdioma.CantidadInversaSimbolos += idioma.CantidadSimbolos;

                    idioma.CantidadInversaDocumentos += otroIdioma.CantidadDocumentos;
                    idioma.CantidadInversaPalabras += otroIdioma.CantidadPalabras;
                    idioma.CantidadInversaLetras += otroIdioma.CantidadLetras;
                    idioma.CantidadInversaSignos += otroIdioma.CantidadSignos;
                    idioma.CantidadInversaSimbolos += otroIdioma.CantidadSimbolos;
                }
            }

            Memoria.RecordarALargoPlazo();

            documentoProcesado.Idioma = nombreIdioma;
        }

        public IEnumerable<IdentificacionResultado> Identifica(DocumentoProcesado documentoProcesado)
        {

            var resultados = new List<IdentificacionResultado>(Memoria.Idiomas.Count);

            foreach (var idioma in Memoria.Idiomas.Select(x => x.Value))
            {
                double logSum = 0;
                double probabilidadDoc = idioma.CantidadDocumentos / (double)Memoria.CantidadDocumentos;

                foreach (var palabraDoc in documentoProcesado.Palabras)
                {
                    if (!Memoria.Palabras.ContainsKey(palabraDoc.Key))
                    {
                        continue;
                    }

                    Palabra palabra;

                    if (idioma.Palabras.ContainsKey(palabraDoc.Key))
                    {
                        palabra = idioma.Palabras[palabraDoc.Key];
                    }
                    else
                    {
                        palabra = new Palabra();
                    }

                    double probabilidadPalabra = 0;
                    double probabilidadInversa = 0;
                    double bayes = 0;

                    if (idioma.CantidadDocumentos > 0)
                    {
                        probabilidadPalabra = palabra.CantidadEnIdioma / (double)idioma.CantidadDocumentos;
                    }

                    if (idioma.CantidadInversaDocumentos > 0)
                    {
                        probabilidadInversa = palabra.CantidadOtrosIdiomas / (double)idioma.CantidadInversaDocumentos;
                    }

                    if ((probabilidadPalabra + probabilidadInversa) > 0)
                    {
                        bayes = probabilidadPalabra / (probabilidadPalabra + probabilidadInversa);
                    }

                    bayes = ((1 * 0.5) + (Memoria.Palabras[palabraDoc.Key] * bayes)) / (1 + Memoria.Palabras[palabraDoc.Key]);

                    if (bayes == 0)
                    {
                        bayes = 0.01;
                    }
                    else if (bayes == 1)
                    {
                        bayes = 0.99;
                    }

                    logSum += (Math.Log(1 - bayes) - Math.Log(bayes));

                }

                resultados.Add(new IdentificacionResultado
                {
                    Idioma = idioma.Nombre,
                    Certeza = 1 / (1 + Math.Exp(logSum))
                });
            }

            return resultados.OrderByDescending(x => x.Certeza).ToArray();
        }

        private static void RegistrarDocumentoProcesado(DocumentoProcesado documentoProcesado, Idioma idioma)
        {
            var documento = new Documento()
            {
                Hash = documentoProcesado.Hash,

                Palabras = documentoProcesado.CantidadPalabras,
                Letras = documentoProcesado.CantidadLetras,
                Signos = documentoProcesado.CantidadSignos,
                Simbolos = documentoProcesado.CantidadSimbolos,

                PalabrasDistintas = documentoProcesado.Palabras.Count,
                LetrasDistintas = documentoProcesado.Letras.Count,
                SignosDistintos = documentoProcesado.Signos.Count,
                SimbolosDistintos = documentoProcesado.Simbolos.Count
            };

            idioma.Documentos.Add(documento.Hash, documento);
        }

        private void RegistrarPalabraSiNoExiste(Idioma idioma, string textPalabra, int cantidad)
        {
            Palabra palabra;

            if (idioma.Palabras.ContainsKey(textPalabra))
            {
                palabra = idioma.Palabras[textPalabra];

                palabra.CantidadEnDocumentos++;
                palabra.CantidadEnIdioma += cantidad;

            }
            else
            {
                palabra = new Palabra { Texto = textPalabra, CantidadEnIdioma = cantidad, CantidadEnDocumentos = 1 };
                idioma.Palabras.Add(palabra.Texto, palabra);
            }

            var palabraOtrosIdiomas = BuscarPalabraEnOtrosIdiomas(palabra, idioma.Nombre);

            palabra.CantidadOtrosIdiomas = palabraOtrosIdiomas.CantidadOtrosIdiomas;
            palabra.CantidadOtrosDocumentos = palabraOtrosIdiomas.CantidadOtrosDocumentos;

            if (Memoria.Palabras.ContainsKey(textPalabra)) {
                Memoria.Palabras[textPalabra]++;
            }
            else
            {
                Memoria.Palabras.Add(textPalabra, 1);
            }

        }

        private void RegistrarLetraSiNoExiste(Idioma idioma, char caracter, int cantidad)
        {
            Letra letra;

            if (idioma.Letras.ContainsKey(caracter))
            {
                letra = idioma.Letras[caracter];
                letra.CantidadEnDocumentos++;
                letra.CantidadEnIdioma += cantidad;
            }
            else
            {
                letra = new Letra { Caracter = caracter, CantidadEnIdioma = cantidad, CantidadEnDocumentos = 1 };
                idioma.Letras.Add(letra.Caracter, letra);
            }

            var letraOtrosIdiomas = BuscarLetraEnOtrosIdiomas(letra, idioma.Nombre);
            letra.CantidadOtrosIdiomas = letraOtrosIdiomas.CantidadOtrosIdiomas;
            letra.CantidadOtrosDocumentos = letraOtrosIdiomas.CantidadOtrosDocumentos;

        }

        private void RegistrarSignoSiNoExiste(Idioma idioma, char caracter, int cantidad)
        {
            Signo signo;

            if (idioma.Signos.ContainsKey(caracter))
            {
                signo = idioma.Signos[caracter];
                signo.CantidadEnDocumentos++;
                signo.CantidadEnIdioma += cantidad;
            }
            else
            {
                signo = new Signo { Caracter = caracter, CantidadEnIdioma = cantidad, CantidadEnDocumentos = 1 };
                idioma.Signos.Add(signo.Caracter, signo);
            }

            var signoOtrosIdiomas = BuscarSignoEnOtrosIdiomas(signo, idioma.Nombre);
            signo.CantidadOtrosIdiomas = signoOtrosIdiomas.CantidadOtrosIdiomas;
            signo.CantidadOtrosDocumentos = signoOtrosIdiomas.CantidadOtrosDocumentos;

        }

        private void RegistrarSimboloSiNoExiste(Idioma idioma, char caracter, int cantidad)
        {
            Simbolo simbolo;

            if (idioma.Simbolos.ContainsKey(caracter))
            {
                simbolo = idioma.Simbolos[caracter];
                simbolo.CantidadEnDocumentos++;
                simbolo.CantidadEnIdioma += cantidad;
            }
            else
            {
                simbolo = new Simbolo { Caracter = caracter, CantidadEnIdioma = cantidad, CantidadEnDocumentos = 1 };
                idioma.Simbolos.Add(simbolo.Caracter, simbolo);
            }

            var simboloOtrosIdiomas = BuscarSimboloEnOtrosIdiomas(simbolo, idioma.Nombre);
            simbolo.CantidadOtrosIdiomas = simboloOtrosIdiomas.CantidadOtrosIdiomas;
            simbolo.CantidadOtrosDocumentos = simboloOtrosIdiomas.CantidadOtrosDocumentos;

        }

        private Palabra BuscarPalabraEnOtrosIdiomas(Palabra palabra, string nombreIdioma)
        {
            Palabra palabraRetorno = new Palabra();

            foreach (var otroIdioma in Memoria.Idiomas.Select(x => x.Value))
            {
                if (otroIdioma.Nombre != nombreIdioma)
                {
                    if (otroIdioma.Palabras.ContainsKey(palabra.Texto))
                    {
                        var palabraOtroIdioma = otroIdioma.Palabras[palabra.Texto];

                        palabraRetorno.CantidadOtrosIdiomas += palabraOtroIdioma.CantidadEnIdioma;
                        palabraRetorno.CantidadOtrosDocumentos += palabraOtroIdioma.CantidadEnDocumentos;

                        palabraOtroIdioma.CantidadOtrosIdiomas += palabra.CantidadEnIdioma;
                        palabraOtroIdioma.CantidadOtrosDocumentos += palabra.CantidadEnDocumentos;
                    }
                }
            }

            return palabraRetorno;
        }

        private Letra BuscarLetraEnOtrosIdiomas(Letra letra, string nombreIdioma)
        {
            Letra letraRetorno = new Letra();

            foreach (var otroIdioma in Memoria.Idiomas.Select(x => x.Value))
            {
                if (otroIdioma.Nombre != nombreIdioma)
                {
                    if (otroIdioma.Letras.ContainsKey(letra.Caracter))
                    {
                        var letraOtroIdioma = otroIdioma.Letras[letra.Caracter];

                        letraRetorno.CantidadOtrosIdiomas += letraOtroIdioma.CantidadEnIdioma;
                        letraRetorno.CantidadOtrosDocumentos += letraOtroIdioma.CantidadEnDocumentos;

                        letraOtroIdioma.CantidadOtrosIdiomas += letra.CantidadEnIdioma;
                        letraOtroIdioma.CantidadOtrosDocumentos += letra.CantidadOtrosDocumentos;
                    }
                }
            }

            return letraRetorno;
        }

        private Signo BuscarSignoEnOtrosIdiomas(Signo signo, string nombreIdioma)
        {
            Signo signoRetorno = new Signo();

            foreach (var otroIdioma in Memoria.Idiomas.Select(x => x.Value))
            {
                if (otroIdioma.Nombre != nombreIdioma)
                {
                    if (otroIdioma.Signos.ContainsKey(signo.Caracter))
                    {
                        var signoOtroIdioma = otroIdioma.Signos[signo.Caracter];

                        signoRetorno.CantidadOtrosIdiomas += signoOtroIdioma.CantidadEnIdioma;
                        signoRetorno.CantidadOtrosDocumentos += signoOtroIdioma.CantidadEnDocumentos;

                        signoOtroIdioma.CantidadOtrosIdiomas += signo.CantidadEnIdioma;
                        signoOtroIdioma.CantidadOtrosDocumentos += signo.CantidadEnDocumentos;

                    }
                }
            }

            return signoRetorno;
        }

        private Simbolo BuscarSimboloEnOtrosIdiomas(Simbolo simbolo, string nombreIdioma)
        {
            Simbolo simboloRetorno = new Simbolo();

            foreach (var otroIdioma in Memoria.Idiomas.Select(x => x.Value))
            {
                if (otroIdioma.Nombre != nombreIdioma)
                {
                    if (otroIdioma.Simbolos.ContainsKey(simbolo.Caracter))
                    {
                        var simboloOtroIdioma = otroIdioma.Simbolos[simbolo.Caracter];

                        simboloRetorno.CantidadOtrosIdiomas += simboloOtroIdioma.CantidadEnIdioma;
                        simboloRetorno.CantidadOtrosDocumentos += simboloOtroIdioma.CantidadEnDocumentos;

                        simboloOtroIdioma.CantidadOtrosIdiomas += simbolo.CantidadEnIdioma;
                        simboloOtroIdioma.CantidadOtrosDocumentos += simbolo.CantidadEnDocumentos;
                    }
                }
            }

            return simboloRetorno;
        }

        private Idioma RegistrarIdiomaSiNoExiste(string nombreIdioma)
        {
            if (Memoria.Idiomas.ContainsKey(nombreIdioma))
            {
                return Memoria.Idiomas[nombreIdioma];
            }
            else
            {
                var idioma = new Idioma { Nombre = nombreIdioma };
                Memoria.Idiomas.Add(nombreIdioma, idioma);
                Memoria.CantidadIdiomas++;
                return idioma;
            }
        }
    }
}
