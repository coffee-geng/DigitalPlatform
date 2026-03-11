using Coffee.DigitalPlatform.Common;
using Coffee.DigitalPlatform.CommWPF;
using Coffee.DigitalPlatform.Entities;
using Coffee.DigitalPlatform.IDataAccess;
using Coffee.DigitalPlatform.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Xaml.Behaviors.Layout;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Coffee.DigitalPlatform.ViewModels
{
    public class SettingsViewModel : ObservableObject, INavigationService
    {
        private ILocalDataAccess _localDataAccess { get; }
        private MonitorComponentViewModel _monitorViewModel { get; }

        public SettingsViewModel(ILocalDataAccess localDataAccess, MonitorComponentViewModel monitorViewModel)
        {
            _localDataAccess = localDataAccess;
            _monitorViewModel = monitorViewModel;

            _userTypeList = new ReadOnlyCollection<UserType>(initUserTypeList());

            RefreshCommand = new RelayCommand(doRefreshCommand);
            SaveCommand = new RelayCommand<FrameworkElement>(doSaveCommand);
            CloseErrorMessageBoxCommand = new RelayCommand<object>(doCloseErrorMessageBox);
            AddUserCommand = new RelayCommand(doAddUserCommand, canAddUserCommand);
            RemoveUserCommand = new RelayCommand<User>(doRemoveUserCommand, canRemoveUserCommand);
            ResetPasswordCommand = new RelayCommand<User>(doResetPasswordCommand, canResetPasswordCommand);

            if (RefreshCommand.CanExecute(null))
            {
                RefreshCommand.Execute(null);
            }
        }

        #region 常规设置
        private string _systemName;
        public string SystemName
        {
            get { return _systemName; }
            set { SetProperty(ref _systemName, value); }
        }

        private string _dataBufferSize;
        public string DataBufferSize
        {
            get { return _dataBufferSize; }
            set { SetProperty(ref _dataBufferSize, value); }
        }

        private string _logBufferSize;
        public string LogBufferSize
        {
            get { return _logBufferSize; }
            set { SetProperty(ref _logBufferSize, value); }
        }

        private string _logPath;
        public string LogPath
        {
            get { return _logPath; }
            set { SetProperty(ref _logPath, value); }
        }
        #endregion

        #region 监测配置
        private IList<SettingInfo> _monitorList;
        public IList<SettingInfo> MonitorList
        {
            get { return _monitorList; }
            set { SetProperty(ref _monitorList, value); }
        }
        
        private IList<Device> _deviceList;
        public IList<Device> DeviceList
        {
            get { return _deviceList; }
            set { SetProperty(ref _deviceList, value); }
        }
        #endregion

        #region 用户管理
        private ObservableCollection<User> _users = new ObservableCollection<User>();
        public ObservableCollection<User> Users
        {
            get { return _users; }
            private set { SetProperty(ref _users, value); }
        }

        private ReadOnlyCollection<UserType> _userTypeList;
        public ReadOnlyCollection<UserType> UserTypeList
        {
            get { return _userTypeList; }
        }

        public RelayCommand AddUserCommand { get; set; }

        public RelayCommand<User> RemoveUserCommand { get; set; }

        public RelayCommand<User> ResetPasswordCommand {  get; set; }

        private void doAddUserCommand()
        {
            Users.Add(new User());

            RemoveUserCommand.NotifyCanExecuteChanged();
            ResetPasswordCommand.NotifyCanExecuteChanged();
        }

        private bool canAddUserCommand()
        {
            return true;
        }

        private void doRemoveUserCommand(User user)
        {
            if (user != null)
            {
                Users.Remove(user);

                RemoveUserCommand.NotifyCanExecuteChanged();
                ResetPasswordCommand.NotifyCanExecuteChanged();
            }
        }

        private bool canResetPasswordCommand(User user)
        {
            return user != null;
        }

        private void doResetPasswordCommand(User user)
        {
            if (user != null)
            {
                _localDataAccess.ResetPassword(user.UserName);
            }
        }

        private bool canRemoveUserCommand(User user)
        {
            return user != null;
        }
        #endregion

        public RelayCommand RefreshCommand { get; set; }
        public RelayCommand<FrameworkElement> SaveCommand { get; set; }

        private void doRefreshCommand()
        {
            DeviceList = _monitorViewModel.DeviceList?.Where(d => d.Variables.Any()).ToList();

            IEnumerable<SettingInfoEntity> settingInfoEntities = _localDataAccess.GetSettingInfos();
            IEnumerable<UserEntity> userEntities = _localDataAccess.GetAllUsers();

            ObservableCollection<User> users = new ObservableCollection<User>();
            if (userEntities != null)
            {
                foreach (var u in userEntities)
                {
                    var user = new User
                    {
                        UserName = u.UserName,
                        Password = u.Password,
                        RealName = u.RealName,
                        Gender = int.Parse(u.Gender),
                        PhoneNumber = u.PhoneNum,
                        Department = u.Department
                    };
                    if (Enum.TryParse(typeof(UserTypes), u.UserType, out object? typeResult))
                    {
                        UserTypes userType = (UserTypes)typeResult;
                        user.UserType = UserTypeList?.FirstOrDefault(u => u.TypeId == userType);
                    }
                    users.Add(user);
                }
            }
            Users = users;

            initDashboardInfos(settingInfoEntities);

            RemoveUserCommand.NotifyCanExecuteChanged();
        }

        private void doSaveCommand(FrameworkElement owner)
        {
            VisualStateManager.GoToElementState(owner, "NormalToSuccess", false);
            VisualStateManager.GoToElementState(owner, "NormalToFailure", false);

            IEnumerable<SettingInfoEntity> baseInfos = _localDataAccess.GetSettingInfos()?.Where(p => p.Type == Enum.GetName(typeof(SettingInfoTypes), SettingInfoTypes.SystemInfo));
            if (baseInfos != null)
            {
                var infoSysName = baseInfos.FirstOrDefault(b => b.InfoNum == "B001");
                var infoDataBufferSize = baseInfos.FirstOrDefault(b => b.InfoNum == "B002");
                var infoLogBufferSize = baseInfos.FirstOrDefault(b => b.InfoNum == "B003");
                var infoLogPath = baseInfos.FirstOrDefault(b => b.InfoNum == "B004");

                if (infoSysName != null)
                {
                    infoSysName.Value = SystemName;
                    infoSysName.ValueType = typeof(string).AssemblyQualifiedName;
                }
                if (infoDataBufferSize != null)
                {
                    infoDataBufferSize.Value = DataBufferSize;
                    infoDataBufferSize.ValueType = typeof(double).AssemblyQualifiedName;
                }
                if (infoLogBufferSize != null)
                {
                    infoLogBufferSize.Value = LogBufferSize;
                    infoLogBufferSize.ValueType = typeof(double).AssemblyQualifiedName;
                }
                if (infoLogPath != null)
                {
                    infoLogPath.Value = LogPath;
                    infoLogPath.ValueType = typeof(string).AssemblyQualifiedName;
                }
            }

            var newSettingInfos = new List<SettingInfoEntity>();
            if (baseInfos != null && baseInfos.Any())
            {
                newSettingInfos.AddRange(baseInfos);
            }
            if (MonitorList != null && MonitorList.Any())
            {
                foreach(var p in MonitorList)
                {
                    var settingEntity = new SettingInfoEntity
                    {
                        InfoNum = p.InfoNum,
                        Title = p.Title,
                        Description = p.Description,
                        DeviceNum = p.DeviceNum,
                        VariableNum = p.VariableNum,
                        Type = Enum.GetName(typeof(SettingInfoTypes), SettingInfoTypes.MonitorInfo)
                    };
                    var @var = p.VariableList?.Where(v => v.DeviceNum == p.DeviceNum && v.VarNum == p.VariableNum).FirstOrDefault();
                    if (var != null)
                    {
                        settingEntity.ValueType = var.VarType?.AssemblyQualifiedName;
                        //不需要保存Value，因为这个值是实时监测的
                        //if (!string.IsNullOrEmpty(settingEntity.ValueType) && var.Value != null)
                        //{
                        //    settingEntity.Value = ObjectToStringConverter.ConvertToString(var.Value);
                        //}
                        //else
                        //{
                        //    settingEntity.Value = null;
                        //}
                    }
                    newSettingInfos.Add(settingEntity);
                }
            }

            try
            {
                _localDataAccess.SaveSettingInfos(newSettingInfos);

                VisualStateManager.GoToElementState(owner, "ShowSuccess", true);
            }
            catch(Exception ex)
            {
                FailureMessageOnSaving = ex.Message;
                VisualStateManager.GoToElementState(owner, "ShowFailure", true);
            }
        }

        private void initDashboardInfos(IEnumerable<SettingInfoEntity> baseInfos)
        {
            SystemName = baseInfos.FirstOrDefault(b => b.InfoNum == "B001").Value;
            DataBufferSize = baseInfos.FirstOrDefault(b => b.InfoNum == "B002").Value;
            LogBufferSize = baseInfos.FirstOrDefault(b => b.InfoNum == "B003").Value;
            LogPath = baseInfos.FirstOrDefault(b => b.InfoNum == "B004").Value;

            var monitorList = new List<SettingInfo>();
            foreach (var item in baseInfos.Where(b => b.Type == Enum.GetName(typeof(SettingInfoTypes), SettingInfoTypes.MonitorInfo)))
            {
                monitorList.Add(new SettingInfo
                {
                    InfoNum = item.InfoNum,
                    Title = item.Title,
                    Description = item.Description,
                    DeviceList = DeviceList,
                    DeviceNum = item.DeviceNum,
                    VariableNum = item.VariableNum
                });
            }
            MonitorList = monitorList;
        }

        private IList<UserType> initUserTypeList()
        {
            var userTypes = new List<UserType>();
            var userTypeValues = Enum.GetValues<UserTypes>();
            foreach (var userTypeValue in userTypeValues)
            {
                string displayName = EnumExtensions.GetDisplayName(userTypeValue);
                var userType = new UserType(userTypeValue, displayName);
                userTypes.Add(userType);
            }
            return userTypes;
        }

        #region 提示消息
        private string _failureMessageOnSaving;

        public string FailureMessageOnSaving
        {
            get { return _failureMessageOnSaving; }
            set { SetProperty(ref _failureMessageOnSaving, value); }
        }

        public RelayCommand<object> CloseErrorMessageBoxCommand { get; set; }

        private void doCloseErrorMessageBox(object owner)
        {
            VisualStateManager.GoToElementState(owner as FrameworkElement, "HideFailure", true);
        }

        public void OnNavigateTo(NavigationContext context = null)
        {
            var owner = context.Parameters["TargetView"];
            if (owner == null || owner is not FrameworkElement)
                return;
            VisualStateManager.GoToElementState(owner as FrameworkElement, "NormalToSuccess", false);
            VisualStateManager.GoToElementState(owner as FrameworkElement, "NormalToFailure", false);
        }

        public void OnNavigateFrom(NavigationContext context = null)
        {
        }
        #endregion
    }
}
