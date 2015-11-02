﻿using LangDetector.Core.Events;
using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;

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

            var entrenar = mnuEntrenamiento.IsChecked;
            var ruta = RutaArchivoTextBox.Text;


            try
            {
                var agente = new Agente(ruta, entrenar);

                agente.SolicitarIdioma += SolicitarIdioma;
                agente.AvanceParcial += AvanceParcial;
                agente.AvanceGlobal += AvanceGlobal;
                var result = await await Task.Factory.StartNew(agente.IdentificarIdioma);
                agente.SolicitarIdioma -= SolicitarIdioma;
                agente.AvanceParcial -= AvanceParcial;
                agente.AvanceGlobal -= AvanceGlobal;
                MessageBox.Show("Todo OK " + result.First().Idioma + " " + result.First().Certeza.ToString("P"), "Todo OK", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al identificar el idioma", MessageBoxButton.OK, MessageBoxImage.Error);
            }


        }

        private void AvanceGlobal(object sender, AvanceEventArgs e)
        {
            Dispatcher.Invoke(() => {
                ProgressBar.Value = e.Porcentaje;
            });
            
        }

        private void AvanceParcial(object sender, AvanceEventArgs e)
        {
            Dispatcher.Invoke(() => {
                ProgressBar2.Value = e.Porcentaje;
            });
        }

        private void SolicitarIdioma(object sender, SolicitarIdiomaEventArgs e)
        {
            Dispatcher.Invoke(() => {
                var ventana = new SolicitarIdiomaWindow();
                ventana.Height = 300;

                ventana.EstablecerParametros(e);
                
                var result = ventana.ShowDialog();

                if (result == true)
                {
                    e.Idioma = ventana.ObtenerIdioma();
                }
            });
        }
        
    }
}
