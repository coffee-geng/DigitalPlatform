using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Coffee.DigitalPlatform.Models
{
    public class ComponentGroup
    {
        public string GroupName { get; set; }
        public List<Component> Children { get; set; } = new List<Component>();
    }

    public class Component
    {
        public string Label { get; set; }

        public string Icon { get; set; }

        public string TargetType { get; set; }
        
        public int Width { get; set; }

        public int Height { get; set; }

        public RelayCommand<object> CreateInstanceCommand { get; set; }

        public Component()
        {
            CreateInstanceCommand = new RelayCommand<object>(DoCreateInstance);
        }
        private void DoCreateInstance(object obj)
        {
            DragDrop.DoDragDrop(obj as DependencyObject, this, DragDropEffects.Copy);
        }
    }
}
