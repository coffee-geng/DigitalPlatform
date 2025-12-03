using Coffee.DigitalPlatform.CommWPF;
using Coffee.DigitalPlatform.IDataAccess;
using Coffee.DigitalPlatform.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Coffee.DigitalPlatform.ViewModels
{
    public class LoginViewModel : ObservableObject
    {
        public User User { get; set; }

        public RelayCommand<object> LoginCommand { get; set; }

        public string _failedMsg;
        public string FailedMsg
        {
            get { return _failedMsg; }
            set { SetProperty<string>(ref _failedMsg, value); }
        }

        ILocalDataAccess _localDataAccess;
        public LoginViewModel(ILocalDataAccess localDataAccess)
        {
            _localDataAccess = localDataAccess;
            if (!DesignTimeHelper.IsInDesignMode)
            {
                User = new User();
                LoginCommand = new RelayCommand<object>(DoLogin);
            }
        }

        private void DoLogin(object obj)
        {
            // 对接数据库
            try
            {
                var data = _localDataAccess.Login(User.UserName, User.Password);
                if (data == null) throw new Exception("登录失败，用户名或密码错误！");

                // 记录一下主窗口所需要的用户信息
                var main = ViewModelLocator.Instance.MainViewModel;
                if (main != null)
                {
                    main.GlobalUserInfo.UserName = User.UserName;
                    main.GlobalUserInfo.Password = User.Password;
                    main.GlobalUserInfo.RealName = User.RealName;
                    main.GlobalUserInfo.UserType = User.UserType;
                    main.GlobalUserInfo.Gender = User.Gender;
                    main.GlobalUserInfo.Department = User.Department;
                    main.GlobalUserInfo.PhoneNumber = User.PhoneNumber;
                }
                (obj as Window).DialogResult = true;
            }
            catch (Exception ex)
            {
                FailedMsg = ex.Message;
            }
        }
    }
}
