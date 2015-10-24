using LangDetector.Core;
using LangDetector.Core.Events;
using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace LangDetector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SeleccionarArchivoButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog {
                DefaultExt = ".txt",
                Filter = "Archivos de texto (*.txt)|*.txt",
                CheckFileExists = true
            };

            var result = openFileDialog.ShowDialog();

            if (result == true && File.Exists(openFileDialog.FileName))
            {
                RutaArchivoTextBox.Text = openFileDialog.FileName;
                ProcesarButton.IsEnabled = true;
            }
            else
            {
                RutaArchivoTextBox.Text = "Seleccione un archivo de texto";
                ProcesarButton.IsEnabled = false;
            }
                
        }

        private async void ProcesarButton_Click(object sender, RoutedEventArgs e)
        {
            ProcesarButton.IsEnabled = false;
            ProgressBar.Value = 0;

            var ruta = RutaArchivoTextBox.Text;
            IDictionary<char, int> letras;
            int totalLetras;

            // Se parsea el documento para obtener las letras
            using (var fileReader = File.OpenRead(ruta))
            {
                var parser = new Parser(fileReader);

                // se suscribe al evento BytesLeidos del parser
                // para que ejecute la función CuandoSeLeanBytes.
                parser.BytesLeidos += CuandoSeLeanBytes;

                // Se ejecuta la función procesar y se espera a que termine.
                await parser.Procesar();

                // Se elimina la suscripción del evento BytesLeidos.
                parser.BytesLeidos -= CuandoSeLeanBytes;

                letras = parser.ObtenerLetras();
                totalLetras = parser.ObtenerTotalLetras();
            }

            // Se muestra el resultado como un texto
            MostrarResultado(letras, totalLetras);
            
        }

        private void CuandoSeLeanBytes(object sender, BytesLeidosEventArgs e)
        {
            var porcentajeDeAvance = ((double)e.BytesLeidos / e.BytesTotales) * 100;
            ProgressBar.Value = porcentajeDeAvance;
        }

        private void MostrarResultado(IDictionary<char, int> letras, int totalLetras)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Codigo Letra Cantidad Porcentaje");
            sb.AppendLine("------ ----- -------- ----------");

            foreach (var valor in letras)
            {
                var letra = valor.Key;
                var codigo = (int)letra;
                var cantidad = valor.Value;
                var porcentaje = ((double)cantidad / totalLetras) * 100;

                sb.AppendFormat("{0,6} {1,5} {2,8} {3,10}%", codigo, letra, cantidad, porcentaje.ToString("N2"));
                sb.AppendLine();
            }

            ResultadoTextBox.Text = sb.ToString();
        }
    }
}
