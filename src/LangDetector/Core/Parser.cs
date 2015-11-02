using LangDetector.Core.Events;
using LangDetector.Core.Modelos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LangDetector.Core
{
    /// <summary>
    /// Clase Parser que se encarga unicamente de leer un archivo de texto
    /// extraer las letras que este contenga y la cantidad de veces que cada
    /// letra aparece en el documento.
    /// </summary>
    class Parser
    {
        // Cantidad de bytes que se leeran a la vez cuando se analiza un archivo.
        // Asi si el archivo es muy grande no se carga todo a memoria, sino que
        // solo se carga en pedazos de 4096 bytes.
        private const int bufferSize = 4096;

        // Stream que permite acceder al archivo para leerlo.
        private readonly string rutaArchivo;

        // Tamaño del archvio en bytes.
        private readonly long pesoArchivo;

        // Evento al que se pueden suscribir quienes utilicen la clase Parser
        // para ir notificando el avance, por ejemplo para mostrar un progressbar
        // en la Interfaz Gráfica
        public event EventHandler<BytesLeidosEventArgs> BytesLeidos;

        /// <summary>
        /// Constructor de la clase parser.
        /// </summary>
        /// <param name="rutaArchivo">Stream para acceder al archivo que se quiere evaluar.</param>
        public Parser(string rutaArchivo)
        {
            this.rutaArchivo = rutaArchivo;
            pesoArchivo = ObtenerPesoArchivo();
        }

        private int ObtenerPesoArchivo()
        {
            int total = 0;
            using (var reader = new StreamReader(rutaArchivo, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: bufferSize))
            {
                var buffer = new char[bufferSize];
                var count = 0;

                do
                {
                    count = reader.Read(buffer, 0, bufferSize);
                    total += count;
                } while (count != 0);
            }
            return total;
        }

        /// <summary>
        /// Lee el archivo en pedazos de 4096 bytes, y llena el diccionario indicando
        /// que letras existen en el archivo y cuantas veces aparece cada una de ellas.
        /// </summary>
        /// <returns>
        /// Retorna una tarea ya que es un proceso que puede ejecutarse en otro 
        /// hilo para no bloquear la UI.
        /// </returns>
        public async Task<DocumentoProcesado> Procesar()
        {
            var palabras = new SortedDictionary<string, int>();
            var letras = new SortedDictionary<char, int>();
            var signos = new SortedDictionary<char, int>();
            var simbolos = new SortedDictionary<char, int>();

            int palabrasCantidad = 0;
            int letrasCantidad = 0;
            int signosCantidad = 0;
            int simbolosCantidad = 0;

            int bytesLeidos = 0;

            // Se asume que todos los archivos tendrán encoding de UTF8
            // a menos que el sistema pueda detectarlo por el order de bytes.
            using (var reader = new StreamReader(rutaArchivo, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: bufferSize))
            {
                var letrasPalabra = new List<char>();
                var buffer = new char[bufferSize];
                var count = 0;

                do
                {
                    // se lee un fragmento de 4096 bytes del archivo
                    count = await reader.ReadAsync(buffer, 0, bufferSize);


                    // Se analiza cada una de esas 4096 letras leídas
                    // y si no se encuentran en el diccionario aún se
                    // agregan con valor de uno,
                    // pero si la letra ya existe en el diccionario,
                    // solo se incrementa en uno su valor.
                    for (int i = 0; i < count; i++)
                    {
                        var caracter = buffer[i];

                        if (char.IsLetter(caracter))
                        {
                            letrasCantidad++;

                            letrasPalabra.Add(caracter);

                            if (letras.ContainsKey(caracter))
                            {
                                letras[caracter]++;
                            }
                            else
                            {
                                letras.Add(caracter, 1);
                            }
                        }
                        else if (letrasPalabra.Count > 0)
                        {

                            if (letrasPalabra.Count <= 5)
                            {
                                palabrasCantidad++;

                                var palabra = new string(letrasPalabra.ToArray()).ToLowerInvariant();

                                if (palabras.ContainsKey(palabra))
                                {
                                    palabras[palabra]++;
                                }
                                else
                                {
                                    palabras.Add(palabra, 1);
                                }
                            }

                            letrasPalabra.Clear();

                        }

                        if (char.IsPunctuation(caracter))
                        {
                            signosCantidad++;

                            if (signos.ContainsKey(caracter))
                            {
                                signos[caracter]++;
                            }
                            else
                            {
                                signos.Add(caracter, 1);
                            }
                        }

                        if (char.IsSymbol(caracter))
                        {

                            simbolosCantidad++;

                            if (simbolos.ContainsKey(caracter))
                            {
                                simbolos[caracter]++;
                            }
                            else
                            {
                                simbolos.Add(caracter, 1);
                            }
                        }
                    }

                    // se lleva la cuenta de la cantidad de bytes que se van leyendo hasta el momento.
                    bytesLeidos += count;

                    // Se notifica el avance con la nueva cantidad de bytes para que la UI pueda
                    // conocer el avance y actualizar un progressbar o algo por el estilo.
                    NotificarAvance(bytesLeidos);

                } while (count != 0);

                NotificarAvance(pesoArchivo);
            }

            return new DocumentoProcesado
            {
                Letras = letras,
                Palabras = palabras,
                Signos = signos,
                Simbolos = simbolos,
                CantidadLetras = letrasCantidad,
                CantidadPalabras = palabrasCantidad,
                CantidadSignos = signosCantidad,
                CantidadSimbolos = simbolosCantidad
            };
        }

        /// <summary>
        /// Verifica si existen funciones suscritas al evento BytesLeidos y las invoca.
        /// </summary>
        /// <param name="bytes">Cantidad de bytes que se han leído del documento, esta cantidad puede compararse con el peso del archivo para saber el porcentaje de avance.</param>
        private void NotificarAvance(long bytes)
        {
            if (BytesLeidos != null)
            {
                var args = new BytesLeidosEventArgs { BytesLeidos = bytes, BytesTotales = pesoArchivo };
                BytesLeidos(this, args);
            }
        }
        
    }
}
