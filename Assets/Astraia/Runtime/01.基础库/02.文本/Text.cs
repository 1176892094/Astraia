// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-09 21:04:38
// // # Recently: 2025-04-09 21:04:38
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Text;

namespace Astraia
{
    public static partial class Service
    {
        public static partial class Text
        {
            [ThreadStatic]
            private static StringBuilder builder;
            
            private static StringBuilder Builder => builder ??= new StringBuilder(1024);

            public static string Format<T>(string format, T arg1)
            {
                Builder.Length = 0;
                Builder.AppendFormat(format, arg1);
                return Builder.ToString();
            }

            public static string Format<T1, T2>(string format, T1 arg1, T2 arg2)
            {
                Builder.Length = 0;
                Builder.AppendFormat(format, arg1, arg2);
                return Builder.ToString();
            }

            public static string Format<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
            {
                Builder.Length = 0;
                Builder.AppendFormat(format, arg1, arg2, arg3);
                return Builder.ToString();
            }

            public static string Format<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
            {
                Builder.Length = 0;
                Builder.AppendFormat(format, arg1, arg2, arg3, arg4);
                return Builder.ToString();
            }
        }
    }
}