using Coffee.DigitalPlatform.Common;
using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Entities.Converter
{
    public class ColumnAttributeTypeMapper<T> : FallbackTypeMapper
    {
        public ColumnAttributeTypeMapper()
            : base(new SqlMapper.ITypeMap[]
                {
                    new CustomPropertyTypeMap(
                       typeof(T),
                       (type, columnName) =>
                           type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(prop =>
                               prop.GetCustomAttributes(false)
                                   .OfType<ColumnAttribute>()
                                   .Any(attr => attr.Name == columnName)
                               )
                       ),
                    new DefaultTypeMap(typeof(T))
                })
        {
        }
    }

    public class FallbackTypeMapper : SqlMapper.ITypeMap
    {
        private readonly IEnumerable<SqlMapper.ITypeMap> _mappers;

        public FallbackTypeMapper(IEnumerable<SqlMapper.ITypeMap> mappers)
        {
            _mappers = mappers;
        }


        public ConstructorInfo FindConstructor(string[] names, Type[] types)
        {
            foreach (var mapper in _mappers)
            {
                try
                {
                    ConstructorInfo result = mapper.FindConstructor(names, types);
                    if (result != null)
                    {
                        return result;
                    }
                }
                catch (NotImplementedException)
                {
                }
            }
            return null;
        }

        public SqlMapper.IMemberMap GetConstructorParameter(ConstructorInfo constructor, string columnName)
        {
            foreach (var mapper in _mappers)
            {
                try
                {
                    var result = mapper.GetConstructorParameter(constructor, columnName);
                    if (result != null)
                    {
                        return result;
                    }
                }
                catch (NotImplementedException)
                {
                }
            }
            return null;
        }

        public SqlMapper.IMemberMap GetMember(string columnName)
        {
            foreach (var mapper in _mappers)
            {
                try
                {
                    var result = mapper.GetMember(columnName);
                    if (result != null)
                    {
                        return result;
                    }
                }
                catch (NotImplementedException)
                {
                }
            }
            return null;
        }


        public ConstructorInfo FindExplicitConstructor()
        {
            return _mappers
                .Select(mapper => mapper.FindExplicitConstructor())
                .FirstOrDefault(result => result != null);
        }
    }

    /// <summary>
    /// 字符串与Type类型转换处理器
    /// </summary>
    public class StringToTypeHandler : SqlMapper.TypeHandler<Type>
    {
        public override Type Parse(object value)
        {
            if (value == null || value == DBNull.Value)
                throw new ArgumentNullException($"对象的类型名不能为空！");

            if (!(value is string strValue))
                throw new ArgumentNullException($"对象的类型名格式不正确！");

            return TypeUtils.GetTypeFromAssemblyQualifiedName(strValue);
        }

        public override void SetValue(IDbDataParameter parameter, Type value)
        {
            parameter.Value = value.AssemblyQualifiedName;
        }
    }

    public class SqliteDateTimeHandler : SqlMapper.TypeHandler<DateTime?>
    {
        public override void SetValue(IDbDataParameter parameter, DateTime? value)
        {
            if (value.HasValue)
            {
                parameter.Value = value.Value.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                parameter.Value = DBNull.Value;
            }
        }

        public override DateTime? Parse(object value)
        {
            if (value == null || value == DBNull.Value)
                return null;
            if (!(value is string))
                throw new ArgumentNullException($"对象的日期时间格式不正确！");
            if (string.IsNullOrEmpty(value.ToString()))
                return null;
            return DateTime.TryParse(value.ToString(), out DateTime dateTime) ? dateTime : null;
        }
    }
}
