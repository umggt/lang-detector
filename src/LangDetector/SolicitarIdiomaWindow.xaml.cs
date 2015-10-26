using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LangDetector
{
    /// <summary>
    /// Interaction logic for SolicitarIdiomaWindow.xaml
    /// </summary>
    public partial class SolicitarIdiomaWindow : Window
    {
        public SolicitarIdiomaWindow()
        {
            InitializeComponent();
        }

        public void EstablecerMensaje(string mensaje)
        {
            txtMensaje.Text = mensaje;
        }

        private void btnAceptar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombreIdioma.Text))
            {
                MessageBox.Show("Debes ingresar el nombre del idioma.", "Datos no válidos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
            Close();
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public string ObtenerNombreIdioma()
        {
            return txtNombreIdioma.Text;
        }
    }
}
