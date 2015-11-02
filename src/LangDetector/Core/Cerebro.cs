﻿using LangDetector.Core.Modelos;
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
            Memoria = new Memoria();
        }

        public string RecordarDocumento(string hash)
        {
            foreach (var doc in Memoria.Documentos.Select(x => x.Value))
            {
                if (doc.Hash == hash)
                {
                    return doc.Idioma;
                }
            }

            return null;
        }

        public int CuantosIdiomasConozco()
        {
            return Memoria.CantidadIdiomas;
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

            documentoProcesado.Idioma = nombreIdioma;
            RegistrarDocumentoProcesado(documentoProcesado);

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

                    Palabra palabra = Memoria.Palabras[palabraDoc.Key];

                    double cantidadEnIdioma = palabra.Idiomas.ContainsKey(idioma.Nombre) ? palabra.Idiomas[idioma.Nombre] : 0;

                    double probabilidadPalabra = cantidadEnIdioma / idioma.CantidadDocumentos;
                    double probabilidadInversa = (palabra.Cantidad - cantidadEnIdioma) / (Memoria.CantidadDocumentos - idioma.CantidadDocumentos);
                    double bayes = probabilidadPalabra / (probabilidadPalabra + probabilidadInversa); ;
                    

                    bayes = ((1 * 0.5) + (palabra.Cantidad * bayes)) / (1 + palabra.Cantidad);

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

        private void RegistrarDocumentoProcesado(DocumentoProcesado documentoProcesado)
        {
            var documento = new Documento()
            {
                Hash = documentoProcesado.Hash,
                Idioma = documentoProcesado.Idioma,
                Palabras = documentoProcesado.CantidadPalabras,
                Letras = documentoProcesado.CantidadLetras,
                Signos = documentoProcesado.CantidadSignos,
                Simbolos = documentoProcesado.CantidadSimbolos,

                PalabrasDistintas = documentoProcesado.Palabras.Count,
                LetrasDistintas = documentoProcesado.Letras.Count,
                SignosDistintos = documentoProcesado.Signos.Count,
                SimbolosDistintos = documentoProcesado.Simbolos.Count
            };

            Memoria.Documentos.Add(documento.Hash, documento);
        }

        private void RegistrarPalabraSiNoExiste(Idioma idioma, string textPalabra, int cantidad)
        {
            Palabra palabra;

            if (Memoria.Palabras.ContainsKey(textPalabra))
            {
                palabra = Memoria.Palabras[textPalabra];
                palabra.Cantidad += cantidad;
                palabra.Documentos++;
            }
            else
            {
                palabra = new Palabra { Texto = textPalabra, Cantidad = cantidad, Documentos = 1 };
                Memoria.Palabras.Add(palabra.Texto, palabra);
            }

            if (palabra.Idiomas.ContainsKey(idioma.Nombre))
            {
                palabra.Idiomas[idioma.Nombre]++;
            }
            else
            {
                palabra.Idiomas.Add(idioma.Nombre, 1);
            }

        }

        private void RegistrarLetraSiNoExiste(Idioma idioma, char caracter, int cantidad)
        {
            Letra letra;

            if (Memoria.Letras.ContainsKey(caracter))
            {
                letra = Memoria.Letras[caracter];
                letra.Cantidad += cantidad;
                letra.Documentos++;
            }
            else
            {
                letra = new Letra { Caracter = caracter, Cantidad = cantidad, Documentos = 1 };
                Memoria.Letras.Add(letra.Caracter, letra);
            }

            if (letra.Idiomas.ContainsKey(idioma.Nombre))
            {
                letra.Idiomas[idioma.Nombre]++;
            }
            else
            {
                letra.Idiomas.Add(idioma.Nombre, 1);
            }

        }

        private void RegistrarSignoSiNoExiste(Idioma idioma, char caracter, int cantidad)
        {
            Signo signo;

            if (Memoria.Signos.ContainsKey(caracter))
            {
                signo = Memoria.Signos[caracter];
                signo.Cantidad += cantidad;
                signo.Documentos++;
            }
            else
            {
                signo = new Signo { Caracter = caracter, Cantidad = cantidad, Documentos = 1 };
                Memoria.Signos.Add(signo.Caracter, signo);
            }

            if (signo.Idiomas.ContainsKey(idioma.Nombre))
            {
                signo.Idiomas[idioma.Nombre]++;
            }
            else
            {
                signo.Idiomas.Add(idioma.Nombre, 1);
            }
        }

        private void RegistrarSimboloSiNoExiste(Idioma idioma, char caracter, int cantidad)
        {
            Simbolo simbolo;

            if (Memoria.Simbolos.ContainsKey(caracter))
            {
                simbolo = Memoria.Simbolos[caracter];
                simbolo.Cantidad += cantidad;
                simbolo.Documentos++;
            }
            else
            {
                simbolo = new Simbolo { Caracter = caracter, Cantidad = cantidad, Documentos = 1 };
                Memoria.Simbolos.Add(simbolo.Caracter, simbolo);
            }

            if (simbolo.Idiomas.ContainsKey(idioma.Nombre))
            {
                simbolo.Idiomas[idioma.Nombre]++;
            }
            else
            {
                simbolo.Idiomas.Add(idioma.Nombre, 1);
            }

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