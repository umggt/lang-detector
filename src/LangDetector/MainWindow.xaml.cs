using LangDetector.Core.Events;
using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using MahApps.Metro.Controls;
using LangDetector.Core.Modelos;
using System.Collections;
using System.Collections.Generic;

namespace LangDetector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        private IEnumerable<IdentificacionResultado> ultimoResultado;

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

            var entrenar = mnuEntrenamiento.IsChecked;
            var ruta = RutaArchivoTextBox.Text;

            gridInfo.Visibility = Visibility.Collapsed;
            gridWarning.Visibility = Visibility.Collapsed;

            ProgressGlobal.IsActive = true;
            
            try
            {
                var agente = new Agente(ruta, entrenar);

                agente.SolicitarIdioma += SolicitarIdioma;
                agente.AvanceParcial += AvanceParcial;
                ultimoResultado = await await Task.Factory.StartNew(agente.IdentificarIdioma);
                agente.SolicitarIdioma -= SolicitarIdioma;
                agente.AvanceParcial -= AvanceParcial;

                var idioma = ultimoResultado.First();
                if (idioma.Certeza >= 0.5)
                {
                    txtInfo.Text = string.Format("Documento escrito en {0},\r\nestoy un {1:P2} seguro", idioma.Idioma, idioma.Certeza);
                }
                else
                {
                    txtInfo.Text = string.Format("Creo que el documento está escrito en {0}, pero lo dudo mucho,\r\nestoy un {1:P2} seguro", idioma.Idioma, idioma.Certeza);
                }

                if (entrenar)
                {
                    txtWarning.Text = "He extraído la información necesaria para el documento y la he aplicado para el idioma " + idioma.Idioma;
                    gridWarning.Visibility = Visibility.Visible;
                }
                else
                {
                    gridInfo.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al identificar el idioma", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            ProgressGlobal.IsActive = false;
            ProcesarButton.IsEnabled = true;
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

        private void mnuEntrenamiento_Checked(object sender, RoutedEventArgs e)
        {
            if (mnuEntrenamiento.IsChecked)
            {
                gridInfo.Visibility = Visibility.Collapsed;
                gridWarning.Visibility = Visibility.Visible;

                txtWarning.Text = "¡Advertencia!\r\nEl agente está en modo entrenamiento, por lo que creerá todo lo que le digas.";
            }
            else
            {
                gridWarning.Visibility = Visibility.Collapsed;
            }
        }

        private void AcercaDe_Click(object sender, RoutedEventArgs e)
        {
            var window = new AcercaDeWindow();
            window.ShowDialog();
        }

        private void MasInformacion_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var window = new MasInfoWindow();
            window.Datos = ultimoResultado;
            window.ShowDialog();
        }
    }
}
