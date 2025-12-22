using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Common
{
    public class TypeUtils
    {
        public static Type GetTypeFromAssemblyQualifiedName(string assemblyQualifiedName)
        {
            if (string.IsNullOrWhiteSpace(assemblyQualifiedName))
            {
                throw new ArgumentException("Assembly qualified name cannot be null or empty.");
            }
            try
            {
                // 方法1：直接使用 Type.GetType()
                Type type = Type.GetType(assemblyQualifiedName, false, true);

                if (type != null)
                {
                    return type;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading type: {ex.Message}");
            }

            // 方法2：如果上面失败，尝试解析程序集
            int commaIndex = assemblyQualifiedName.IndexOf(',');
            if (commaIndex > 0)
            {
                string typeName = assemblyQualifiedName.Substring(0, commaIndex).Trim();
                string assemblyNamePart = assemblyQualifiedName.Substring(commaIndex + 1).Trim();

                // 提取程序集名称（可能包含版本等信息）
                string simpleAssemblyName = assemblyNamePart.Split(',')[0].Trim();

                try
                {
                    string dllFile = System.IO.Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName, $"{simpleAssemblyName}.dll");
                    Assembly assembly = Assembly.LoadFrom(dllFile);
                    return assembly.GetType(typeName, true, true);
                }
                catch
                {
                    //如果简单名称加载失败，尝试完整名称
                    Assembly assembly = Assembly.Load(assemblyNamePart);
                    return assembly.GetType(typeName, true, true);
                }
            }

            throw new TypeLoadException($"Unable to load type from: {assemblyQualifiedName}");
        }

        public static bool EqualCollection<T>( IEnumerable<T> list1, IEnumerable<T> list2)
        {
            if (list1 == null && list2 == null)
                return true;
            if (list1 == null || list2 == null)
                return false;
            if (ReferenceEquals(list1, list2)) 
                return true;
            if (list1.Count() != list2.Count())
                return false;
            foreach (T item in list1)
            {
                if (!list2.Contains(item))
                    return false;
            }
            return true;
        }
    }
}
