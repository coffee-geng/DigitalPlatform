using Coffee.DigitalPlatform.Models;
using Coffee.DigitalPlatform.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Coffee.DigitalPlatform.Views
{
    /// <summary>
    /// SettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsPage : UserControl
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void LogPathRelocate_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "请选择保存文件的目录",
                // 设置初始目录（可选）
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                // 是否允许多选文件夹（可选）
                Multiselect = false,
                // 是否在对话框中显示隐藏项（可选）
                ShowHiddenItems = false
            };

            // 显示对话框
            if (dialog.ShowDialog() == true)
            {
                if (DataContext is SettingsViewModel vm)
                {
                    vm.LogPath = dialog.FolderName;
                }
            }
        }
    }
}
