using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Models
{
    public class Menu : CheckableTreeItem
    {
        public int Key { get; set; }

        // 菜单标题
        public string Header { get; set; }
        // 菜单图标
        public string Icon { get; set; }
        // 菜单导航目标视图
        public string TargetView { get; set; }
    }
}
