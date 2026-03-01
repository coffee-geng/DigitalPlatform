using Coffee.DigitalPlatform.Entities;
using System.Data;

namespace Coffee.DigitalPlatform.IDataAccess
{
    public interface ILocalDataAccess
    {
        #region 登录注册
        UserEntity Login(string username, string password);

        void ResetPassword(string username);

        IEnumerable<UserEntity> GetAllUsers();
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

        #region 条件选项
        //获取所有触发条件，包括一级触发条件及其子条件
        IEnumerable<ConditionEntity> GetConditions();

        //获取所有一级触发条件
        IEnumerable<ConditionEntity> GetTopConditions();

        ConditionEntity? GetConditionByCNum(string c_num);
        #endregion

        #region 预警信息
        void AddOrModifyAlarmInfoToDevice(DeviceEntity device, AlarmEntity alarmInfo, ConditionEntity condition);

        void DeleteAlarmInfoFromDevice(DeviceEntity device, string alarmNum);

        IList<AlarmEntity> GetAlarmsForDevice(DeviceEntity device);

        AlarmEntity GetAlarmByNum(string alarmNum, string deviceNum, bool isHistory = false);

        Dictionary<string, IList<AlarmEntity>> ReadAlarms(bool isHistory=false);

        /// <summary>
        /// 读取最近的预警历史记录。对于每个设备，可以返回多个预警信息，每个预警信息只包含离现在最近的一条预警历史记录。
        /// </summary>
        /// <returns></returns>
        Dictionary<string, IList<AlarmHistoryRecord>> ReadRecentAlarms();

        /// <summary>
        /// 保存所有设备的报警信息及触发条件。
        /// </summary>
        /// <param name="deviceAlarmDict">字典保存设备名及其关联的预警信息</param>
        /// <param name="conditionDict">字典保存顶级条件选项编号及其条件和子条件集合</param>
        void SaveAlarms(Dictionary<string, IList<AlarmEntity>> deviceAlarmDict, Dictionary<string, IList<ConditionEntity>> conditionDict);

        /// <summary>
        /// 当预警状态切换后，保存预警状态。该方法会更新预警信息中的报警状态、预警条件触发的条件项源数据、报警时间、处理时间和操作员等字段。
        /// </summary>
        /// <param name="alarmNum">预警编号，每个设备的预警编号都不一样</param>
        /// <param name="deviceNum">设备编号</param>
        /// <param name="newState">报警状态</param>
        /// <param name="alarmVariables">预警条件触发的阈值数据，复杂的预警条件包含多个条件项源数据</param>
        /// <param name="alarmTime">触发条件时的时间</param>
        /// <param name="solvedTime">报警解决的时间</param>
        /// <param name="userId">操作员ID</param>
        void UpdateAlarmHistory(string alarmNum, string deviceNum, string newState, IList<AlarmVariable>? alarmVariables, DateTime? alarmTime, DateTime? solvedTime, string userId);

        /// <summary>
        /// 批量保存预警状态。
        /// </summary>
        /// <param name="updateAlarmStateRecords">要保存的预警状态集合。集合内每个元素提供了要更新的预警信息，包括报警状态、预警条件触发的条件项源数据、报警时间、处理时间和操作员等字段</param>
        void BatchUpdateAlarmHistory(IEnumerable<AlarmHistoryRecord> updateAlarmStateRecords);
        #endregion

        #region 手动控制信息
        Dictionary<string, IList<ControlInfoByManualEntity>> ReadControlInfosByManual();

        void SaveControlInfosByManual(Dictionary<string, IList<ControlInfoByManualEntity>> deviceControlInfoDict);
        #endregion

        #region 联动控制信息
        Dictionary<string, IList<ControlInfoByTriggerEntity>> ReadControlInfosByTrigger();

        void SaveControlInfosByTrigger(Dictionary<string, IList<ControlInfoByTriggerEntity>> deviceControlInfoDict, Dictionary<string, IList<ConditionEntity>> conditionDict);
        #endregion

        void CleanUpOutdatedConditions(IList<string> aliveConditionNumList, IEnumerable<ConditionEntity>? oldTopConditions);
    }
}
