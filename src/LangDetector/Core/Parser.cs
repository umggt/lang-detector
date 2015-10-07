using LangDetector.Core.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LangDetector.Core
{
    public class Parser
    {
        // Cantidad de bytes que se leeran a la vez cuando se analiza un archivo.
        // Asi si el archivo es muy grande no se carga todo a memoria, sino que
        // solo se carga en pedazos de 1024 bytes.
        private const int bufferSize = 1024;

        // Cantidad de letras distintas que puede entender nuestro algoritmo
        // ver la función Considerar para mas detalles.
        private const int capacity = 85;

        // Diccionario que contendrá el ascii de cada letra que tiene el archivo
        // y la cantidad de veces que esta aparece en el mismo.
        private readonly IDictionary<int, int> letras;

        // Stream que permite acceder al archivo para leerlo.
        private readonly Stream archivo;

        // Tamaño del archvio en bytes.
        private readonly long pesoArchivo;

        public event EventHandler<BytesLeidosEventArgs> BytesLeidos;

        /// <summary>
        /// Constructor de la clase parser.
        /// </summary>
        /// <param name="archivo">Stream para acceder al archivo que se quiere evaluar.</param>
        public Parser(Stream archivo)
        {
            this.archivo = archivo;
            pesoArchivo = archivo.Length;
            letras = new Dictionary<int, int>(capacity);
        }

        /// <summary>
        /// Lee el archivo en pedazos de 1024 bytes, y llena el diccionario indicando
        /// que letras existen en el archivo y cuantas veces aparece cada una de ellas.
        /// </summary>
        /// <returns>
        /// Retorna una tarea ya que es un proceso que puede ejecutarse en otro 
        /// hilo para no bloquear la UI.
        /// </returns>
        public async Task Procesar()
        {
            int bytesLeidos = 0;

            // Se asume que todos los archivos tendrán encoding de UTF8
            // a menos que el sistema pueda detectarlo por el order de bytes.
            using (var reader = new StreamReader(archivo, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: bufferSize))
            {
                var buffer = new char[bufferSize];
                var count = 0;
                do
                {
                    // se lee un fragmento de 1024 bytes del archivo
                    count = await reader.ReadAsync(buffer, 0, bufferSize);


                    // Se analiza cada una de esas 1024 letras leídas
                    // y si no se encuentran en el diccionario aún se
                    // agregan con valor de uno,
                    // pero si la letra ya existe en el diccionario,
                    // solo se incrementa en uno su valor.
                    for (int i = 0; i < count; i++)
                    {
                        var letra = buffer[i];

                        if (!Considerar(letra))
                        {
                            continue;
                        }

                        if (letras.ContainsKey(letra))
                        {
                            letras[letra]++;
                        }
                        else
                        {
                            letras.Add(letra, 1);
                        }
                    }

                    // se lleva la cuenta de la cantidad de bytes que se van leyendo hasta el momento.
                    bytesLeidos += count;

                    // Se notifica el avance con la nueva cantidad de bytes para que la UI pueda
                    // conocer el avance y actualizar un progressbar o algo por el estilo.
                    NotificarAvance(bytesLeidos);
                    System.Diagnostics.Trace.WriteLine(string.Format("Count {0}, {1} bytes", count, letras.Count));

                } while (count == bufferSize);
                
            }
        }

        private void NotificarAvance(int bytes)
        {
            if (BytesLeidos != null)
            {
                var args = new BytesLeidosEventArgs { BytesLeidos = bytes, BytesTotales = pesoArchivo };
                BytesLeidos(this, args);
            }
        }

        /// <summary>
        /// Indica si la letra debe considerarse para evaluarse o si debe ignorarse,
        /// por ejemplo, para letras a,b,c retornará true y para espacios en blanco, 
        /// enter, etc. retornará false.
        /// </summary>
        /// <param name="letra">Letra que se evaluará</param>
        /// <returns>Retorna true si se debe tomar en cuenta la letra, o false si se debe ignorar.</returns>
        public static bool Considerar(int letra)
        {
            // Extremos de los símbolos
            if (letra < 65 || letra > 165)
            {
                return false;
            }

            // Rangos de letras a considerar:

            // Mayúsculas (26 posibilidades)
            if (letra >= 65 && letra <= 90)
            {
                return true;
            }

            // Minusculas (26 posibilidades)
            if (letra >= 97 && letra <= 122)
            {
                return true;
            }

            // Diacríticos (letras tildadas, con dieresis, etc.)
            // (27 posibilidades)
            if (letra >= 128 && letra <= 154)
            {
                return true;
            }

            // Diacríticos 2 (letras tildadas incluyendo la enye)
            // (6 posibilidades)
            if (letra >= 160 && letra <= 165)
            {
                return true;
            }

            return false;
        }

        
    }
}
