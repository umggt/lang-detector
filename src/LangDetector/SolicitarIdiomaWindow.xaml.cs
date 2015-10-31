using LangDetector.Core;
using System.Windows;
using LangDetector.Core.Events;
using System.Linq;
using LangDetector.Core.Modelos;

namespace LangDetector
{
    /// <summary>
    /// Interaction logic for SolicitarIdiomaWindow.xaml
    /// </summary>
    public partial class SolicitarIdiomaWindow : Window
    {
        private SolicitarIdiomaEventArgs parametros;

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
            if (ObtenerIdioma() == null)
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

        internal Idioma ObtenerIdioma()
        {
            var item = cmbIdiomas.SelectedItem as Idioma;
            if (item != null && item.Id > 0)
            {
                return item;
            }
            else if (!string.IsNullOrWhiteSpace(txtNombreIdioma.Text))
            {
                return new Idioma { Nombre = txtNombreIdioma.Text };
            }
            else
            {
                return null;
            }
        }

      

        internal void EstablecerParametros(SolicitarIdiomaEventArgs parametros)
        {
            this.parametros = parametros;
            txtMensaje.Text = parametros.Mensaje;
            var data = (new[] { new Idioma() { Nombre = " (Nuevo) " } }).Union(parametros.Repositorio.ObtenerIdiomas());
            cmbIdiomas.ItemsSource = data;
            cmbIdiomas.SelectedItem = data.FirstOrDefault(x => x.Id == 0);

        }

        private void cmbIdiomas_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var item = cmbIdiomas.SelectedItem as Idioma;
            if (item == null || item.Id == 0)
            {
                txtNombreIdioma.IsEnabled = true;
            }
            else
            {
                txtNombreIdioma.IsEnabled = false;
            }
        }
    }
}
