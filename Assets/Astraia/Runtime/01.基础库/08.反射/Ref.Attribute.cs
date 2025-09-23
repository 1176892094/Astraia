// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-09-23 14:09:04
// // # Recently: 2025-09-23 14:09:04
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Astraia
{
    public static class Attribute<T> where T : Attribute
    {
        private static readonly Dictionary<MemberInfo, Dictionary<Type, T[]>> attributeCache = new();

        public static T GetAttribute(MemberInfo member, bool inherit = true)
        {
            var attrs = GetAttributes(member, inherit);
            return attrs.Length > 0 ? attrs[0] : null;
        }

        public static T[] GetAttributes(MemberInfo member, bool inherit = true)
        {
            if (!attributeCache.TryGetValue(member, out var mapper))
            {
                mapper = new Dictionary<Type, T[]>();
                attributeCache[member] = mapper;
            }

            if (!mapper.TryGetValue(typeof(T), out var result))
            {
                var reason = member.GetCustomAttributes(typeof(T), inherit);
                result = new T[reason.Length];
                for (int i = 0; i < reason.Length; i++)
                {
                    result[i] = (T)reason[i];
                }

                mapper.Add(typeof(T), result);
            }

            return result;
        }
    }
}