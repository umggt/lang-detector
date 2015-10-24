using System.IO;

namespace LangDetector.Tools
{
    /// <summary>
    /// Clase Resources que permite acceder a los archivos incluídos en el proyecto como recursos.
    /// </summary>
    class Resources
    {

        /// <summary>
        /// Lee todo el texto de un archivo incrustado en el proyecto.
        /// </summary>
        /// <param name="resourceName">Nombre del recurso (ruta del archivo relativa al proyecto)</param>
        /// <returns>Retorna todo el texto contenido en el archivo.</returns>
        public static string ObtenerTexto(string resourceName)
        {
            using (var sr = new StreamReader(typeof(Resources).Assembly.GetManifestResourceStream(resourceName)))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
