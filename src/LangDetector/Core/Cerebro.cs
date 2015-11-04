using LangDetector.Core.Events;
using LangDetector.Core.Modelos;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Memoria.CantidadDocumentos++;

            foreach (var palabra in documentoProcesado.Palabras)
            {
                RegistrarPalabraSiNoExiste(idioma, palabra.Key, palabra.Value);
            }

            foreach (var letra in documentoProcesado.Letras)
            {
                RegistrarLetraSiNoExiste(idioma, letra.Key, letra.Value);
            }
            
            documentoProcesado.Idioma = nombreIdioma;
            RegistrarDocumentoProcesado(documentoProcesado);

            Memoria.RecordarALargoPlazo();

        }

        public IEnumerable<IdentificacionResultado> Identifica(DocumentoProcesado documentoProcesado)
        {

            var resultados = new List<IdentificacionResultado>(Memoria.Idiomas.Count);

            foreach (var idioma in Memoria.Idiomas.Select(x => x.Value))
            {
                double logSum = 0;
                double logSum2 = 0;

                //double probabilidadDoc = idioma.CantidadDocumentos / (double)Memoria.CantidadDocumentos;

                foreach (var palabraDoc in documentoProcesado.Palabras)
                {
                    if (!Memoria.Palabras.ContainsKey(palabraDoc.Key))
                    {
                        continue;
                    }

                    Palabra palabra = Memoria.Palabras[palabraDoc.Key];

                    double cantidadEnIdioma = palabra.Idiomas.ContainsKey(idioma.Nombre) ? palabra.Idiomas[idioma.Nombre] : 0;

                    double probabilidadPalabra = cantidadEnIdioma / idioma.CantidadDocumentos;
                    double probabilidadInversa = (palabra.Documentos - cantidadEnIdioma) / (Memoria.CantidadDocumentos - idioma.CantidadDocumentos);

                    if (probabilidadPalabra + probabilidadInversa == 0)
                    {
                        continue;
                    }

                    double bayes = probabilidadPalabra / (probabilidadPalabra + probabilidadInversa);


                    bayes = ((1 * 0.5) + (palabra.Documentos * bayes)) / (1 + palabra.Documentos);

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

                foreach (var palabraDoc in documentoProcesado.Letras)
                {
                    if (!Memoria.Letras.ContainsKey(palabraDoc.Key))
                    {
                        continue;
                    }

                    Letra letra = Memoria.Letras[palabraDoc.Key];

                    double cantidadEnIdioma = letra.Idiomas.ContainsKey(idioma.Nombre) ? letra.Idiomas[idioma.Nombre] : 0;

                    double probabilidadLetra = cantidadEnIdioma / idioma.CantidadDocumentos;
                    double probabilidadInversa = (letra.Documentos - cantidadEnIdioma) / (Memoria.CantidadDocumentos - idioma.CantidadDocumentos);

                    if (probabilidadLetra + probabilidadInversa == 0)
                    {
                        continue;
                    }

                    double bayes = probabilidadLetra / (probabilidadLetra + probabilidadInversa);


                    bayes = ((1 * 0.5) + (letra.Documentos * bayes)) / (1 + letra.Documentos);

                    if (bayes == 0)
                    {
                        bayes = 0.01;
                    }
                    else if (bayes == 1)
                    {
                        bayes = 0.99;
                    }

                    logSum2 += (Math.Log(1 - bayes) - Math.Log(bayes));

                }

                resultados.Add(new IdentificacionResultado
                {
                    Idioma = idioma.Nombre,
                    Certeza = 1 / (1 + Math.Exp(logSum)),
                    Certeza2 = 1 / (1 + Math.Exp(logSum))
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
                Palabras = documentoProcesado.Palabras.Count,
                Letras = documentoProcesado.Letras.Count
            };

            Memoria.Documentos.Add(documento.Hash, documento);
        }

        private void RegistrarPalabraSiNoExiste(Idioma idioma, string textPalabra, int cantidad)
        {
            Palabra palabra;

            if (Memoria.Palabras.ContainsKey(textPalabra))
            {
                palabra = Memoria.Palabras[textPalabra];
                //palabra.Cantidad += cantidad;
                palabra.Documentos++;
            }
            else
            {
                palabra = new Palabra { Texto = textPalabra, Documentos = 1 };
                Memoria.Palabras.Add(palabra.Texto, palabra);
                Memoria.CantidadPalabras++;
            }

            if (palabra.Idiomas.ContainsKey(idioma.Nombre))
            {
                palabra.Idiomas[idioma.Nombre] += 1;
            }
            else
            {
                palabra.Idiomas.Add(idioma.Nombre, 1);
                idioma.CantidadPalabras++;
            }

            
        }

        private void RegistrarLetraSiNoExiste(Idioma idioma, char caracter, int cantidad)
        {
            Letra letra;

            if (Memoria.Letras.ContainsKey(caracter))
            {
                letra = Memoria.Letras[caracter];
                //letra.Cantidad += cantidad;
                letra.Documentos++;
            }
            else
            {
                letra = new Letra { Caracter = caracter, Documentos = 1 };
                Memoria.Letras.Add(letra.Caracter, letra);
                Memoria.CantidadLetras++;
            }

            if (letra.Idiomas.ContainsKey(idioma.Nombre))
            {
                letra.Idiomas[idioma.Nombre]++;
            }
            else
            {
                letra.Idiomas.Add(idioma.Nombre, 1);
                idioma.CantidadLetras++;
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
