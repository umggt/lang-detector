﻿using LangDetector.Core;
using System.Windows;

namespace LangDetector
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            BaseDeDatos.CrearSiNoExiste();
        }
    }
}
