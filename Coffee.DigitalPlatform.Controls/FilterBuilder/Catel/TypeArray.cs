using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public static class TypeArray
    {
        private static class ArrayCache<T1, T2, T3, T4, T5>
        {
            internal static readonly Type[] Value = new Type[5]
            {
            typeof(T1),
            typeof(T2),
            typeof(T3),
            typeof(T4),
            typeof(T5)
            };
        }

        private static class ArrayCache<T1, T2, T3, T4>
        {
            internal static readonly Type[] Value = new Type[4]
            {
            typeof(T1),
            typeof(T2),
            typeof(T3),
            typeof(T4)
            };
        }

        private static class ArrayCache<T1, T2, T3>
        {
            internal static readonly Type[] Value = new Type[3]
            {
            typeof(T1),
            typeof(T2),
            typeof(T3)
            };
        }

        private static class ArrayCache<T1, T2>
        {
            internal static readonly Type[] Value = new Type[2]
            {
            typeof(T1),
            typeof(T2)
            };
        }

        private static class ArrayCache<T1>
        {
            internal static readonly Type[] Value = new Type[1] { typeof(T1) };
        }

        //
        // 摘要:
        //     Gets an array of type from two type parameters.
        //
        // 类型参数:
        //   T1:
        //     The type 1
        //
        //   T2:
        //     The type 2
        //
        //   T3:
        //     The type 3
        //
        //   T4:
        //     The type 4
        //
        //   T5:
        //     The type 5
        //
        // 返回结果:
        //     Array of types
        public static Type[] From<T1, T2, T3, T4, T5>()
        {
            return ArrayCache<T1, T2, T3, T4, T5>.Value;
        }

        //
        // 摘要:
        //     Gets an array of type from two type parameters.
        //
        // 类型参数:
        //   T1:
        //     The type 1
        //
        //   T2:
        //     The type 2
        //
        //   T3:
        //     The type 3
        //
        //   T4:
        //     The type 4
        //
        // 返回结果:
        //     Array of types
        public static Type[] From<T1, T2, T3, T4>()
        {
            return ArrayCache<T1, T2, T3, T4>.Value;
        }

        //
        // 摘要:
        //     Gets an array of type from two type parameters.
        //
        // 类型参数:
        //   T1:
        //     The type 1
        //
        //   T2:
        //     The type 2
        //
        //   T3:
        //     The type 3
        //
        // 返回结果:
        //     Array of types
        public static Type[] From<T1, T2, T3>()
        {
            return ArrayCache<T1, T2, T3>.Value;
        }

        //
        // 摘要:
        //     Gets an array of type from two type parameters.
        //
        // 类型参数:
        //   T1:
        //     The type 1
        //
        //   T2:
        //     The type 2
        //
        // 返回结果:
        //     Array of types
        public static Type[] From<T1, T2>()
        {
            return ArrayCache<T1, T2>.Value;
        }

        //
        // 摘要:
        //     Gets an array of type from two type parameters.
        //
        // 类型参数:
        //   T:
        //     The type
        //
        // 返回结果:
        //     Array of types
        public static Type[] From<T>()
        {
            return ArrayCache<T>.Value;
        }
    }
}
