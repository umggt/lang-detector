using LangDetector.Core;
using LangDetector.Core.Events;
using Microsoft.Win32;
using System;
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

            var agente = new Agente(ruta);
            agente.SolicitarIdioma += SolicitarIdioma;

            try
            {
                await agente.IdentificarIdioma();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al identificar el idioma", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            agente.SolicitarIdioma -= SolicitarIdioma;
        }

        private void SolicitarIdioma(object sender, SinIdiomasEventArgs e)
        {
            var ventana = new SolicitarIdiomaWindow();
            ventana.EstablecerMensaje(e.Mensaje);
            var result = ventana.ShowDialog();

            if (result == true)
            {
                e.NombreIdioma = ventana.ObtenerNombreIdioma();
            }
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
