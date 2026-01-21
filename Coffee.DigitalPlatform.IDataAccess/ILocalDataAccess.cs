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
        void AddOrModifyAlarmInfoToDevice(DeviceEntity device, AlarmEntity alarmInfo, ConditionEntity condition);

        void DeleteAlarmInfoFromDevice(DeviceEntity device, string alarmNum);

        IList<AlarmEntity> GetAlarmsForDevice(DeviceEntity device);

        AlarmEntity GetAlarmByNum(string alarmNum);

        Dictionary<string, IList<AlarmEntity>> ReadAlarms();

        /// <summary>
        /// 保存所有设备的报警信息及触发条件。
        /// </summary>
        /// <param name="deviceAlarmDict">字典保存设备名及其关联的预警信息</param>
        /// <param name="conditionDict">字典保存顶级条件选项编号及其条件和子条件集合</param>
        void SaveAlarms(Dictionary<string, IList<AlarmEntity>> deviceAlarmDict, Dictionary<string, IList<ConditionEntity>> conditionDict);

        //获取所有预警条件，包括一级预警条件及其子条件
        IEnumerable<ConditionEntity> GetConditions();

        //获取所有一级预警条件
        IEnumerable<ConditionEntity> GetTopConditions();

        ConditionEntity? GetConditionByCNum(string c_num);
        #endregion
    }
}
