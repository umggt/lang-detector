using System.IO;

namespace LangDetector.Tools
{
    class Resources
    {
        public static string ObtenerTexto(string resourceName)
        {
            using (var sr = new StreamReader(typeof(Resources).Assembly.GetManifestResourceStream(resourceName)))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
