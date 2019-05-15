using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using Autobox.Services;
using Autobox.Desktop.Services;

namespace Autobox.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Library = ServiceProvider.AddService<ILibrary, Library>(new Library("Library"));
            Library.LoadAllAsync().Wait();
        }

        // ##### Application singletons
        private readonly ILibrary Library;
    }
}
