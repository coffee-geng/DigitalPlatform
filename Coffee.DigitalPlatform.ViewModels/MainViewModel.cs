using Coffee.DigitalPlatform.Common;
using Coffee.DigitalPlatform.CommWPF;
using Coffee.DigitalPlatform.IDataAccess;
using Coffee.DigitalPlatform.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Coffee.DigitalPlatform.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        ILocalDataAccess _localDataAccess;

        public MainViewModel(ILocalDataAccess localDataAccess)
        {
            _localDataAccess = localDataAccess;

            if (!DesignTimeHelper.IsInDesignMode)
            {
                // 主窗口数据
                #region 主窗口菜单 
                Menus = new List<Models.Menu>();
                Menus.Add(new Models.Menu
                {
                    CheckState = true,
                    Header = "监控",
                    Icon = "\ue639",
                    TargetView = "MonitorPage"
                });
                Menus.Add(new Models.Menu
                {
                    Header = "趋势",
                    Icon = "\ue61a",
                    TargetView = "TrendPage"
                });
                Menus.Add(new Models.Menu
                {
                    Header = "报警",
                    Icon = "\ue60b",
                    TargetView = "AlarmPage"
                });
                Menus.Add(new Models.Menu
                {
                    Header = "报表",
                    Icon = "\ue703",
                    TargetView = "ReportPage"
                });
                Menus.Add(new Models.Menu
                {
                    Header = "配置",
                    Icon = "\ue60f",
                    TargetView = "SettingsPage"
                });
                #endregion

                SwitchPageCommand = new RelayCommand<object>(obj =>
                {
                    ShowPage(obj);
                });
                SwitchToHomeCommand = new RelayCommand(GoToHome);
            }
        }

        private string _systemTitle;

        public string SystemTtile
        {
            get { return _systemTitle; }
            set { SetProperty<string>(ref _systemTitle, value); }
        }

        private int _viewBlur = 0;

        public int ViewBlur
        {
            get { return _viewBlur; }
            set { SetProperty<int>(ref _viewBlur, value); }
        }

        public User GlobalUserInfo { get; set; } = new User();

        private object _viewContent;

        public object ViewContent
        {
            get { return _viewContent; }
            set { SetProperty(ref _viewContent, value); }
        }

        public List<Models.Menu> Menus { get; set; }

        #region SwitchPageCommand
        public RelayCommand<object> SwitchPageCommand { get; set; }

        public RelayCommand SwitchToHomeCommand { get; set; }

        internal void ShowPage(object obj, Dictionary<string, object> navigateParameters = null)
        {
            Action<NavigationContext> callbackBeforeNavigate = context =>
            {
                var fromVM = context.FromContext;
                if (fromVM is INavigationService navigateService)
                {
                    navigateService.OnNavigateFrom(context);
                }
            };
            Action<NavigationContext> callbackAfterNavigate = context =>
            {
                var toVM = context.ToContext;
                if (toVM is INavigationService navigateService)
                {
                    navigateService.OnNavigateTo(context);
                }
            };
            ShowPage(obj, callbackBeforeNavigate, callbackAfterNavigate, navigateParameters);
        }

        internal void ShowPage(object obj, Action<NavigationContext> callbackBeforeNavigate, Action<NavigationContext> callbackAfterNavigate, Dictionary<string, object> navigateParameters = null)
        {
            var model = obj as Models.Menu;
            if (model != null)
            {
                //页面切换前的视图（视图一定是继承UserControl的对象）
                var oldPage = ViewContent as UserControl;

                if (GlobalUserInfo.UserType == UserTypes.Operator && model.TargetView != "MonitorPage")
                {
                    if (callbackBeforeNavigate != null)
                    {
                        var context = new NavigationContext(null, oldPage?.DataContext as INavigationService, null);
                        if (navigateParameters != null)
                        {
                            foreach(var pair in navigateParameters)
                            {
                                context.Parameters.Add(pair.Key, pair.Value);
                            }
                        }
                        callbackBeforeNavigate.Invoke(context);
                    }
                    // 提示权限
                    this.Menus[0].CheckState = true;
                    // 提示没有权限操作
                    if (ActionManager.ExecuteAndResult<object>("ShowRight", null))
                    {
                        // 执行重新登录
                        DoLogout();
                    }
                }
                else
                {
                    if (ViewContent != null && ViewContent.GetType().Name == model.TargetView) return;

                    Type type = Assembly.Load("Coffee.DigitalPlatform.Views")
                        .GetType("Coffee.DigitalPlatform.Views." + model.TargetView)!;
                    var targetView = Activator.CreateInstance(type)!;

                    INavigationService toContext = null;
                    if (targetView is UserControl)
                    {
                        toContext = (targetView as UserControl).DataContext as INavigationService;
                    }

                    if (callbackBeforeNavigate != null)
                    {
                        var context = new NavigationContext(new Uri(targetView.GetType().Name, UriKind.Relative), oldPage?.DataContext as INavigationService, toContext);
                        if (navigateParameters != null)
                        {
                            foreach (var pair in navigateParameters)
                            {
                                context.Parameters.Add(pair.Key, pair.Value);
                            }
                        }
                        callbackBeforeNavigate.Invoke(context);
                    }

                    ViewContent = targetView;

                    if (callbackAfterNavigate != null)
                    {
                        var context = new NavigationContext(new Uri(targetView.GetType().Name, UriKind.Relative), oldPage?.DataContext as INavigationService, toContext);
                        if (navigateParameters != null)
                        {
                            foreach (var pair in navigateParameters)
                            {
                                context.Parameters.Add(pair.Key, pair.Value);
                            }
                        }
                        callbackAfterNavigate.Invoke(context);
                    }
                }
            }
        }

        private void GoToHome()
        {
            var homePage = Menus.FirstOrDefault();
            if (homePage == null)
                return;
            Action<NavigationContext> callbackBeforeNavigate = context =>
            {
                var fromVM = context.FromContext;
                if (fromVM is INavigationService navigateService)
                {
                    navigateService.OnNavigateFrom(context);
                }
            };
            Action<NavigationContext> callbackAfterNavigate = context =>
            {
                var toVM = context.ToContext;
                if (toVM is INavigationService navigateService)
                {
                    navigateService.OnNavigateTo(context);
                }
            };
            ShowPage(homePage, callbackBeforeNavigate, callbackAfterNavigate);
        }
        #endregion

        #region Logout
        private bool _isWindowClose;

        public bool IsWindowClose
        {
            get { return _isWindowClose; }
            set { SetProperty(ref _isWindowClose, value); }
        }

        private void DoLogout()
        {
            Process.Start("Coffee.DigitalPlatform.exe");

            this.IsWindowClose = true;// 设计上来看  等同于关闭窗口
            // 关闭当前
        }
        #endregion

        /// <summary>
        /// 提示没有权限操作
        /// </summary>
        public void ShowNonPermission()
        {
            if (ActionManager.ExecuteAndResult<object>("ShowRight", null))
            {
                // 执行重新登录
                DoLogout();
            }
        }
    }
}
