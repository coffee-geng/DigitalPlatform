using Coffee.DigitalPlatform.Entities;
using System.Data;

namespace Coffee.DigitalPlatform.IDataAccess
{
    public interface ILocalDataAccess
    {
        #region 登录注册
        UserEntity Login(string username, string password);

        void ResetPassword(string username);
        #endregion

        #region 设备信息
        IList<ComponentEntity> GetComponentsForCreate();

        void SaveDevices(IList<DeviceEntity> devices);

        IList<DeviceEntity> ReadDevices();
        #endregion

        #region 通信参数
        CommunicationParameterDefinitionEntity GetProtocolParamDefinition();

        IList<CommunicationParameterDefinitionEntity> GetCommunicationParamDefinitions(string protocol);

        IList<CommunicationParameterDefinitionEntity> GetCommunicationParamDefinitions();

        IList<CommunicationParameterEntity> GetCommunicationParametersByDevice(string deviceNum);

        IList<CommunicationParameterOptionEntity> GetCommunicationParameterOptions(CommunicationParameterDefinitionEntity commParam);
        #endregion

        #region 变量点位信息
        IList<VariableEntity> GetVariablesByDevice(string deviceNum);
        #endregion

        #region 预警信息
        void AddAlarmInfoToDevice(DeviceEntity device, AlarmEntity alarmInfo, ConditionEntity condition);

        void DeleteAlarmInfoFromDevice(DeviceEntity device, string alarmNum);

        IList<AlarmEntity> GetAlarmsForDevice(DeviceEntity device);

        AlarmEntity GetAlarmByNum(string alarmNum);
        #endregion
    }
}
