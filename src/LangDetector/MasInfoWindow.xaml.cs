using LangDetector.Core.Modelos;
using MahApps.Metro.Controls;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
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
    /// Interaction logic for MasInfoWindow.xaml
    /// </summary>
    public partial class MasInfoWindow : MetroWindow
    {

        public IEnumerable<IdentificacionResultado> Datos { get; set; }

        public MasInfoWindow()
        {
            InitializeComponent();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {

            if (Datos == null || Datos.Count() == 0)
            {
                return;
            }

            var plotModel1 = new PlotModel();
            plotModel1.Title = "Idiomas";

            var categoryAxis1 = new CategoryAxis();
            categoryAxis1.Position = AxisPosition.Left;
            plotModel1.Axes.Add(categoryAxis1);

            var linearAxis1 = new LinearAxis();
            linearAxis1.AxislineStyle = LineStyle.Solid;
            linearAxis1.Position = AxisPosition.Bottom;
            plotModel1.Axes.Add(linearAxis1);
            var barSeries1 = new BarSeries();

            foreach (var item in Datos)
            {
                categoryAxis1.ActualLabels.Add(item.Idioma);
                barSeries1.Items.Add(new BarItem(item.Certeza));
            }
            

            plotModel1.Series.Add(barSeries1);

            Grafica.Model = plotModel1;

        }
    }
}
