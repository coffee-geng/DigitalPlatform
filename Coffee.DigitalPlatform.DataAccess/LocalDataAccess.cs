using Coffee.DigitalPlatform.Common;
using Coffee.DigitalPlatform.Entities;
using Coffee.DigitalPlatform.Entities.Converter;
using Coffee.DigitalPlatform.IDataAccess;
using Dapper;
using Microsoft.Data.Sqlite;
using SQLitePCL;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Dynamic;
using System.Net.Http.Headers;
using System.Reflection;

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
                            if (cmd == null) continue;
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

        //得到通信协议参数定义
        public CommunicationParameterDefinitionEntity GetProtocolParamDefinition()
        {
            SqlMapper.SetTypeMap(typeof(CommunicationParameterDefinitionEntity), new ColumnAttributeTypeMapper<CommunicationParameterDefinitionEntity>());
            string sql = "select * from properties where p_name='Protocol'";
            var results = SqlQuery<CommunicationParameterDefinitionEntity>(sql);
            if (results != null && results.Any())
            {
                var entity = results.First();
                Dictionary<string, object> paramDict = new Dictionary<string, object>();
                paramDict.Add("@PropName", entity.ParameterName);
                var protocolNames = SqlQuery<string>("select protocol from properties_protocols where prop_name=@PropName", paramDict);
                entity.ProtocolNames = protocolNames != null ? protocolNames.ToList() : new List<string>();
                return entity;
            }
            else
                return null;
        }

        //得到某种协议的通信参数定义集合
        public IList<CommunicationParameterDefinitionEntity> GetCommunicationParamDefinitions(string protocol)
        {
            SqlMapper.SetTypeMap(typeof(CommunicationParameterDefinitionEntity), new ColumnAttributeTypeMapper<CommunicationParameterDefinitionEntity>());
            string sql = "SELECT p.* FROM properties p LEFT JOIN properties_protocols pp ON p.p_name = pp.prop_name WHERE pp.protocol=@Protocol";
            Dictionary<string, object> paramDict = new Dictionary<string, object>();
            paramDict.Add("@Protocol", protocol);
            var results = SqlQuery<dynamic>(sql, paramDict);
            if (results != null && results.Any())
                return results.Select(p => new CommunicationParameterDefinitionEntity
                {
                    ParameterName = p.p_name,
                    Label = p.p_header,
                    ValueInputType = (int)Enum.Parse(typeof(ValueInputTypes), p.p_type.ToString()),
                    DefaultValueOption = p.p_default,
                    IsDefaultParameter = (p.is_default != null || Convert.IsDBNull(p.is_default)) ? (bool)Convert.ChangeType(p.is_default, typeof(bool)) : false,

                }).ToList();
            else
                return null;
        }

        //得到所有通信参数定义集合
        public IList<CommunicationParameterDefinitionEntity> GetCommunicationParamDefinitions()
        {
            SqlMapper.SetTypeMap(typeof(CommunicationParameterDefinitionEntity), new ColumnAttributeTypeMapper<CommunicationParameterDefinitionEntity>());
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
            string sql = "SELECT * FROM device_properties WHERE device_num='@DeviceNum'";
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
        #endregion
    }
}
