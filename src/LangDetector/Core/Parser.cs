using LangDetector.Core.Events;
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
    public class Parser
    {
        // Cantidad de bytes que se leeran a la vez cuando se analiza un archivo.
        // Asi si el archivo es muy grande no se carga todo a memoria, sino que
        // solo se carga en pedazos de 4096 bytes.
        private const int bufferSize = 4096;

        // Diccionario que contendrá el ascii de cada letra que tiene el archivo
        // y la cantidad de veces que esta aparece en el mismo.
        private readonly IDictionary<char, int> letras;

        // Cantidad de letras que posee el documento.
        private int totalLetras;

        // Stream que permite acceder al archivo para leerlo.
        private readonly Stream archivo;

        // Tamaño del archvio en bytes.
        private readonly long pesoArchivo;

        // Evento al que se pueden suscribir quienes utilicen la clase Parser
        // para ir notificando el avance, por ejemplo para mostrar un progressbar
        // en la Interfaz Gráfica
        public event EventHandler<BytesLeidosEventArgs> BytesLeidos;

        /// <summary>
        /// Constructor de la clase parser.
        /// </summary>
        /// <param name="archivo">Stream para acceder al archivo que se quiere evaluar.</param>
        public Parser(Stream archivo)
        {
            this.archivo = archivo;
            pesoArchivo = archivo.Length;
            letras = new Dictionary<char, int>();
        }

        /// <summary>
        /// Lee el archivo en pedazos de 4096 bytes, y llena el diccionario indicando
        /// que letras existen en el archivo y cuantas veces aparece cada una de ellas.
        /// </summary>
        /// <returns>
        /// Retorna una tarea ya que es un proceso que puede ejecutarse en otro 
        /// hilo para no bloquear la UI.
        /// </returns>
        public async Task Procesar()
        {
            int bytesLeidos = 0;
            totalLetras = 0;

            // Se asume que todos los archivos tendrán encoding de UTF8
            // a menos que el sistema pueda detectarlo por el order de bytes.
            using (var reader = new StreamReader(archivo, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: bufferSize))
            {
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

                        // Si el caracter no es considerado una letra, se ignora
                        if (!char.IsLetter(caracter))
                        {
                            // Nota importante, char.IsLetter considera letras en todos los idiomas,
                            // por ejemplo si es una letra china, char.IsLetter retorna true, por lo
                            // que esta función es importante para el objetivo del agente.
                            continue;
                        }

                        if (letras.ContainsKey(caracter))
                        {
                            letras[caracter]++;
                        }
                        else
                        {
                            letras.Add(caracter, 1);
                        }

                        totalLetras++;
                    }

                    // se lleva la cuenta de la cantidad de bytes que se van leyendo hasta el momento.
                    bytesLeidos += count;

                    // Se notifica el avance con la nueva cantidad de bytes para que la UI pueda
                    // conocer el avance y actualizar un progressbar o algo por el estilo.
                    NotificarAvance(bytesLeidos);

                } while (count != 0);

                NotificarAvance(pesoArchivo);

            }
        }

        /// <summary>
        /// Retorna las letras que se extrajeron el documento al procesarlo.
        /// </summary>
        /// <returns>Retorna un diccionario con la letra como indice y la cantidad de veces que aparece en el documento</returns>
        public IDictionary<char, int> ObtenerLetras()
        {
            return letras;
        }

        /// <summary>
        /// Obtiene el total de letras que posee el documento.
        /// </summary>
        /// <returns>Retorna la cantidad de letras que posee el documento en total.</returns>
        public int ObtenerTotalLetras()
        {
            return totalLetras;
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
