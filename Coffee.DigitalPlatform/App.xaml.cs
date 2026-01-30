using Coffee.DigitalPlatform.DataAccess;
using Coffee.DigitalPlatform.IDataAccess;
using Coffee.DigitalPlatform.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.ServiceProcess;
using System.Windows;

namespace Coffee.DigitalPlatform
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var services = new ServiceCollection();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<LoginViewModel>();
            services.AddSingleton<ILocalDataAccess, LocalDataAccess>();

            services.AddSingleton<ConfigureComponentViewModel>();

            services.AddSingleton<MonitorComponentViewModel>();

            var serviceProvider = services.BuildServiceProvider();

            var locator = this.TryFindResource("locator") as ViewModelLocator;
            if (locator == null)
                throw new NullReferenceException("ViewModelLocator is not found.");
            locator.Provider = serviceProvider;
        }
    }

}
