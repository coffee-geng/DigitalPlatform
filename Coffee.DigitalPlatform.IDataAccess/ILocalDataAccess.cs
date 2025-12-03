using Coffee.DigitalPlatform.Entities;
using System.Data;

namespace Coffee.DigitalPlatform.IDataAccess
{
    public interface ILocalDataAccess
    {
        UserEntity Login(string username, string password);

        void ResetPassword(string username);
    }
}
