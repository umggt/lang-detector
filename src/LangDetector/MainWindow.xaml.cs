using LangDetector.Core;
using LangDetector.Core.Events;
using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
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

            var entrenar = mnuEntrenamiento.IsChecked;
            var ruta = RutaArchivoTextBox.Text;

            using (var agente = new Agente(ruta, entrenar))
            {
                agente.SolicitarIdioma += SolicitarIdioma;
                agente.AvanceParcial += AvanceParcial;
                agente.AvanceGlobal += AvanceGlobal;

                try
                {
                    var result = await await Task.Factory.StartNew(agente.IdentificarIdioma);
                    
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
                ventana.Height = 250;

                ventana.EstablecerParametros(e);
                
                var result = ventana.ShowDialog();

                if (result == true)
                {
                    var idioma = ventana.ObtenerIdioma();
                    if (idioma.Id > 0)
                    {
                        e.IdiomaId = idioma.Id;
                    }
                    e.IdiomaNombre = idioma.Nombre;
                }
            });
        }
        
    }
}
