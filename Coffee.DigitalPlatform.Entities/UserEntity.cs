using System.ComponentModel.DataAnnotations.Schema;

namespace Coffee.DigitalPlatform.Entities
{
    public class UserEntity
    {
        [Column("user_name")]
        public string UserName { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("user_type")]
        public string UserType { get; set; }

        [Column("real_name")]
        public string RealName { get; set; }

        [Column("gender")]
        public string Gender { get; set; }

        [Column("phone_num")]
        public string PhoneNum { get; set; }

        [Column("department")]
        public string Department { get; set; }
    }
}
