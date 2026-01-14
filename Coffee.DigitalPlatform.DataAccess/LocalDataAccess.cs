using Coffee.DigitalPlatform.Common;
using Coffee.DigitalPlatform.Entities;
using Coffee.DigitalPlatform.Entities.Converter;
using Coffee.DigitalPlatform.IDataAccess;
using Dapper;
using Microsoft.Data.Sqlite;
using SQLitePCL;
using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using static Dapper.SqlMapper;

namespace Coffee.DigitalPlatform.DataAccess
{
    public class LocalDataAccess : ILocalDataAccess
    {
        public LocalDataAccess()
        {
            Batteries.Init();
        }

        // Sqlite数据库的连接字符串
        string connStr = "Data Source=./data.db;";

        // ORM中的处理
        private IEnumerable<T> SqlQuery<T>(string sql, Dictionary<string, object> paramDict = null)
        {
            try
            {
                using (IDbConnection db = new SqliteConnection(connStr))
                {
                    db.Open();

                    if (paramDict != null)
                    {
                        dynamic dynamicObj = new ExpandoObject();
                        var expandoDict = dynamicObj as IDictionary<string, object>;
                        foreach (var kvp in paramDict)
                        {
                            expandoDict[kvp.Key] = kvp.Value;
                        }
                        return db.Query<T>(sql, expandoDict);
                    }
                    else
                    {
                        return db.Query<T>(sql, null);
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private T? SqlQueryFirst<T>(string sql, Dictionary<string, object> paramDict = null)
        {
            try
            {
                using (IDbConnection db = new SqliteConnection(connStr))
                {
                    db.Open();

                    if (paramDict != null)
                    {
                        dynamic dynamicObj = new ExpandoObject();
                        var expandoDict = dynamicObj as IDictionary<string, object>;
                        foreach (var kvp in paramDict)
                        {
                            expandoDict[kvp.Key] = kvp.Value;
                        }
                        return db.QueryFirst(sql, expandoDict);
                    }
                    else
                    {
                        return db.QueryFirst(sql, null);
                    }
                }
            }
            catch (Exception ex)
            {
                return default(T);
            }
        }

        private bool SqlExist(string sql, Dictionary<string, object> paramDict = null)
        {
            try
            {
                using (IDbConnection db = new SqliteConnection(connStr))
                {
                    db.Open();

                    if (paramDict != null)
                    {
                        dynamic dynamicObj = new ExpandoObject();
                        var expandoDict = dynamicObj as IDictionary<string, object>;
                        foreach (var kvp in paramDict)
                        {
                            expandoDict[kvp.Key] = kvp.Value;
                        }
                        return db.QueryFirst(sql, expandoDict) != null;
                    }
                    else
                    {
                        return db.QueryFirst(sql, null) != null;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private int SqlExecute(string sql, Dictionary<string, object> paramDict = null)
        {
            using (IDbConnection db = new SqliteConnection(connStr))
            {
                db.Open();

                if (paramDict != null)
                {
                    dynamic dynamicObj = new ExpandoObject();
                    var expandoDict = dynamicObj as IDictionary<string, object>;
                    foreach (var kvp in paramDict)
                    {
                        expandoDict[kvp.Key] = kvp.Value;
                    }
                    return db.Execute(sql, expandoDict);
                }
                else
                {
                    return db.Execute(sql, null);
                }
            }
        }

        private int SqlExecute(IList<SqlCommand> sqlCommands)
        {
            if (sqlCommands == null || sqlCommands.Count == 0)
                return 0;
            string curSql = string.Empty; //当前正在执行的SQL语句
            using (IDbConnection db = new SqliteConnection(connStr))
            {
                db.Open();

                using (IDbTransaction transaction = db.BeginTransaction())
                {
                    try
                    {
                        int rows = 0;
                        foreach (SqlCommand cmd in sqlCommands)
                        {
                            if (cmd == null)
                            {
                                throw new Exception("Sql语句不完整！");
                            }
                            curSql = cmd.Sql;
                            if (cmd.Parameters != null && cmd.Parameters.Count > 0)
                            {
                                Dictionary<string, object> paramDict = new Dictionary<string, object>();
                                foreach (var pair in cmd.Parameters)
                                {
                                    paramDict.Add(pair.Key, pair.Value);
                                }
                                dynamic dynamicObj = new ExpandoObject();
                                var expandoDict = dynamicObj as IDictionary<string, object>;
                                foreach (var kvp in paramDict)
                                {
                                    expandoDict[kvp.Key] = kvp.Value;
                                }
                                rows = db.Execute(cmd.Sql, expandoDict);
                            }
                            else
                            {
                                rows = db.Execute(cmd.Sql, null);
                            }
                        }
                        transaction.Commit();
                        return rows;
                    }
                    catch(Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception($"Sql语句执行失败：{curSql}", ex); 
                    }
                }
            }
        }

        #region 登录逻辑
        public UserEntity Login(string username, string password)
        {
            // 不能拼接 ，Sql注入攻击
            string userSql = "select * from sys_users where user_name=@user_name and password=@password";

            Dictionary<string, object> paramDict = new Dictionary<string, object>();
            paramDict.Add("@user_name", username);
            paramDict.Add("@password", password);

            var result = this.SqlQuery<UserEntity>(userSql, paramDict);
            if (result == null || !result.Any())
                throw new Exception("用户名或密码错误");

            return result.First();
        }
        public void ResetPassword(string username)
        {
            string sql = $"update sys_users set password='123456' where user_name=@username";
            Dictionary<string, object> paramDict = new Dictionary<string, object>();
            paramDict.Add("@username", username);

            this.SqlExecute(sql, paramDict);
        }
        #endregion

        #region 设备信息

        public IList<ComponentEntity> GetComponentsForCreate()
        {
            SqlMapper.SetTypeMap(typeof(ComponentEntity), new ColumnAttributeTypeMapper<ComponentEntity>());

            string sql = "select * from thumbs";
            var results = SqlQuery<ComponentEntity>(sql);
            if (results != null)
                return results.ToList();
            else
                return Enumerable.Empty<ComponentEntity>().ToList();
        }

        public void SaveDevices(IList<DeviceEntity> devices)
        {
            if (devices == null)
                throw new ArgumentNullException($"没有设备需要保存");

            var sourceDevices = ReadDevices();
            var deviceUpdateStateDict = checkDevicesForUpdating(sourceDevices, devices);

            SqlMapper.SetTypeMap(typeof(DeviceEntity), new ColumnAttributeTypeMapper<DeviceEntity>());
            SqlMapper.SetTypeMap(typeof(CommunicationParameterEntity), new ColumnAttributeTypeMapper<CommunicationParameterEntity>());
            SqlMapper.SetTypeMap(typeof(VariableEntity), new ColumnAttributeTypeMapper<VariableEntity>());
            foreach (var pair in deviceUpdateStateDict)
            {
                var device = pair.Key;
                if (string.IsNullOrEmpty(device.DeviceNum))
                    throw new ArgumentNullException($"设备编号不能为空");
                if (pair.Value == ItemUpdateStates.Unchanged)
                    continue;
                else if (pair.Value == ItemUpdateStates.Deleted)
                {
                    IList<SqlCommand> sqlCommands = new List<SqlCommand>();
                    //删除设备关联通信参数信息
                    string sql = @"DELETE FROM device_properties WHERE device_num=@DeviceNum";
                    sqlCommands.Add(new SqlCommand(sql, new Dictionary<string, object>
                    {
                        {"@DeviceNum", device.DeviceNum }
                    }));
                    //删除设备关联变量点位信息
                    sql = @"DELETE FROM variables WHERE device_num=@DeviceNum";
                    sqlCommands.Add(new SqlCommand(sql, new Dictionary<string, object>
                    {
                        {"@DeviceNum", device.DeviceNum }
                    }));
                    //删除设备信息
                    sql = @"DELETE FROM devices WHERE d_num=@DeviceNum";
                    sqlCommands.Add(new SqlCommand(sql, new Dictionary<string, object>
                    {
                        {"@DeviceNum", device.DeviceNum }
                    }));
                    SqlExecute(sqlCommands);
                }
                else
                {
                    IList<SqlCommand> sqlCommands = new List<SqlCommand>();
                    //插入或更新设备信息
                    string sql = @"INSERT INTO devices (d_num, x, y, z, w, h, d_type_name, header, flow_direction, rotate) 
                                    VALUES (@DeviceNum, @X, @Y, @Z, @Width, @Height, @DeviceTypeName, @Label, @FlowDirection, @Rotate) 
                                    ON CONFLICT(d_num) DO UPDATE 
                                    SET x=@X, y=@Y, z=@Z, w=@Width, h=@Height, d_type_name=@DeviceTypeName, header=@Label, flow_direction=@FlowDirection, rotate=@Rotate";
                    sqlCommands.Add(new SqlCommand(sql, new Dictionary<string, object>()
                    {
                        {"@DeviceNum", device.DeviceNum },
                        {"@X", device.X },
                        {"@Y", device.Y },
                        {"@Z", device.Z },
                        {"@Width", device.Width },
                        {"@Height", device.Height },
                        {"@DeviceTypeName", device.DeviceTypeName },
                        {"@Label", device.Label },
                        {"@FlowDirection", device.FlowDirection },
                        {"@Rotate", device.Rotate }
                    }, new Dictionary<string, Type>()
                    {
                        {"@DeviceNum", typeof(string) },
                        {"@X", typeof(string) },
                        {"@Y", typeof(string) },
                        {"@Z", typeof(string) },
                        {"@Width", typeof(string) },
                        {"@Height", typeof(string) },
                        {"@DeviceTypeName", typeof(string) },
                        {"@Label", typeof(string) },
                        {"@FlowDirection", typeof(string) },
                        {"@Rotate", typeof(string) }
                    }));

                    if (pair.Value == ItemUpdateStates.Modified)
                    {
                        //如果是更新设备信息，则先删除设备现有的通信参数
                        sql = @"DELETE FROM device_properties WHERE device_num=@DeviceNum";
                        sqlCommands.Add(new SqlCommand(sql, new Dictionary<string, object>
                        {
                            {"@DeviceNum", device.DeviceNum }
                        }));
                        //删除设备现有的变量点位信息
                        sql = @"DELETE FROM variables WHERE device_num=@DeviceNum";
                        sqlCommands.Add(new SqlCommand(sql, new Dictionary<string, object>
                        {
                            {"@DeviceNum", device.DeviceNum }
                        }));
                    }
                    if (device.CommunicationParameters != null && device.CommunicationParameters.Any())
                    {
                        foreach (var commParam in device.CommunicationParameters)
                        {
                            //插入或更新设备的通信参数
                            sql = @"INSERT INTO device_properties (device_num, prop_name, prop_value, prop_type) 
                                VALUES (@DeviceNum, @PropName, @PropValue, @PropValueType) 
                                ON CONFLICT(device_num, prop_name) DO UPDATE 
                                SET prop_value=@PropValue, prop_type=@PropValueType";
                            sqlCommands.Add(new SqlCommand(sql, new Dictionary<string, object>()
                            {
                                {"@DeviceNum", device.DeviceNum },
                                {"@PropName",  commParam.PropName},
                                {"@PropValue", commParam.PropValue },
                                {"@PropValueType", commParam.PropValueType }
                            }, new Dictionary<string, Type>()
                            {
                                {"@DeviceNum", typeof(string) },
                                {"@PropName", typeof(string) },
                                {"@PropValue", typeof(string) },
                                {"@PropValueType", typeof(string) }
                            }));
                        }
                    }
                    if (device.Variables != null && device.Variables.Any())
                    {
                        foreach (var variable in device.Variables)
                        {
                            //插入变量点位信息
                            sql = @"INSERT INTO variables (device_num, var_num, var_name, var_address, var_type, offset, modulus) 
                                VALUES (@DeviceNum, @VarNum, @VarName, @VarAddress, @VarType, @Offset, @Modulus)";
                            sqlCommands.Add(new SqlCommand(sql, new Dictionary<string, object>()
                            {
                                {"@DeviceNum", device.DeviceNum },
                                {"@VarNum",  variable.VarNum},
                                {"@VarName",  variable.Label},
                                {"@VarAddress", variable.Address },
                                {"@VarType", variable.VarType },
                                {"@Offset", variable.Offset },
                                {"@Modulus", variable.Factor }
                            }, new Dictionary<string, Type>()
                            {
                                {"@DeviceNum", typeof(string) },
                                {"@VarNum",  typeof(string) },
                                {"@VarName", typeof(string) },
                                {"@VarAddress", typeof(string) },
                                {"@VarType", typeof(string) },
                                {"@Offset", typeof(double) },
                                {"@Modulus", typeof(double) }
                            }));
                        }
                    }

                    //执行批量SQL语句
                    SqlExecute(sqlCommands);
                }
            }
        }

        // 比较数据库和界面中的设备数据，找出需要更新的设备及属性
        private Dictionary<DeviceEntity, ItemUpdateStates> checkDevicesForUpdating(IList<DeviceEntity> sourceDevices, IList<DeviceEntity> targetDevices)
        {
            if (sourceDevices == null || !sourceDevices.Any())
                return targetDevices != null ? targetDevices.ToDictionary(d => d, d => ItemUpdateStates.Added) : new Dictionary<DeviceEntity, ItemUpdateStates>();
            if (targetDevices == null || !targetDevices.Any())
            {
                if (sourceDevices != null && sourceDevices.Any())
                    return sourceDevices.ToDictionary(d => d, d => ItemUpdateStates.Deleted);
                else
                    return new Dictionary<DeviceEntity, ItemUpdateStates>();
            }

            Dictionary<DeviceEntity, ItemUpdateStates> itemUpdateStateDict = new Dictionary<DeviceEntity, ItemUpdateStates>();
            var devicesForRemove = sourceDevices.Except(targetDevices, new DeviceByNumComparer()).ToList();
            devicesForRemove.ForEach(d => itemUpdateStateDict.Add(d, ItemUpdateStates.Deleted));
            var devicesForAdd = targetDevices.Except(sourceDevices, new DeviceByNumComparer()).ToList();
            devicesForAdd.ForEach(d => itemUpdateStateDict.Add(d, ItemUpdateStates.Added));

            var otherDevices = sourceDevices.Intersect(targetDevices, new DeviceByNumComparer()).ToList(); //包括属性变更或未变更的设备
            
            foreach (var sourceDevice in otherDevices)
            {
                var targetDevice = targetDevices.FirstOrDefault(d => string.Equals(d.DeviceNum, sourceDevice.DeviceNum, StringComparison.OrdinalIgnoreCase));
                if (targetDevice == null)
                    continue;
                if (!targetDevice.Equals(sourceDevice))
                {
                    itemUpdateStateDict.Add(targetDevice, ItemUpdateStates.Modified);
                }
                else
                {
                    itemUpdateStateDict.Add(targetDevice, ItemUpdateStates.Unchanged);
                }
            }
            return itemUpdateStateDict;
        }

        public IList<DeviceEntity> ReadDevices()
        {
            SqlMapper.SetTypeMap(typeof(DeviceEntity), new ColumnAttributeTypeMapper<DeviceEntity>());
            string sql = "select * from devices";
            var devices = SqlQuery<DeviceEntity>(sql);
            if (devices != null && devices.Any())
            {
                foreach(var device in devices)
                {
                    //获取设备的通信参数
                    var commParams = GetCommunicationParametersByDevice(device.DeviceNum);
                    device.CommunicationParameters = commParams != null ? commParams.ToList() : new List<CommunicationParameterEntity>();

                    var variables = GetVariablesByDevice(device.DeviceNum);
                    device.Variables = variables != null ? variables.ToList() : new List<VariableEntity>();
                }
                return devices.ToList();
            }
            else
                return Enumerable.Empty<DeviceEntity>().ToList();
        }

        #endregion

        #region 通信参数
        //得到通信协议参数定义
        public CommunicationParameterDefinitionEntity GetProtocolParamDefinition()
        {
            SqlMapper.SetTypeMap(typeof(CommunicationParameterDefinitionEntity), new ColumnAttributeTypeMapper<CommunicationParameterDefinitionEntity>());
            SqlMapper.AddTypeHandler(typeof(Type), new StringToTypeHandler());

            string sql = "select * from properties where p_name='Protocol'";
            var results = SqlQuery<CommunicationParameterDefinitionEntity>(sql);
            if (results != null && results.Any())
            {
                var entity = results.First();
                Dictionary<string, object> paramDict = new Dictionary<string, object>();
                paramDict.Add("@PropName", entity.ParameterName);
                var protocolNames = SqlQuery<string>("select protocol from properties_protocols where prop_name=@PropName", paramDict);
                entity.ProtocolNames = protocolNames != null ? protocolNames.ToList() : new List<string>();

                //如果参数类型是Selector，根据参数默认值，得到默认值所在集合的索引值
                var protocolOptions = GetCommunicationParameterOptions(entity);
                if (protocolOptions != null && protocolOptions.Any())
                {
                    int defaultIndex = 0;
                    for (int i = 0; i < protocolOptions.Count; i++)
                    {
                        if (string.Equals(protocolOptions[i].PropOptionValue, entity.DefaultValueOption, StringComparison.OrdinalIgnoreCase))
                        {
                            defaultIndex = i;
                            break;
                        }
                    }
                    entity.DefaultOptionIndex = defaultIndex;
                }
                return entity;
            }
            else
                return null;
        }

        //得到某种协议的通信参数定义集合
        public IList<CommunicationParameterDefinitionEntity> GetCommunicationParamDefinitions(string protocol)
        {
            SqlMapper.SetTypeMap(typeof(CommunicationParameterDefinitionEntity), new ColumnAttributeTypeMapper<CommunicationParameterDefinitionEntity>());
            SqlMapper.AddTypeHandler(typeof(Type), new StringToTypeHandler());

            string sql = "SELECT p.* FROM properties p LEFT JOIN properties_protocols pp ON p.p_name = pp.prop_name WHERE pp.protocol=@Protocol";
            Dictionary<string, object> paramDict = new Dictionary<string, object>();
            paramDict.Add("@Protocol", protocol);
            var results = SqlQuery<dynamic>(sql, paramDict);
            if (results != null && results.Any())
            {
                var protocolNames = SqlQuery<dynamic>("select prop_name, GROUP_CONCAT(protocol) AS protocol_names from properties_protocols GROUP BY prop_name", null);
                Dictionary<string, IList<string>> protocolDict = new Dictionary<string, IList<string>>();
                foreach (var p in protocolNames)
                {
                    if (protocolDict.ContainsKey((string)p.prop_name))
                    {
                        var lst1 = protocolDict[((string)p.prop_name)];
                        var lst2 = p["protocol_names"].Split(',').ToList();
                        foreach (var item in lst2)
                        {
                            if (!lst1.Contains(item))
                            {
                                lst1.Add(item);
                            }
                        }
                    }
                    else
                    {
                        var array = new List<string>(p.protocol_names.Split(','));
                        protocolDict.Add((string)p.prop_name, array);
                    }
                }

                var optionDict = GetAllCommunicationParameterOptions();
                return results.Select(p => new CommunicationParameterDefinitionEntity
                {
                    ParameterName = p.p_name,
                    Label = p.p_header,
                    ValueInputType = (int)Enum.Parse(typeof(ValueInputTypes), p.p_input_type.ToString()),
                    ValueDataType = TypeUtils.GetTypeFromAssemblyQualifiedName(p.p_data_type),
                    DefaultValueOption = p.p_default,
                    DefaultOptionIndex = (optionDict.ContainsKey(p.p_name) ?
                                         optionDict[p.p_name].FindIndex((Predicate<CommunicationParameterOptionEntity>)(o => string.Equals(o.PropOptionValue, p.p_default, StringComparison.OrdinalIgnoreCase))) : -1),
                    IsDefaultParameter = (p.is_default != null || Convert.IsDBNull(p.is_default)) ? (bool)Convert.ChangeType(p.is_default, typeof(bool)) : false,
                    ProtocolNames = protocolDict.ContainsKey(p.p_name) ? protocolDict[p.p_name] : new List<string>()
                }).ToList();
            }
            else
            {
                return null;
            }
        }

        //得到所有通信参数定义集合
        public IList<CommunicationParameterDefinitionEntity> GetCommunicationParamDefinitions()
        {
            SqlMapper.SetTypeMap(typeof(CommunicationParameterDefinitionEntity), new ColumnAttributeTypeMapper<CommunicationParameterDefinitionEntity>());
            SqlMapper.AddTypeHandler(typeof(Type), new StringToTypeHandler());

            string sql = "SELECT * FROM properties'";
            var results = SqlQuery<CommunicationParameterDefinitionEntity>(sql);
            if (results != null && results.Any())
                return results.ToList();
            else
                return null;
        }

        //得到某个设备正在使用的通信参数集合
        public IList<CommunicationParameterEntity> GetCommunicationParametersByDevice(string deviceNum)
        {
            SqlMapper.SetTypeMap(typeof(CommunicationParameterEntity), new ColumnAttributeTypeMapper<CommunicationParameterEntity>());
            string sql = "SELECT * FROM device_properties WHERE device_num=@DeviceNum";
            Dictionary<string, object> paramDict = new Dictionary<string, object>();
            paramDict.Add("@DeviceNum", deviceNum);
            var results = SqlQuery<CommunicationParameterEntity>(sql, paramDict);
            if (results != null && results.Any())
                return results.ToList();
            else
                return null;
        }

        public IList<CommunicationParameterOptionEntity> GetCommunicationParameterOptions(CommunicationParameterDefinitionEntity commParam)
        {
            if (commParam == null)
                throw new ArgumentNullException(nameof(commParam));
            SqlMapper.SetTypeMap(typeof(CommunicationParameterOptionEntity), new ColumnAttributeTypeMapper<CommunicationParameterOptionEntity>());
            string sql = "SELECT prop_name, prop_option_value, prop_option_label FROM properties_options WHERE prop_name=@PropName";
            Dictionary<string, object> paramDict = new Dictionary<string, object>();
            paramDict.Add("@PropName", commParam.ParameterName);
            var results = SqlQuery<CommunicationParameterOptionEntity>(sql, paramDict);
            if (results != null && results.Any())
                return results.ToList();
            else
                return null;
        }

        public Dictionary<string, IList<CommunicationParameterOptionEntity>> GetAllCommunicationParameterOptions()
        {
            SqlMapper.SetTypeMap(typeof(CommunicationParameterOptionEntity), new ColumnAttributeTypeMapper<CommunicationParameterOptionEntity>());
            string sql = "SELECT prop_name, prop_option_value, prop_option_label FROM properties_options";
            var results = SqlQuery<CommunicationParameterOptionEntity>(sql);
            if (results != null && results.Any())
                return results.GroupBy(o => o.PropName)
                              .ToDictionary(g => g.Key, g => (IList<CommunicationParameterOptionEntity>)g.ToList());
            else
                return new Dictionary<string, IList<CommunicationParameterOptionEntity>>();
        }
        #endregion

        #region 变量点位信息
        //得到某个设备正在使用的点位信息集合
        public IList<VariableEntity> GetVariablesByDevice(string deviceNum)
        {
            SqlMapper.SetTypeMap(typeof(VariableEntity), new ColumnAttributeTypeMapper<VariableEntity>());
            string sql = "SELECT * FROM variables WHERE device_num=@DeviceNum";
            Dictionary<string, object> paramDict = new Dictionary<string, object>();
            paramDict.Add("@DeviceNum", deviceNum);
            var results = SqlQuery<VariableEntity>(sql, paramDict);
            if (results != null && results.Any())
                return results.ToList();
            else
                return null;
        }
        #endregion

        #region 预警信息
        public void AddAlarmInfoToDevice(DeviceEntity device, AlarmEntity alarmInfo, ConditionEntity condition)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
            SqlMapper.SetTypeMap(typeof(ConditionEntity), new ColumnAttributeTypeMapper<ConditionEntity>());
            SqlMapper.SetTypeMap(typeof(AlarmEntity), new ColumnAttributeTypeMapper<AlarmEntity>());
            
            AddCondition(condition);

            SqlCommand cmd = new SqlCommand(@"INSERT INTO alarms(a_num, c_num, d_num, content, alarm_time, level, state, user_id, solve_time) 
                                                VALUES (@AlarmNum, @CNum, @DeviceNum, @Content, @AlarmTime, @Level, @State, @UserId, @SolveTime);", new Dictionary<string, object>()
                                                {
                                                    { "@AlarmNum", alarmInfo.AlarmNum },
                                                    { "@CNum", alarmInfo.ConditionNum },
                                                    { "@DeviceNum", alarmInfo.DeviceNum },
                                                    { "@Content", alarmInfo.AlarmContent },
                                                    { "@AlarmTime", alarmInfo.AlarmTime },
                                                    { "@Level", alarmInfo.AlarmLevel },
                                                    { "@State", alarmInfo.State },
                                                    { "@UserId", alarmInfo.UserId },
                                                    { "@SolveTime", alarmInfo.SolveTime }
                                                });
            IList<SqlCommand> sqlCommands = new List<SqlCommand>();
            sqlCommands.Add(cmd);
            SqlExecute(sqlCommands);
        }

        public void DeleteAlarmInfoFromDevice(DeviceEntity device, string alarmNum)
        {
            if (device == null)
                return;
            if (string.IsNullOrEmpty(alarmNum))
                return;

            IList<SqlCommand> sqlCommands = new List<SqlCommand>();
            var cmd = new SqlCommand("DELETE FROM alarms WHERE a_num = @AlarmNum", new Dictionary<string, object>()
                {
                    { "@AlarmNum", alarmNum }
                });
            sqlCommands.Add(cmd);
            SqlExecute(sqlCommands);
        }

        public IList<AlarmEntity> GetAlarmsForDevice(DeviceEntity device)
        {
            if (device == null)
                return null;
            return SqlQuery<AlarmEntity>("SELECT * FROM alarms WHERE d_num = @DeviceNum", new Dictionary<string, object>()
            {
                { "@DeviceNum", device.DeviceNum }
            }).ToList();
        }

        public AlarmEntity GetAlarmByNum(string alarmNum)
        {
            if (string.IsNullOrWhiteSpace(alarmNum))
                return null;
            return SqlQueryFirst<AlarmEntity>("SELECT * FROM alarms WHERE a_num = @AlarmNum", new Dictionary<string, object>()
            {
                { "@AlarmNum", alarmNum }
            });
        }

        private void AddCondition(ConditionEntity condition)
        {
            if (condition == null)
                return;
            if (SqlExist(@"SELECT 1 FROM alarms WHERE c_num = @CNum", new Dictionary<string, object>()
            {
                { "@CNum", condition.CNum }
            }))
            {
                throw new Exception($"当前筛选条件'{condition.CNum}'正在使用中...");
            }

            //全删全插
            var childConditions = getChildConditions(condition);
            IList<SqlCommand> sqlCommands = new List<SqlCommand>();
            foreach (var childCondition in childConditions)
            {
                var cmd = new SqlCommand("DELETE FROM conditions WHERE c_num = @CNum", new Dictionary<string, object>()
                {
                    { "@CNum", childCondition.CNum }
                });
                sqlCommands.Add(cmd);
            }
            SqlCommand cmd2 = new SqlCommand(@"INSERT INTO conditions(c_num, c_type, parent_id, v_num, operator, value) 
                                                VALUES (@CNum, @CType, @ParentId, @VarNum, @Operator, @Value);", new Dictionary<string, object>()
                                                {
                                                    { "@CNum", condition.CNum },
                                                    { "@CType", (int)condition.ConditionNodeTypes },
                                                    { "@ParentId", condition.ParentId },
                                                    { "@VarNum", condition.VarNum },
                                                    { "@Operator", condition.Operator },
                                                    { "@Value", condition.Value },
                                                });
            sqlCommands.Add(cmd2);
            SqlExecute(sqlCommands);
        }

        private void AddConditions(IEnumerable<ConditionEntity> conditions)
        {
            if (conditions == null || !conditions.Any())
                return;
            foreach (var condition in conditions)
            {
                if (SqlExist(@"SELECT 1 FROM alarms WHERE c_num = @CNum", new Dictionary<string, object>()
                {
                    { "@CNum", condition.CNum }
                }))
                {
                    throw new Exception($"当前筛选条件'{condition.CNum}'正在使用中...");
                }
            }

            //全删全插
            List<ConditionEntity> allConditionsForDelete = new List<ConditionEntity>();
            foreach (var condition in conditions)
            {
                allConditionsForDelete.AddRange(getChildConditions(condition));
            }
            allConditionsForDelete = allConditionsForDelete.DistinctBy(c => c.CNum).OrderByDescending(c => c.Level).ToList();

            IList<SqlCommand> sqlCommands = new List<SqlCommand>();
            foreach (var conditionItem in allConditionsForDelete)
            {
                var cmd = new SqlCommand("DELETE FROM conditions WHERE c_num = @CNum", new Dictionary<string, object>()
                {
                    { "@CNum", conditionItem.CNum }
                });
                sqlCommands.Add(cmd);
            }

            foreach (var condition in conditions)
            {
                SqlCommand cmd2 = new SqlCommand(@"INSERT INTO conditions(c_num, c_type, parent_id, v_num, operator, value) 
                                                VALUES (@CNum, @CType, @ParentId, @VarNum, @Operator, @Value);", new Dictionary<string, object>()
                                                {
                                                    { "@CNum", condition.CNum },
                                                    { "@CType", (int)condition.ConditionNodeTypes },
                                                    { "@ParentId", condition.ParentId },
                                                    { "@VarNum", condition.VarNum },
                                                    { "@Operator", condition.Operator },
                                                    { "@Value", condition.Value },
                                                });
                sqlCommands.Add(cmd2);
            }
            
            SqlExecute(sqlCommands);
        }

        private void DeleteCondition(ConditionEntity condition) 
        {
            if (condition == null)
                return;
            //如果指定条件正在使用，则不允许删除
            if (SqlExist(@"SELECT 1 FROM alarms WHERE c_num = @CNum", new Dictionary<string, object>()
            {
                { "@CNum", condition.CNum }
            }))
            {
                throw new Exception($"当前筛选条件'{condition.CNum}'正在使用中...");
            }
            
            var childConditions = getChildConditions(condition);
            //删除指定条件及其组内的所有子条件项
            IList<SqlCommand> delConditionCommands = new List<SqlCommand>();
            foreach(var childCondition in childConditions)
            {
                var cmd = new SqlCommand("DELETE FROM conditions WHERE c_num = @CNum", new Dictionary<string, object>()
                {
                    { "@CNum", childCondition.CNum }
                });
                delConditionCommands.Add(cmd);
            }
            SqlExecute(delConditionCommands);
        }

        private void DeleteConditions(IEnumerable<ConditionEntity> conditions)
        {
            if (conditions == null || !conditions.Any())
                return;
            foreach (var condition in conditions)
            {
                if (SqlExist(@"SELECT 1 FROM alarms WHERE c_num = @CNum", new Dictionary<string, object>()
                {
                    { "@CNum", condition.CNum }
                }))
                {
                    throw new Exception($"当前筛选条件'{condition.CNum}'正在使用中...");
                }
            }

            List<ConditionEntity> allConditionsForDelete = new List<ConditionEntity>();
            foreach (var condition in conditions)
            {
                //删除指定条件及其组内的所有子条件项
                allConditionsForDelete.AddRange(getChildConditions(condition));
            }
            allConditionsForDelete = allConditionsForDelete.DistinctBy(c => c.CNum).OrderByDescending(c => c.Level).ToList();

            IList<SqlCommand> sqlCommands = new List<SqlCommand>();
            foreach (var conditionItem in allConditionsForDelete)
            {
                var cmd = new SqlCommand("DELETE FROM conditions WHERE c_num = @CNum", new Dictionary<string, object>()
                {
                    { "@CNum", conditionItem.CNum }
                });
                sqlCommands.Add(cmd);
            }
            SqlExecute(sqlCommands);
        }

        private IEnumerable<ConditionEntity> getChildConditions(ConditionEntity condition)
        {
            //如果指定条件在某个条件链上是其他条件项的组，当其组内任一条件项正在使用，则不允许删除
            string sql = @"WITH RECURSIVE recursive_query(id, c_num, parent_id, level) AS (
                           SELECT id, c_num, parent_id, 0 AS level FROM conditions WHERE parent_id = IN (SELECT id FROM conditions WHERE v_num=@VNum)
	                       UNION ALL
	                       SELECT conditions.id, conditions.c_num, conditions.parent_id, level + 1 FROM conditions, recursive_query
	                       WHERE conditions.parent_id = recursive_query.id
                          );
                          SELECT * FROM recursive_query ORDER BY level DESC;";
            Dictionary<string, object> paramDict = new Dictionary<string, object>();
            paramDict.Add("@VNum", condition.CNum);
            //返回指定条件组下的所有子条件项，并且按照其在条件链上的树状顺序由子节点到父节点遍历
            var childConditions = SqlQuery<ConditionEntity>(sql, paramDict);
            if (childConditions != null && childConditions.Any())
            {
                string c_nums = string.Join(',', childConditions.Select(c => $"'{c.CNum}'"));
                if (SqlExist($"SELECT 1 FROM alarms WHERE c_num IN ({c_nums})"))
                    throw new Exception("当前筛选条件或其条件组正在使用中...");
            }
            return childConditions;
        }

        #endregion
    }

    internal class DeviceByNumComparer : IEqualityComparer<DeviceEntity>
    {
        public bool Equals(DeviceEntity x, DeviceEntity y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;
            return string.Equals(x.DeviceNum, y.DeviceNum, StringComparison.OrdinalIgnoreCase);
        }
        public int GetHashCode(DeviceEntity obj)
        {
            if (obj == null || string.IsNullOrEmpty(obj.DeviceNum))
                return 0;
            return obj.DeviceNum.GetHashCode();
        }
    }

    public enum ItemUpdateStates
    {
        Unchanged = 0,
        Added = 1,
        Modified = 2,
        Deleted = 3
    }
}
