using Coffee.DigitalPlatform.Common;
using Coffee.DigitalPlatform.Entities;
using Coffee.DigitalPlatform.Entities.Converter;
using Coffee.DigitalPlatform.IDataAccess;
using Dapper;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using SQLitePCL;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
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
                                rows = db.Execute(cmd.Sql, expandoDict, transaction);
                            }
                            else
                            {
                                rows = db.Execute(cmd.Sql, null, transaction);
                            }
                        }
                        transaction.Commit();
                        return rows;
                    }
                    catch (Exception ex)
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
                foreach (var device in devices)
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

        #region 条件选项
        //获取所有预警条件，包括一级预警条件及其子条件
        public IEnumerable<ConditionEntity> GetConditions()
        {
            SqlMapper.SetTypeMap(typeof(ConditionEntity), new ColumnAttributeTypeMapper<ConditionEntity>());
            return SqlQuery<ConditionEntity>("SELECT * FROM conditions");
        }

        //获取所有一级预警条件
        public IEnumerable<ConditionEntity> GetTopConditions()
        {
            SqlMapper.SetTypeMap(typeof(ConditionEntity), new ColumnAttributeTypeMapper<ConditionEntity>());
            return SqlQuery<ConditionEntity>("SELECT * FROM conditions WHERE c_num_parent IS NULL");
        }

        public ConditionEntity? GetConditionByCNum(string c_num)
        {
            if (string.IsNullOrWhiteSpace(c_num))
                return null;
            SqlMapper.SetTypeMap(typeof(ConditionEntity), new ColumnAttributeTypeMapper<ConditionEntity>());
            return SqlQueryFirst<ConditionEntity>("SELECT * FROM conditions WHERE c_num = @CNum", new Dictionary<string, object>()
            {
                { "@CNum", c_num }
            });
        }

        private IEnumerable<ConditionEntity> getChildConditions(ConditionEntity condition)
        {
            //如果指定条件在某个条件链上是其他条件项的组，当其组内任一条件项正在使用，则不允许删除
            string sql = @"WITH RECURSIVE recursive_query(id, c_num, c_num_parent, level) AS (
                           SELECT id, c_num, c_num_parent, 1 AS level FROM conditions WHERE c_num_parent = @CNum
	                       UNION ALL
	                       SELECT conditions.id, conditions.c_num, conditions.c_num_parent, level + 1 FROM conditions, recursive_query
	                       WHERE conditions.c_num_parent = recursive_query.c_num
                          )
                          SELECT * FROM recursive_query ORDER BY level DESC;";
            Dictionary<string, object> paramDict = new Dictionary<string, object>();
            paramDict.Add("@CNum", condition.CNum);
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

        private void AddCondition(ConditionEntity condition)
        {
            var sqlCommands = CreateSqlCommandsForAddingCondition(condition);
            SqlExecute(sqlCommands);
        }

        private void AddConditions(IEnumerable<ConditionEntity> conditions)
        {
            var sqlCommands = CreateSqlCommandsForAddingConditions(conditions);
            SqlExecute(sqlCommands);
        }

        private void DeleteCondition(ConditionEntity condition)
        {
            var delConditionCommands = CreateSqlCommandsForDeleteCondition(condition);
            SqlExecute(delConditionCommands);
        }

        private void DeleteConditions(IEnumerable<ConditionEntity> conditions)
        {
            var sqlCommands = CreateSqlCommandsForDeleteConditions(conditions);
            SqlExecute(sqlCommands);
        }

        private IList<SqlCommand> CreateSqlCommandsForAddingCondition(ConditionEntity condition)
        {
            if (condition == null)
                return Enumerable.Empty<SqlCommand>().ToList();
            IList<SqlCommand> sqlCommands = new List<SqlCommand>();

            //if (SqlExist(@"SELECT 1 FROM alarms WHERE c_num = @CNum", new Dictionary<string, object>()
            //{
            //    { "@CNum", condition.CNum }
            //}))
            //{
            //    //throw new Exception($"当前筛选条件'{condition.CNum}'正在使用中...");
            //    var cmd = new SqlCommand("DELETE FROM alarms WHERE c_num = @CNum", new Dictionary<string, object>()
            //        {
            //            { "@CNum", condition.CNum }
            //        });
            //    sqlCommands.Add(cmd);
            //}

            //全删全插
            //注意：删除条件选项时，其字条件也需要同时删除
            //但是，这里在添加条件选项时，只添加当前条件，不会主动添加其子条件，因为ConditionEntity中不包含子条件信息。要同时添加父条件和子条件，需在参数中指定
            sqlCommands.Add(new SqlCommand("DELETE FROM conditions WHERE c_num = @CNum", new Dictionary<string, object>()
                            {
                                { "@CNum", condition.CNum }
                            }));
            var childConditions = getChildConditions(condition);
            if (childConditions != null)
            {
                foreach (var childCondition in childConditions)
                {
                    var cmd = new SqlCommand("DELETE FROM conditions WHERE c_num = @CNum", new Dictionary<string, object>()
                    {
                        { "@CNum", childCondition.CNum }
                    });
                    sqlCommands.Add(cmd);
                }
            }
            SqlCommand cmd2 = new SqlCommand(@"INSERT INTO conditions(c_num, c_type, c_num_parent, v_num, operator, value) 
                                                VALUES (@CNum, @CType, @CNum_Parent, @VarNum, @Operator, @Value);", new Dictionary<string, object>()
                                                {
                                                    { "@CNum", condition.CNum },
                                                    { "@CType", (int)condition.ConditionNodeTypes },
                                                    { "@ParentId", condition.CNum_Parent },
                                                    { "@VarNum", condition.VarNum },
                                                    { "@Operator", condition.Operator },
                                                    { "@Value", condition.Value },
                                                });
            return sqlCommands;
        }

        private IList<SqlCommand> CreateSqlCommandsForAddingConditions(IEnumerable<ConditionEntity> conditions)
        {
            if (conditions == null || !conditions.Any())
                return Enumerable.Empty<SqlCommand>().ToList();
            IList<SqlCommand> sqlCommands = new List<SqlCommand>();

            //foreach (var condition in conditions)
            //{
            //    if (SqlExist(@"SELECT 1 FROM alarms WHERE c_num = @CNum", new Dictionary<string, object>()
            //    {
            //        { "@CNum", condition.CNum }
            //    }))
            //    {
            //        //throw new Exception($"当前筛选条件'{condition.CNum}'正在使用中...");
            //        var cmd = new SqlCommand("DELETE FROM alarms WHERE c_num = @CNum", new Dictionary<string, object>()
            //        {
            //            { "@CNum", condition.CNum }
            //        });
            //        sqlCommands.Add(cmd);
            //    }
            //}

            //全删全插
            //注意：删除条件选项时，其字条件也需要同时删除
            //但是，这里在添加条件选项时，只添加当前条件，不会主动添加其子条件，因为ConditionEntity中不包含子条件信息。要同时添加父条件和子条件，需在参数中指定
            List<ConditionEntity> allConditionsForDelete = new List<ConditionEntity>();
            foreach (var condition in conditions)
            {
                allConditionsForDelete.Add(condition);
                var childConditions = getChildConditions(condition);
                if (childConditions == null)
                    continue;
                allConditionsForDelete.AddRange(childConditions);
            }
            allConditionsForDelete = allConditionsForDelete.DistinctBy(c => c.CNum).OrderByDescending(c => c.Level).ToList();

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
                SqlCommand cmd2 = new SqlCommand(@"INSERT INTO conditions(c_num, c_type, c_num_parent, v_num, operator, value) 
                                                VALUES (@CNum, @CType, @CNum_Parent, @VarNum, @Operator, @Value);", new Dictionary<string, object>()
                                                {
                                                    { "@CNum", condition.CNum },
                                                    { "@CType", (int)condition.ConditionNodeTypes },
                                                    { "@CNum_Parent", condition.CNum_Parent },
                                                    { "@VarNum", condition.VarNum },
                                                    { "@Operator", condition.Operator },
                                                    { "@Value", condition.Value },
                                                });
                sqlCommands.Add(cmd2);
            }

            return sqlCommands;
        }

        private IList<SqlCommand> CreateSqlCommandsForDeleteCondition(ConditionEntity condition)
        {
            if (condition == null)
                return Enumerable.Empty<SqlCommand>().ToList();
            //如果指定条件正在使用，则不允许删除
            //if (SqlExist(@"SELECT 1 FROM alarms WHERE c_num = @CNum", new Dictionary<string, object>()
            //{
            //    { "@CNum", condition.CNum }
            //}))
            //{
            //    throw new Exception($"当前筛选条件'{condition.CNum}'正在使用中...");
            //}

            List<ConditionEntity> allConditionsForDelete = new List<ConditionEntity>();
            //删除指定条件及其组内的所有子条件项
            allConditionsForDelete.Add(condition);
            allConditionsForDelete.AddRange(getChildConditions(condition));

            IList<SqlCommand> delConditionCommands = new List<SqlCommand>();
            foreach (var childCondition in allConditionsForDelete)
            {
                var cmd = new SqlCommand("DELETE FROM conditions WHERE c_num = @CNum", new Dictionary<string, object>()
                {
                    { "@CNum", childCondition.CNum }
                });
                delConditionCommands.Add(cmd);
            }
            return delConditionCommands;
        }

        private IList<SqlCommand> CreateSqlCommandsForDeleteConditions(IEnumerable<ConditionEntity> conditions)
        {
            if (conditions == null || !conditions.Any())
                return Enumerable.Empty<SqlCommand>().ToList();
            foreach (var condition in conditions)
            {
                //if (SqlExist(@"SELECT 1 FROM alarms WHERE c_num = @CNum", new Dictionary<string, object>()
                //{
                //    { "@CNum", condition.CNum }
                //}))
                //{
                //    throw new Exception($"当前筛选条件'{condition.CNum}'正在使用中...");
                //}
            }

            List<ConditionEntity> allConditionsForDelete = new List<ConditionEntity>();
            foreach (var condition in conditions)
            {
                //删除指定条件及其组内的所有子条件项
                allConditionsForDelete.Add(condition);
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
            return sqlCommands;
        }
        #endregion

        #region 预警信息
        public void AddOrModifyAlarmInfoToDevice(DeviceEntity device, AlarmEntity alarmInfo, ConditionEntity condition)
        {
            var sqlCommands = CreateSqlCommandsForAddOrModifyAlarm(device, alarmInfo, condition);
            SqlExecute(sqlCommands);
        }

        public IList<SqlCommand> CreateSqlCommandsForAddOrModifyAlarm(DeviceEntity device, AlarmEntity alarmInfo, ConditionEntity condition)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
            SqlMapper.SetTypeMap(typeof(ConditionEntity), new ColumnAttributeTypeMapper<ConditionEntity>());
            SqlMapper.SetTypeMap(typeof(AlarmEntity), new ColumnAttributeTypeMapper<AlarmEntity>());

            AddCondition(condition);

            SqlCommand cmd = new SqlCommand(@"INSERT INTO alarms(a_num, c_num, d_num, content, alarm_time, level, state, user_id, solve_time, tag, statechange_history) 
                                                VALUES (@AlarmNum, @CNum, @DeviceNum, @Content, @AlarmTime, @Level, @State, @UserId, @SolveTime, @Tag, 0)
                                                ON CONFLICT(a_num, d_num, statechange_history) DO UPDATE
                                                SET c_num=@CNum, content=@Content, alarm_time=@AlarmTime, level=@Level, state=@State, user_id=@UserId, solve_time=@SolveTime", new Dictionary<string, object>()
                                                {
                                                    { "@AlarmNum", alarmInfo.AlarmNum },
                                                    { "@CNum", alarmInfo.ConditionNum },
                                                    { "@DeviceNum", alarmInfo.DeviceNum },
                                                    { "@Content", alarmInfo.AlarmMessage },
                                                    { "@AlarmTime", alarmInfo.AlarmTime },
                                                    { "@Level", alarmInfo.AlarmLevel },
                                                    { "@State", alarmInfo.State },
                                                    { "@UserId", alarmInfo.UserId },
                                                    { "@SolveTime", alarmInfo.SolvedTime },
                                                    { "@Tag", alarmInfo.AlarmTag   }
                                                });
            IList<SqlCommand> sqlCommands = new List<SqlCommand>();
            sqlCommands.Add(cmd);
            return sqlCommands;
        }

        public void DeleteAlarmInfoFromDevice(DeviceEntity device, string alarmNum)
        {
            var sqlCommands = CreateSqlCommandsForDeleteAlarm(device, alarmNum);
            SqlExecute(sqlCommands);
        }

        public IList<SqlCommand> CreateSqlCommandsForDeleteAlarm(DeviceEntity device, string alarmNum)
        {
            if (device == null)
                return Enumerable.Empty<SqlCommand>().ToList();
            if (string.IsNullOrEmpty(alarmNum))
                return Enumerable.Empty<SqlCommand>().ToList();

            IList<SqlCommand> sqlCommands = new List<SqlCommand>();
            //仅删除statechange_history=0的预警信息，即还没有报警过的预警信息。如果这个预警信息已报警，则说明其不是设计时添加的预警信息，而是运行过程中产生的预警信息历史记录，此时不允许删除，以免影响预警信息历史记录的完整性
            var cmd = new SqlCommand("DELETE FROM alarms WHERE a_num=@AlarmNum AND d_num=@DeviceNum AND statechange_history=0", new Dictionary<string, object>()
                {
                    { "@AlarmNum", alarmNum },
                    {"@DeviceNum", device.DeviceNum }
                });
            sqlCommands.Add(cmd);
            return sqlCommands;
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

        public AlarmEntity GetAlarmByNum(string alarmNum, string deviceNum, bool isHistory=false)
        {
            if (string.IsNullOrWhiteSpace(alarmNum))
                return null;
            string condition1 = isHistory ? "AND statechange_history > 0" : "AND statechange_history = 0";
            return SqlQueryFirst<AlarmEntity>($"SELECT * FROM alarms WHERE a_num=@AlarmNum AND d_num=@DeviceNum {condition1}", new Dictionary<string, object>()
            {
                { "@AlarmNum", alarmNum },
                { "@DeviceNum", deviceNum }
            });
        }

        public Dictionary<string, IList<AlarmEntity>> ReadAlarms(bool isHistory=false)
        {
            SqlMapper.SetTypeMap(typeof(AlarmEntity), new ColumnAttributeTypeMapper<AlarmEntity>());
            SqlMapper.AddTypeHandler(typeof(Type), new StringToTypeHandler());
            SqlMapper.AddTypeHandler<IList<AlarmVariable>>(new JsonTypeHandler<IList<AlarmVariable>>());

            string condition1 = isHistory ? "WHERE statechange_history > 0" : "WHERE statechange_history = 0";

            var alarms = SqlQuery<AlarmEntity>($"SELECT * FROM alarms {condition1}");
            if (alarms != null && alarms.Any())
            {
                return alarms.GroupBy(a => a.DeviceNum)
                             .ToDictionary(g => g.Key, g => (IList<AlarmEntity>)g.ToList());
            }
            else
            {
                return new Dictionary<string, IList<AlarmEntity>>();
            }
        }

        public Dictionary<string, IList<AlarmHistoryRecord>> ReadRecentAlarms()
        {
            SqlMapper.SetTypeMap(typeof(AlarmHistoryRecord), new ColumnAttributeTypeMapper<AlarmHistoryRecord>());
            SqlMapper.AddTypeHandler(typeof(Type), new StringToTypeHandler());
            SqlMapper.AddTypeHandler(typeof(DateTime?), new SqliteDateTimeHandler());
            SqlMapper.AddTypeHandler<IList<AlarmVariable>>(new JsonTypeHandler<IList<AlarmVariable>>());

            var alarms = SqlQuery<AlarmHistoryRecord>(@"WITH ranked_alarms AS (SELECT *, ROW_NUMBER() OVER (PARTITION BY a_num, d_num ORDER BY statechange_history DESC) AS rn FROM alarms WHERE statechange_history > 0) 
                SELECT a_num, d_num, state, alarm_values, alarm_time, solve_time, user_id, statechange_history FROM ranked_alarms WHERE rn = 1 ORDER BY a_num DESC, d_num DESC;");
            if (alarms != null && alarms.Any())
            {
                return alarms.GroupBy(a => a.DeviceNum)
                             .ToDictionary(g => g.Key, g => (IList<AlarmHistoryRecord>)g.ToList());
            }
            else
            {
                return new Dictionary<string, IList<AlarmHistoryRecord>>();
            }
        }

        /// <summary>
        /// 保存设备的预警信息。
        /// </summary>
        /// <param name="deviceAlarmDict">所有的设备及其预警信息</param>
        /// <param name="topConditionDict">所有的一级预警条件，只有一级预警条件才能作为设备预警的触发条件</param>
        public void SaveAlarms(Dictionary<string, IList<AlarmEntity>> deviceAlarmDict, Dictionary<string, IList<ConditionEntity>> conditionDict)
        {
            List<SqlCommand> sqlCommands = new List<SqlCommand>();
            var topConditionDict = new Dictionary<string, ConditionEntity>();
            var conditionList = new List<ConditionEntity>(); //要写入的所有条件实体
            if (conditionDict != null)
            {
                foreach(var pair in conditionDict)
                {
                    if (!topConditionDict.ContainsKey(pair.Key))
                    {
                        topConditionDict.Add(pair.Key, pair.Value?.First(c => string.IsNullOrEmpty(c.CNum_Parent)));

                        conditionList.AddRange(pair.Value);
                    }
                }
            }

            var sqlCommandsForConditions = CreateSqlCommandsForAddingConditions(conditionList.DistinctBy(c => c.CNum));
            sqlCommands.AddRange(sqlCommandsForConditions);

            var alarmUpdateStateDict = checkAlarmsForUpdating(ReadAlarms(), deviceAlarmDict);
            foreach (var pair in alarmUpdateStateDict)
            {
                if (pair.Value == ItemUpdateStates.Added || pair.Value == ItemUpdateStates.Modified)
                {
                    var alarm = pair.Key;
                    if (topConditionDict.TryGetValue(alarm.ConditionNum, out ConditionEntity condition))
                    {
                        var sqlCommandsForAdd = CreateSqlCommandsForAddOrModifyAlarm(new DeviceEntity { DeviceNum = alarm.DeviceNum }, alarm, condition);
                        sqlCommands.AddRange(sqlCommandsForAdd);
                    }
                    else
                    {
                        //如果没有找到对应的一级预警条件，则当前预警信息无效，需将其从数据库中剔除
                        var sqlCommandsForDelete = CreateSqlCommandsForDeleteAlarm(new DeviceEntity { DeviceNum = alarm.DeviceNum }, alarm.AlarmNum);
                        sqlCommands.AddRange(sqlCommandsForDelete);
                    }
                }
                else if (pair.Value == ItemUpdateStates.Deleted)
                {
                    var alarm = pair.Key;
                    var sqlCommandsForDelete = CreateSqlCommandsForDeleteAlarm(new DeviceEntity { DeviceNum = alarm.DeviceNum }, alarm.AlarmNum);
                    sqlCommands.AddRange(sqlCommandsForDelete);
                }
            }

            SqlExecute(sqlCommands);
        }

        private Dictionary<AlarmEntity, ItemUpdateStates> checkAlarmsForUpdating(Dictionary<string, IList<AlarmEntity>> sourceAlarmDict, Dictionary<string, IList<AlarmEntity>> targetAlarmDict)
        {
            if (sourceAlarmDict == null || !sourceAlarmDict.Any())
                return targetAlarmDict != null ? targetAlarmDict.SelectMany(kvp => kvp.Value).ToDictionary(a => a, a => ItemUpdateStates.Added) : new Dictionary<AlarmEntity, ItemUpdateStates>();
            if (targetAlarmDict == null || !targetAlarmDict.Any())
            {
                if (sourceAlarmDict != null && sourceAlarmDict.Any())
                    return sourceAlarmDict.SelectMany(kvp => kvp.Value).ToDictionary(a => a, a => ItemUpdateStates.Deleted);
                else
                    return new Dictionary<AlarmEntity, ItemUpdateStates>();
            }
            Dictionary<AlarmEntity, ItemUpdateStates> itemUpdateStateDict = new Dictionary<AlarmEntity, ItemUpdateStates>();
            foreach (var targetKvp in targetAlarmDict)
            {
                var deviceNum = targetKvp.Key;
                var targetAlarms = targetKvp.Value;
                if (!sourceAlarmDict.ContainsKey(deviceNum))
                {
                    //源集合中不存在该设备，则该设备的所有预警信息均为新增状态
                    foreach (var alarm in targetAlarms)
                    {
                        itemUpdateStateDict.Add(alarm, ItemUpdateStates.Added);
                    }
                }
            }
            //先判断每个设备的更新状态，即它是新添加或被删除的，还是需要进一步比较
            foreach (var sourceKvp in sourceAlarmDict)
            {
                var deviceNum = sourceKvp.Key;
                var sourceAlarms = sourceKvp.Value;
                if (!targetAlarmDict.ContainsKey(deviceNum))
                {
                    //目标集合中不存在该设备，则该设备的所有报警信息均为删除状态
                    foreach (var alarm in sourceAlarms)
                    {
                        itemUpdateStateDict.Add(alarm, ItemUpdateStates.Deleted);
                    }
                }
                else
                {
                    var targetAlarms = targetAlarmDict[deviceNum];
                    var alarmsForRemove = sourceAlarms.Except(targetAlarms, new AlarmByNumComparer()).ToList();
                    alarmsForRemove.ForEach(a => itemUpdateStateDict.Add(a, ItemUpdateStates.Deleted));
                    var alarmsForAdd = targetAlarms.Except(sourceAlarms, new AlarmByNumComparer()).ToList();
                    alarmsForAdd.ForEach(a => itemUpdateStateDict.Add(a, ItemUpdateStates.Added));

                    var otherAlarms = sourceAlarms.Intersect(targetAlarms, new AlarmByNumComparer()).ToList(); //包括属性变更或未变更的报警信息
                    foreach (var sourceAlarm in otherAlarms)
                    {
                        var targetAlarm = targetAlarms.FirstOrDefault(a => string.Equals(a.AlarmNum, sourceAlarm.AlarmNum, StringComparison.OrdinalIgnoreCase) && string.Equals(a.DeviceNum, sourceAlarm.DeviceNum, StringComparison.OrdinalIgnoreCase));
                        if (targetAlarm == null)
                            continue;
                        if (!targetAlarm.Equals(sourceAlarm))
                        {
                            itemUpdateStateDict.Add(targetAlarm, ItemUpdateStates.Modified);
                        }
                        else
                        {
                            itemUpdateStateDict.Add(targetAlarm, ItemUpdateStates.Unchanged);
                        }
                    }
                }
            }
            return itemUpdateStateDict;
        }

        public void UpdateAlarmHistory(string alarmNum, string deviceNum, string newState, IList<AlarmVariable>? alarmVariables, DateTime? alarmTime, DateTime? solvedTime, string userId)
        {
            List<SqlCommand> sqlCommands = new List<SqlCommand>();
            var cmd = createSqlCommandForUpdateAlarmState(alarmNum, deviceNum, newState, alarmVariables, alarmTime, solvedTime, userId);
            sqlCommands.Add(cmd);

            SqlExecute(sqlCommands);
        }

        public void BatchUpdateAlarmHistory(IEnumerable<AlarmHistoryRecord> updateAlarmStateRecords)
        {
            if (updateAlarmStateRecords == null || !updateAlarmStateRecords.Any())
                return;
            List<SqlCommand> sqlCommands = new List<SqlCommand>();
            foreach (var record in updateAlarmStateRecords)
            {
                var cmd = createSqlCommandForUpdateAlarmState(record.AlarmNum, record.DeviceNum, record.AlarmState, record.AlarmVariables, record.AlarmTime, record.SolvedTime, record.UserId);
                sqlCommands.Add(cmd);
            }
            SqlExecute(sqlCommands);
        }

        private SqlCommand createSqlCommandForUpdateAlarmState(string alarmNum, string deviceNum, string newState, IList<AlarmVariable>? alarmVariables, DateTime? alarmTime, DateTime? solvedTime, string userId)
        {
            //last_history为最近匹配到的预警历史信息记录。
            //alarm_record为匹配到的预警记录（设计时），即还没有报警过的预警信息记录。
            //注意：当statechange_history大于0时，说明当前预警信息已经报警过，此时属于运行历史记录。每修改一次预警状态，其statechange_history字段值就加1，以记录其状态变更的次数
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"WITH last_history AS (SELECT * FROM alarms WHERE a_num=@AlarmNum AND d_num=@DeviceNum AND statechange_history > 0 ORDER BY statechange_history DESC LIMIT 1),
            action_type AS (SELECT CASE WHEN EXISTS(SELECT 1 FROM last_history WHERE state=@NewState) THEN 'UPDATE' ELSE 'INSERT' END as action),
            alarm_record AS (SELECT * FROM alarms WHERE a_num=@AlarmNum AND d_num=@DeviceNum AND statechange_history = 0 LIMIT 1)");

            sb.Append(@"INSERT INTO alarms(a_num, c_num, d_num, content, level, tag, state, alarm_values, alarm_time, solve_time, user_id, statechange_history) 
                        SELECT a_num, c_num, d_num, content, level, tag, 
                            COALESCE(@NewState, t.state) AS state,
                            COALESCE(@AlarmVariables, t.alarm_values) AS alarm_values,
                            COALESCE(@AlarmTime, t.alarm_time) AS alarm_time,
                            COALESCE(@SolveTime, t.solve_time) AS solve_time,
                            COALESCE(@UserId, t.user_id) AS user_id,
                            CASE 
                                WHEN EXISTS(SELECT 1 FROM last_history) THEN (SELECT  statechange_history+1 FROM last_history)
                                ELSE (SELECT 1) END AS statechange_history
                        FROM alarm_record t WHERE (SELECT action FROM action_type) = 'INSERT';");

            var cmd = new SqlCommand(sb.ToString(), new Dictionary<string, object>()
            {
                { "@AlarmNum", alarmNum },
                { "@DeviceNum", deviceNum },
                { "@NewState", newState },
                { "@AlarmVariables", alarmVariables != null && alarmVariables.Any() ? JsonConvert.SerializeObject(alarmVariables) : null },
                { "@AlarmTime", alarmTime.HasValue ? alarmTime.Value : null },
                { "@SolveTime", solvedTime.HasValue ? solvedTime.Value : null },
                { "@UserId", userId }
            });
            return cmd;
        }
        #endregion

        #region 手动控制信息
        public Dictionary<string, IList<ControlInfoByManualEntity>> ReadControlInfosByManual()
        {
            SqlMapper.SetTypeMap(typeof(ControlInfoByManualEntity), new ColumnAttributeTypeMapper<ControlInfoByManualEntity>());
            var controlInfos = SqlQuery<ControlInfoByManualEntity>("SELECT * FROM manual_controls");
            if (controlInfos != null && controlInfos.Any())
            {
                return controlInfos.GroupBy(a => a.DeviceNum)
                             .ToDictionary(g => g.Key, g => (IList<ControlInfoByManualEntity>)g.ToList());
            }
            else
            {
                return new Dictionary<string, IList<ControlInfoByManualEntity>>();
            }
        }

        public void SaveControlInfosByManual(Dictionary<string, IList<ControlInfoByManualEntity>> deviceControlInfoDict)
        {
            List<SqlCommand> sqlCommands = new List<SqlCommand>();

            var controlInfoUpdateStateDict = checkControlInfoByManualForUpdating(ReadControlInfosByManual(), deviceControlInfoDict);
            foreach (var pair in controlInfoUpdateStateDict)
            {
                var controlInfo = pair.Key;
                if (pair.Value == ItemUpdateStates.Added || pair.Value == ItemUpdateStates.Modified)
                {
                    var cmd = new SqlCommand(@"INSERT INTO manual_controls(c_num, d_num, c_header, v_num, c_value) 
                                                VALUES (@CNum, @DeviceNum, @Header, @VarNum, @Value)
                                                ON CONFLICT(c_num, d_num) DO UPDATE
                                                SET c_header=@Header, v_num=@VarNum, c_value=@Value", new Dictionary<string, object>()
                                                {
                                                    { "@CNum", controlInfo.CNum },
                                                    { "@DeviceNum", controlInfo.DeviceNum },
                                                    { "@Header", controlInfo.Header },
                                                    { "@VarNum", controlInfo.VarNum },
                                                    { "@Value", controlInfo.Value }
                                                });
                    sqlCommands.Add(cmd);
                }
                else if (pair.Value == ItemUpdateStates.Deleted)
                {
                    var cmd = new SqlCommand("DELETE FROM manual_controls WHERE c_num = @CNum", new Dictionary<string, object>()
                    {
                        { "@CNum", controlInfo.CNum }
                    });
                    sqlCommands.Add(cmd);
                }
            }

            SqlExecute(sqlCommands);
        }

        private Dictionary<ControlInfoByManualEntity, ItemUpdateStates> checkControlInfoByManualForUpdating(Dictionary<string, IList<ControlInfoByManualEntity>> sourceControlInfoDict, Dictionary<string, IList<ControlInfoByManualEntity>> targetControlInfoDict)
        {
            if (sourceControlInfoDict == null || !sourceControlInfoDict.Any())
                return targetControlInfoDict != null ? targetControlInfoDict.SelectMany(kvp => kvp.Value).ToDictionary(a => a, a => ItemUpdateStates.Added) : new Dictionary<ControlInfoByManualEntity, ItemUpdateStates>();
            if (targetControlInfoDict == null || !targetControlInfoDict.Any())
            {
                if (sourceControlInfoDict != null && sourceControlInfoDict.Any())
                    return sourceControlInfoDict.SelectMany(kvp => kvp.Value).ToDictionary(a => a, a => ItemUpdateStates.Deleted);
                else
                    return new Dictionary<ControlInfoByManualEntity, ItemUpdateStates>();
            }
            Dictionary<ControlInfoByManualEntity, ItemUpdateStates> itemUpdateStateDict = new Dictionary<ControlInfoByManualEntity, ItemUpdateStates>();
            foreach (var targetKvp in targetControlInfoDict)
            {
                var deviceNum = targetKvp.Key;
                var targetControlInfos = targetKvp.Value;
                if (!sourceControlInfoDict.ContainsKey(deviceNum))
                {
                    //源集合中不存在该设备，则该设备的所有手动控制信息均为新增状态
                    foreach (var controlInfo in targetControlInfos)
                    {
                        itemUpdateStateDict.Add(controlInfo, ItemUpdateStates.Added);
                    }
                }
            }
            //先判断每个设备的更新状态，即它是新添加或被删除的，还是需要进一步比较
            foreach (var sourceKvp in sourceControlInfoDict)
            {
                var deviceNum = sourceKvp.Key;
                var sourceControlInfos = sourceKvp.Value;
                if (!targetControlInfoDict.ContainsKey(deviceNum))
                {
                    //目标集合中不存在该设备，则该设备的所有手动控制信息均为删除状态
                    foreach (var controlInfo in sourceControlInfos)
                    {
                        itemUpdateStateDict.Add(controlInfo, ItemUpdateStates.Deleted);
                    }
                }
                else
                {
                    var targetControlInfos = targetControlInfoDict[deviceNum];
                    var controlInfosForRemove = sourceControlInfos.Except(targetControlInfos, new ControlInfoByManualByNumComparer()).ToList();
                    controlInfosForRemove.ForEach(a => itemUpdateStateDict.Add(a, ItemUpdateStates.Deleted));
                    var controlInfosForAdd = targetControlInfos.Except(sourceControlInfos, new ControlInfoByManualByNumComparer()).ToList();
                    controlInfosForAdd.ForEach(a => itemUpdateStateDict.Add(a, ItemUpdateStates.Added));

                    var otherControlInfos = sourceControlInfos.Intersect(targetControlInfos, new ControlInfoByManualByNumComparer()).ToList(); //包括属性变更或未变更的手动控制信息
                    foreach (var sourceControlInfo in otherControlInfos)
                    {
                        var targetControlInfo = targetControlInfos.FirstOrDefault(a => string.Equals(a.CNum, sourceControlInfo.CNum, StringComparison.OrdinalIgnoreCase) && string.Equals(a.DeviceNum, sourceControlInfo.DeviceNum, StringComparison.OrdinalIgnoreCase));
                        if (targetControlInfo == null)
                            continue;
                        if (!targetControlInfo.Equals(sourceControlInfo))
                        {
                            itemUpdateStateDict.Add(targetControlInfo, ItemUpdateStates.Modified);
                        }
                        else
                        {
                            itemUpdateStateDict.Add(targetControlInfo, ItemUpdateStates.Unchanged);
                        }
                    }
                }
            }
            return itemUpdateStateDict;
        }
        #endregion

        #region 联动控制信息
        public void AddOrModifyControlInfoByTriggerToDevice(DeviceEntity device, ControlInfoByTriggerEntity controlInfo, ConditionEntity condition)
        {
            var sqlCommands = CreateSqlCommandsForAddOrModifyControlInfoByTrigger(device, controlInfo, condition);
            SqlExecute(sqlCommands);
        }

        public IList<SqlCommand> CreateSqlCommandsForAddOrModifyControlInfoByTrigger(DeviceEntity device, ControlInfoByTriggerEntity controlInfo, ConditionEntity condition)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
            SqlMapper.SetTypeMap(typeof(ConditionEntity), new ColumnAttributeTypeMapper<ConditionEntity>());
            SqlMapper.SetTypeMap(typeof(ControlInfoByTriggerEntity), new ColumnAttributeTypeMapper<ControlInfoByTriggerEntity>());

            AddCondition(condition);

            SqlCommand cmd = new SqlCommand(@"INSERT INTO trigger_controls(u_num, c_num, c_d_num, c_header, u_d_num, v_num, c_value) 
                                                VALUES (@LinkageNum, @ConditionNum, @ConditionDeviceNum, @Header, @LinkageDeviceNum, @VarNum, @Value)
                                                ON CONFLICT(c_d_num, u_num, u_d_num) DO UPDATE
                                                SET c_num=@ConditionNum, c_header=@Header, u_d_num=@LinkageDeviceNum, v_num=@VarNum, c_value=@Value", new Dictionary<string, object>()
                                                {
                                                    { "@LinkageNum", controlInfo.LinkageNum },
                                                    { "@ConditionNum", controlInfo.ConditionNum },
                                                    { "@ConditionDeviceNum", controlInfo.ConditionDeviceNum },
                                                    { "@Header", controlInfo.Header },
                                                    { "@LinkageDeviceNum", controlInfo.LinkageDeviceNum },
                                                    { "@VarNum", controlInfo.VarNum },
                                                    { "@Value", controlInfo.Value }
                                                });
            IList<SqlCommand> sqlCommands = new List<SqlCommand>();
            sqlCommands.Add(cmd);
            return sqlCommands;
        }

        public void DeleteControlInfoByTriggerFromDevice(DeviceEntity device, string linkageNum, string conditionDeviceNum)
        {
            var sqlCommands = CreateSqlCommandsForDeleteControlInfoByTrigger(device, linkageNum, conditionDeviceNum);
            SqlExecute(sqlCommands);
        }

        public IList<SqlCommand> CreateSqlCommandsForDeleteControlInfoByTrigger(DeviceEntity device, string linkageNum, string conditionDeviceNum)
        {
            if (device == null)
                return Enumerable.Empty<SqlCommand>().ToList();
            if (string.IsNullOrEmpty(linkageNum))
                return Enumerable.Empty<SqlCommand>().ToList();

            IList<SqlCommand> sqlCommands = new List<SqlCommand>();
            var cmd = new SqlCommand("DELETE FROM trigger_controls WHERE c_d_num=@ConditionDeviceNum AND u_num=@LinkageNum AND u_d_num=@LinkageDeviceNum", new Dictionary<string, object>()
                {
                    { "@ConditionDeviceNum", conditionDeviceNum },
                    { "@LinkageNum", linkageNum },
                    { "@LinkageDeviceNum", device.DeviceNum }
                });
            sqlCommands.Add(cmd);
            return sqlCommands;
        }

        public IList<ControlInfoByTriggerEntity> GetControlInfosByTriggerForDevice(DeviceEntity device)
        {
            if (device == null)
                return null;
            return SqlQuery<ControlInfoByTriggerEntity>("SELECT * FROM trigger_controls WHERE c_d_num = @ConditionDeviceNum", new Dictionary<string, object>()
            {
                { "@ConditionDeviceNum", device.DeviceNum }
            }).ToList();
        }

        public ControlInfoByTriggerEntity GetControlInfoByTriggerByNum(string linkageNum, DeviceEntity device, string conditionDeviceNum)
        {
            if (string.IsNullOrWhiteSpace(linkageNum))
                return null;
            if (device == null)
                return null;
            return SqlQueryFirst<ControlInfoByTriggerEntity>("SELECT * FROM trigger_controls WHERE c_d_num=@ConditionDeviceNum AND u_num=@LinkageNum AND u_d_num=@LinkageDeviceNum", new Dictionary<string, object>()
            {
                { "@ConditionDeviceNum", conditionDeviceNum },
                { "@LinkageNum", linkageNum },
                { "@LinkageDeviceNum", device.DeviceNum }
            });
        }

        public Dictionary<string, IList<ControlInfoByTriggerEntity>> ReadControlInfosByTrigger()
        {
            SqlMapper.SetTypeMap(typeof(ControlInfoByTriggerEntity), new ColumnAttributeTypeMapper<ControlInfoByTriggerEntity>());
            var controlInfos = SqlQuery<ControlInfoByTriggerEntity>("SELECT * FROM trigger_controls");
            if (controlInfos != null && controlInfos.Any())
            {
                return controlInfos.GroupBy(a => a.ConditionDeviceNum)
                             .ToDictionary(g => g.Key, g => (IList<ControlInfoByTriggerEntity>)g.ToList());
            }
            else
            {
                return new Dictionary<string, IList<ControlInfoByTriggerEntity>>();
            }
        }

        public void SaveControlInfosByTrigger(Dictionary<string, IList<ControlInfoByTriggerEntity>> deviceControlInfoDict, Dictionary<string, IList<ConditionEntity>> conditionDict)
        {                 
            List<SqlCommand> sqlCommands = new List<SqlCommand>();
            var topConditionDict = new Dictionary<string, ConditionEntity>();
            var conditionList = new List<ConditionEntity>(); //要写入的所有条件实体
            if (conditionDict != null)
            {
                foreach (var pair in conditionDict)
                {
                    if (!topConditionDict.ContainsKey(pair.Key))
                    {
                        topConditionDict.Add(pair.Key, pair.Value?.First(c => string.IsNullOrEmpty(c.CNum_Parent)));

                        conditionList.AddRange(pair.Value);
                    }
                }
            }

            var sqlCommandsForConditions = CreateSqlCommandsForAddingConditions(conditionList.DistinctBy(c => c.CNum));
            sqlCommands.AddRange(sqlCommandsForConditions);

            var controlInfoUpdateStateDict = checkControlInfosByTriggerForUpdating(ReadControlInfosByTrigger(), deviceControlInfoDict);
            foreach (var pair in controlInfoUpdateStateDict)
            {
                if (pair.Value == ItemUpdateStates.Added || pair.Value == ItemUpdateStates.Modified)
                {
                    var controlInfo = pair.Key;
                    if (topConditionDict.TryGetValue(controlInfo.ConditionNum, out ConditionEntity condition))
                    {
                        var sqlCommandsForAdd = CreateSqlCommandsForAddOrModifyControlInfoByTrigger(new DeviceEntity { DeviceNum = controlInfo.ConditionDeviceNum }, controlInfo, condition);
                        sqlCommands.AddRange(sqlCommandsForAdd);
                    }
                    else
                    {
                        //如果没有找到对应的一级联控条件，则当前联控选项信息无效，需将其从数据库中剔除
                        var sqlCommandsForDelete = CreateSqlCommandsForDeleteControlInfoByTrigger(new DeviceEntity { DeviceNum = controlInfo.ConditionDeviceNum }, controlInfo.LinkageNum, controlInfo.ConditionDeviceNum);
                        sqlCommands.AddRange(sqlCommandsForDelete);
                    }
                }
                else if (pair.Value == ItemUpdateStates.Deleted)
                {
                    var controlInfo = pair.Key;
                    var sqlCommandsForDelete = CreateSqlCommandsForDeleteControlInfoByTrigger(new DeviceEntity { DeviceNum = controlInfo.ConditionDeviceNum }, controlInfo.LinkageNum, controlInfo.ConditionDeviceNum);
                    sqlCommands.AddRange(sqlCommandsForDelete);
                }
            }

            SqlExecute(sqlCommands);
        }

        private Dictionary<ControlInfoByTriggerEntity, ItemUpdateStates> checkControlInfosByTriggerForUpdating(Dictionary<string, IList<ControlInfoByTriggerEntity>> sourceControlInfoDict, Dictionary<string, IList<ControlInfoByTriggerEntity>> targetControlInfoDict)
        {
            if (sourceControlInfoDict == null || !sourceControlInfoDict.Any())
                return targetControlInfoDict != null ? targetControlInfoDict.SelectMany(kvp => kvp.Value).ToDictionary(a => a, a => ItemUpdateStates.Added) : new Dictionary<ControlInfoByTriggerEntity, ItemUpdateStates>();
            if (targetControlInfoDict == null || !targetControlInfoDict.Any())
            {
                if (sourceControlInfoDict != null && sourceControlInfoDict.Any())
                    return sourceControlInfoDict.SelectMany(kvp => kvp.Value).ToDictionary(a => a, a => ItemUpdateStates.Deleted);
                else
                    return new Dictionary<ControlInfoByTriggerEntity, ItemUpdateStates>();
            }

            Dictionary<ControlInfoByTriggerEntity, ItemUpdateStates> itemUpdateStateDict = new Dictionary<ControlInfoByTriggerEntity, ItemUpdateStates>();
            foreach (var targetKvp in targetControlInfoDict)
            {
                var conditionDeviceNum = targetKvp.Key;
                var targetControlInfos = targetKvp.Value;
                if (!sourceControlInfoDict.ContainsKey(conditionDeviceNum))
                {
                    //源集合中不存在该设备，则该设备的所有手动控制信息均为新增状态
                    foreach (var controlInfo in targetControlInfos)
                    {
                        itemUpdateStateDict.Add(controlInfo, ItemUpdateStates.Added);
                    }
                }
            }
            //先判断每个设备的更新状态，即它是新添加或被删除的，还是需要进一步比较
            foreach (var sourceKvp in sourceControlInfoDict)
            {
                var conditionDeviceNum = sourceKvp.Key;
                var sourceControlInfos = sourceKvp.Value;
                if (!targetControlInfoDict.ContainsKey(conditionDeviceNum))
                {
                    //目标集合中不存在该设备，则该设备的所有报警信息均为删除状态
                    foreach (var controlInfo in sourceControlInfos)
                    {
                        itemUpdateStateDict.Add(controlInfo, ItemUpdateStates.Deleted);
                    }
                }
                else
                {
                    //当ConditionDeviceNum相同，即是同一个设备触发的条件，则需进一步根据ConditionDeviceNum，LinkageDeviceNum，LinkageNum来判断是否是trigger_controls表的同一条记录
                    //因为trigger_controls表的记录主键是由ConditionDeviceNum，LinkageNum，LinkageDeviceNum三个字段共同组成的
                    var targetControlInfos = targetControlInfoDict[conditionDeviceNum];
                    var controlInfosForRemove = sourceControlInfos.Except(targetControlInfos, new ControlInfoByTriggerByNumComparer()).ToList();
                    controlInfosForRemove.ForEach(a => itemUpdateStateDict.Add(a, ItemUpdateStates.Deleted));
                    var controlInfosForAdd = targetControlInfos.Except(sourceControlInfos, new ControlInfoByTriggerByNumComparer()).ToList();
                    controlInfosForAdd.ForEach(a => itemUpdateStateDict.Add(a, ItemUpdateStates.Added));

                    var otherControlInfos = sourceControlInfos.Intersect(targetControlInfos, new ControlInfoByTriggerByNumComparer()).ToList(); //包括属性变更或未变更的报警信息
                    foreach (var sourceControlInfo in otherControlInfos)
                    {
                        var targetControlInfo = targetControlInfos.FirstOrDefault(a => string.Equals(a.LinkageNum, sourceControlInfo.LinkageNum, StringComparison.OrdinalIgnoreCase) && string.Equals(a.ConditionDeviceNum, sourceControlInfo.ConditionDeviceNum, StringComparison.OrdinalIgnoreCase));
                        if (targetControlInfo == null)
                            continue;
                        if (!targetControlInfo.Equals(sourceControlInfo))
                        {
                            itemUpdateStateDict.Add(targetControlInfo, ItemUpdateStates.Modified);
                        }
                        else
                        {
                            itemUpdateStateDict.Add(targetControlInfo, ItemUpdateStates.Unchanged);
                        }
                    }
                }
            }
            return itemUpdateStateDict;
        }
        #endregion

        /// <summary>
        /// 删除那些不再使用的条件（包括顶级及其子条件）
        /// </summary>
        /// <param name="aliveConditionNumList">当前还在使用的顶级条件（包括预警条件和联动条件）</param>
        /// <param name="oldTopConditions">数据库中所有的顶级条件</param>
        public void CleanUpOutdatedConditions(IList<string> aliveConditionNumList, IEnumerable<ConditionEntity>? oldTopConditions)
        {
            List<SqlCommand> sqlCommands = new List<SqlCommand>();
            var topConditionsForDelete = oldTopConditions.Where(c => c.CNum_Parent == null && !aliveConditionNumList.Contains(c.CNum)).ToList();
            var sqlCommandsForDeleteConditions = CreateSqlCommandsForDeleteConditions(topConditionsForDelete);
            sqlCommands.AddRange(sqlCommandsForDeleteConditions);

            SqlExecute(sqlCommands);
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

        internal class AlarmByNumComparer : IEqualityComparer<AlarmEntity>
        {
            public bool Equals(AlarmEntity x, AlarmEntity y)
            {
                if (x == null && y == null)
                    return true;
                if (x == null || y == null)
                    return false;
                return string.Equals(x.AlarmNum, y.AlarmNum, StringComparison.OrdinalIgnoreCase) && string.Equals(x.DeviceNum, y.DeviceNum, StringComparison.OrdinalIgnoreCase);
            }
            public int GetHashCode(AlarmEntity obj)
            {
                if (obj == null || string.IsNullOrEmpty(obj.AlarmNum))
                    return 0;
                unchecked // 允许溢出
                {
                    int hash = 17;
                    hash = hash * 23 + (obj.AlarmNum?.GetHashCode() ?? 0);
                    hash = hash * 23 + (obj.DeviceNum?.GetHashCode() ?? 0);
                    return hash;
                }
            }
        }

        internal class ControlInfoByManualByNumComparer : IEqualityComparer<ControlInfoByManualEntity>
        {
            public bool Equals(ControlInfoByManualEntity? x, ControlInfoByManualEntity? y)
            {
                if (x == null && y == null)
                    return true;
                if (x == null || y == null)
                    return false;
                return string.Equals(x.CNum, y.CNum, StringComparison.OrdinalIgnoreCase) && string.Equals(x.DeviceNum, y.DeviceNum, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode([DisallowNull] ControlInfoByManualEntity obj)
            {
                if (obj == null || string.IsNullOrEmpty(obj.CNum))
                    return 0;
                unchecked // 允许溢出
                {
                    int hash = 17;
                    hash = hash * 23 + (obj.CNum?.GetHashCode() ?? 0);
                    hash = hash * 23 + (obj.DeviceNum?.GetHashCode() ?? 0);
                    return hash;
                }
            }
        }

        internal class ControlInfoByTriggerByNumComparer : IEqualityComparer<ControlInfoByTriggerEntity>
        {
            public bool Equals(ControlInfoByTriggerEntity? x, ControlInfoByTriggerEntity? y)
            {
                if (x == null && y == null)
                    return true;
                if (x == null || y == null)
                    return false;
                return string.Equals(x.ConditionDeviceNum, y.ConditionDeviceNum, StringComparison.OrdinalIgnoreCase) && string.Equals(x.LinkageNum, y.LinkageNum, StringComparison.OrdinalIgnoreCase) && string.Equals(x.LinkageDeviceNum, y.LinkageDeviceNum, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode([DisallowNull] ControlInfoByTriggerEntity obj)
            {
                if (obj == null || string.IsNullOrEmpty(obj.LinkageNum))
                    return 0;
                unchecked // 允许溢出
                {
                    int hash = 17;
                    hash = hash * 23 + (obj.ConditionDeviceNum?.GetHashCode() ?? 0);
                    hash = hash * 23 + (obj.LinkageNum?.GetHashCode() ?? 0);
                    hash = hash * 23 + (obj.LinkageDeviceNum?.GetHashCode() ?? 0);
                    return hash;
                }
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
}
