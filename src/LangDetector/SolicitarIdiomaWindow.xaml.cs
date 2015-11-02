using LangDetector.Core.Events;
using System.Linq;
using System.Windows;

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

        internal string ObtenerIdioma()
        {
            var item = cmbIdiomas.SelectedItem as string;
            if (item != null && item != " (Nuevo) ")
            {
                return item;
            }
            else if (!string.IsNullOrWhiteSpace(txtNombreIdioma.Text))
            {
                return txtNombreIdioma.Text;
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
            var data = (new[] { " (Nuevo) " }).Union(parametros.Idiomas);
            cmbIdiomas.ItemsSource = data;
            cmbIdiomas.SelectedItem = data.FirstOrDefault(x => x == " (Nuevo) ");

        }

        private void cmbIdiomas_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var item = cmbIdiomas.SelectedItem as string;
            if (item == null || item == " (Nuevo) ")
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
