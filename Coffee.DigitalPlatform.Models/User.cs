using Coffee.DigitalPlatform.Common;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Coffee.DigitalPlatform.Models
{
    public class User : ObservableObject
    {
        private string _userName = "admin";
        public string UserName
        {
            get { return _userName; }
            set { SetProperty(ref _userName, value); }
        }

        private string _password = "123456";
        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }

        private string _realName;
        public string RealName
        {
            get { return _realName; }
            set { SetProperty(ref _realName, value); }
        }

        private UserType? _userType;
        public UserType? UserType
        {
            get { return _userType; }
            set { SetProperty(ref _userType, value); }
        }

        private string _phoneNumber;
        public string PhoneNumber
        {
            get { return _phoneNumber; }
            set { SetProperty(ref _phoneNumber, value); }
        }

        private string _department;
        public string Department
        {
            get { return _department; }
            set { SetProperty(ref _department, value); }
        }

        private int _gender;
        public int Gender
        {
            get { return _gender; }
            set { SetProperty(ref _gender, value); }
        }
    }
}
