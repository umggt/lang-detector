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

            using (var agente = new Agente(ruta))
            {
                agente.SolicitarIdioma += SolicitarIdioma;
                agente.AvanceParcial += AvanceParcial;
                agente.AvanceGlobal += AvanceGlobal;

                try
                {
                    await agente.IdentificarIdioma();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error al identificar el idioma", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                agente.SolicitarIdioma -= SolicitarIdioma;
                agente.AvanceParcial -= AvanceParcial;
                agente.AvanceGlobal -= AvanceGlobal;
            }
        }

        private void AvanceGlobal(object sender, AvanceEventArgs e)
        {
            ProgressBar.Value = e.Porcentaje;
        }

        private void AvanceParcial(object sender, AvanceEventArgs e)
        {
            ProgressBar2.Value = e.Porcentaje;
        }

        private void SolicitarIdioma(object sender, SinIdiomasEventArgs e)
        {
            var ventana = new SolicitarIdiomaWindow();
            ventana.Height = 220;

            ventana.EstablecerMensaje(e.Mensaje);
            var result = ventana.ShowDialog();

            if (result == true)
            {
                e.NombreIdioma = ventana.ObtenerNombreIdioma();
            }
        }
        
    }
}
