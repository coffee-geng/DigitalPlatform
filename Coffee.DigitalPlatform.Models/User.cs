using Coffee.DigitalPlatform.Common;

namespace Coffee.DigitalPlatform.Models
{
    public class User
    {
        public string UserName { get; set; } = "admin";
        public string Password { get; set; } = "123456";

        public string RealName { get; set; }
        public UserTypes UserType { get; set; }
        public string PhoneNumber { get; set; }
        public string Department { get; set; }
        public int Gender { get; set; }
    }
}
