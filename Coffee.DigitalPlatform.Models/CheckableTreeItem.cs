using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Models
{
    public class CheckableTreeItem : ObservableObject
    {
        private bool? _checkState = false;
        public bool? CheckState
        {
            get { return _checkState; }
            set { SetProperty(ref _checkState, value); }
        }
    }
}
