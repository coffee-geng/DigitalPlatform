using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.ViewModels
{
    public class ViewModelLocator
    {
        public static ViewModelLocator Instance { get; private set; }

        public ViewModelLocator()
        {
            Instance = this;
        }

        public IServiceProvider Provider { get; set; }

        public MainViewModel MainViewModel => Provider.GetService<MainViewModel>();

        public LoginViewModel LoginViewModel => Provider.GetService<LoginViewModel>();

        public MonitorComponentViewModel MonitorViewModel => Provider.GetService<MonitorComponentViewModel>();

        public ConfigureComponentViewModel ConfigureComponentViewModel => Provider.GetService<ConfigureComponentViewModel>();
    }
}
