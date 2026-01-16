using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Common
{
    public static class DynamicClassCreator
    {
        // 动态创建包含属性的类
        public static Type CreateDynamicType(string typeName, Dictionary<string, Type> properties, Action<TypeBuilder ,Dictionary<string, PropertyBuilder>> callback)
        {
            // 1. 创建程序集
            AssemblyName assemblyName = new AssemblyName("Coffee.DigitalPlatform.Common");
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
                assemblyName,
                AssemblyBuilderAccess.RunAndCollect  // RunAndCollect支持垃圾回收
            );

            // 2. 创建模块
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

            // 3. 创建类型
            TypeBuilder typeBuilder = moduleBuilder.DefineType(
                typeName,
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit
            );

            var propertyBuildDict = new Dictionary<string, PropertyBuilder>();
            // 4. 为每个属性添加字段和属性
            foreach (var property in properties)
            {
                var propertyBuild = AddProperty(typeBuilder, property.Key, property.Value);
                if (!propertyBuildDict.ContainsKey(property.Key))
                {
                    propertyBuildDict.Add(property.Key, propertyBuild);
                }
            }

            // 5. 可选：添加默认构造函数
            AddDefaultConstructor(typeBuilder);

            //通过回调函数创建自定义属性
            callback(typeBuilder, propertyBuildDict);

            // 6. 创建类型
            Type dynamicType = typeBuilder.CreateType();
            return dynamicType;
        }

        private static PropertyBuilder AddProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
        {
            // 创建字段
            string fieldName = $"_{propertyName.ToLowerInvariant()}";
            FieldBuilder fieldBuilder = typeBuilder.DefineField(
                fieldName,
                propertyType,
                FieldAttributes.Private
            );

            // 创建属性
            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(
                propertyName,
                PropertyAttributes.HasDefault,
                propertyType,
                null
            );

            // 创建get方法
            MethodBuilder getMethodBuilder = typeBuilder.DefineMethod(
                $"get_{propertyName}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                propertyType,
                Type.EmptyTypes
            );

            ILGenerator getIl = getMethodBuilder.GetILGenerator();
            getIl.Emit(OpCodes.Ldarg_0);          // 加载this
            getIl.Emit(OpCodes.Ldfld, fieldBuilder); // 加载字段值
            getIl.Emit(OpCodes.Ret);              // 返回

            // 创建set方法
            MethodBuilder setMethodBuilder = typeBuilder.DefineMethod(
                $"set_{propertyName}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                null,
                new Type[] { propertyType }
            );

            ILGenerator setIl = setMethodBuilder.GetILGenerator();
            setIl.Emit(OpCodes.Ldarg_0);          // 加载this
            setIl.Emit(OpCodes.Ldarg_1);          // 加载参数值
            setIl.Emit(OpCodes.Stfld, fieldBuilder); // 存储到字段
            setIl.Emit(OpCodes.Ret);              // 返回

            // 关联方法到属性
            propertyBuilder.SetGetMethod(getMethodBuilder);
            propertyBuilder.SetSetMethod(setMethodBuilder);

            return propertyBuilder;
        }

        private static void AddDefaultConstructor(TypeBuilder typeBuilder)
        {
            ConstructorBuilder constructor = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                Type.EmptyTypes
            );

            ILGenerator il = constructor.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);  // 加载this
            il.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));
            il.Emit(OpCodes.Ret);
        }
    }
}
