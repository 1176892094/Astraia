// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-08-25 21:08:20
// // # Recently: 2025-08-25 21:08:20
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Astraia
{
    public static partial class Service
    {
        public static partial class Find
        {
            private static readonly Dictionary<Type, Dictionary<string, Func<object, object>>> getterCache = new();
            private static readonly Dictionary<Type, Dictionary<string, Action<object, object>>> setterCache = new();
            private static readonly Dictionary<Type, Dictionary<string, Func<object, object[], object>>> methodCache = new();

            internal static object Invoke(object target, string name, params object[] args)
            {
                var targetType = target as Type ?? target.GetType();
                if (!methodCache.TryGetValue(targetType, out var methods))
                {
                    methods = new Dictionary<string, Func<object, object[], object>>();
                    methodCache[targetType] = methods;
                }

                if (!methods.TryGetValue(name, out var method))
                {
                    var parameters = args.Select(a => a.GetType()).ToArray();
                    for (var current = targetType; current != null; current = current.BaseType)
                    {
                        var result = current.GetMethod(name, Declared, null, parameters, null);
                        if (result != null)
                        {
                            method = LoadFunction(result);
                            methods[name] = method;
                            break;
                        }
                    }
                }

                if (method == null)
                {
                    throw new MissingMethodException(targetType.FullName, name);
                }

                return method.Invoke(target is Type ? null : target, args);
            }

            internal static object GetValue(object target, string name)
            {
                var targetType = target as Type ?? target.GetType();
                if (!getterCache.TryGetValue(targetType, out var getters))
                {
                    getters = new Dictionary<string, Func<object, object>>();
                    getterCache[targetType] = getters;
                }

                if (!getters.TryGetValue(name, out var getter))
                {
                    for (var current = targetType; current != null; current = current.BaseType)
                    {
                        var field = current.GetField(name, Declared);
                        if (field != null)
                        {
                            getter = LoadGetter(field);
                            getters[name] = getter;
                            break;
                        }

                        var property = current.GetProperty(name, Declared);
                        if (property != null && property.CanRead)
                        {
                            getter = LoadGetter(property, property.GetGetMethod(true));
                            getters[name] = getter;
                            break;
                        }
                    }
                }

                if (getter == null)
                {
                    throw new MissingMemberException(targetType.FullName, name);
                }

                return getter.Invoke(target is Type ? null : target);
            }

            internal static void SetValue<T>(object target, string name, T value)
            {
                var targetType = target as Type ?? target.GetType();
                if (!setterCache.TryGetValue(targetType, out var setters))
                {
                    setters = new Dictionary<string, Action<object, object>>();
                    setterCache[targetType] = setters;
                }

                if (!setters.TryGetValue(name, out var setter))
                {
                    for (var current = targetType; current != null; current = current.BaseType)
                    {
                        var field = current.GetField(name, Declared);

                        if (field != null)
                        {
                            setter = LoadSetter<T>(field);
                            setters[name] = setter;
                            break;
                        }

                        var property = current.GetProperty(name, Declared);
                        if (property != null && property.CanRead)
                        {
                            setter = LoadSetter<T>(property, property.GetSetMethod(true));
                            setters[name] = setter;
                            break;
                        }
                    }
                }

                if (setter == null)
                {
                    throw new MissingMemberException(targetType.FullName, name);
                }

                setter.Invoke(target is Type ? null : target, value);
            }

            private static Func<object, object[], object> LoadFunction(MethodInfo method)
            {
                var name = "invoke_{0}_{1}".Format(method.DeclaringType!.Name, method.Name);
                var DM = new DynamicMethod(name, typeof(object), new[] { typeof(object), typeof(object[]) }, method.DeclaringType, true);
                var IL = DM.GetILGenerator();

                if (!method.IsStatic)
                {
                    IL.Emit(OpCodes.Ldarg_0);
                    IL.Emit(method.DeclaringType.IsValueType ? OpCodes.Unbox : OpCodes.Castclass, method.DeclaringType);
                }

                var parameters = method.GetParameters();
                for (var i = 0; i < parameters.Length; i++)
                {
                    IL.Emit(OpCodes.Ldarg_1);
                    IL.Emit(OpCodes.Ldc_I4, i);
                    IL.Emit(OpCodes.Ldelem_Ref);

                    var parameter = parameters[i].ParameterType;
                    IL.Emit(parameter.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, parameter);
                }

                IL.EmitCall(method.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, method, null);
                if (method.ReturnType == typeof(void))
                {
                    IL.Emit(OpCodes.Ldnull);
                }
                else if (method.ReturnType.IsValueType)
                {
                    IL.Emit(OpCodes.Box, method.ReturnType);
                }

                IL.Emit(OpCodes.Ret);
                return (Func<object, object[], object>)DM.CreateDelegate(typeof(Func<object, object[], object>));
            }

            private static Func<object, object> LoadGetter(FieldInfo field)
            {
                var name = "get_{0}_{1}".Format(field.DeclaringType!.Name, field.Name);
                var DM = new DynamicMethod(name, typeof(object), new[] { typeof(object) }, field.DeclaringType, true);
                var IL = DM.GetILGenerator();

                if (!field.IsStatic)
                {
                    IL.Emit(OpCodes.Ldarg_0);
                    IL.Emit(field.DeclaringType.IsValueType ? OpCodes.Unbox : OpCodes.Castclass, field.DeclaringType);
                }

                IL.Emit(!field.IsStatic ? OpCodes.Ldfld : OpCodes.Ldsfld, field);
                if (field.FieldType.IsValueType)
                {
                    IL.Emit(OpCodes.Box, field.FieldType);
                }

                IL.Emit(OpCodes.Ret);
                return (Func<object, object>)DM.CreateDelegate(typeof(Func<object, object>));
            }

            private static Action<object, object> LoadSetter<T>(FieldInfo field)
            {
                var name = "set_{0}_{1}".Format(field.DeclaringType!.Name, field.Name);
                var DM = new DynamicMethod(name, typeof(void), new[] { typeof(object), typeof(object) }, field.DeclaringType, true);
                var IL = DM.GetILGenerator();

                if (!field.IsStatic)
                {
                    IL.Emit(OpCodes.Ldarg_0);
                    IL.Emit(field.DeclaringType.IsValueType ? OpCodes.Unbox : OpCodes.Castclass, field.DeclaringType);
                }

                IL.Emit(OpCodes.Ldarg_1);
                IL.Emit(field.FieldType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, typeof(T));
                IL.Emit(!field.IsStatic ? OpCodes.Stfld : OpCodes.Stsfld, field);
                IL.Emit(OpCodes.Ret);
                return (Action<object, object>)DM.CreateDelegate(typeof(Action<object, object>));
            }

            private static Func<object, object> LoadGetter(PropertyInfo property, MethodInfo method)
            {
                var name = "get_{0}_{1}".Format(property.DeclaringType!.Name, property.Name);
                var DM = new DynamicMethod(name, typeof(object), new[] { typeof(object) }, property.DeclaringType, true);
                var IL = DM.GetILGenerator();

                if (!method.IsStatic)
                {
                    IL.Emit(OpCodes.Ldarg_0);
                    IL.Emit(property.DeclaringType.IsValueType ? OpCodes.Unbox : OpCodes.Castclass, property.DeclaringType);
                }

                IL.EmitCall(method.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, method, null);
                if (property.PropertyType.IsValueType)
                {
                    IL.Emit(OpCodes.Box, property.PropertyType);
                }

                IL.Emit(OpCodes.Ret);
                return (Func<object, object>)DM.CreateDelegate(typeof(Func<object, object>));
            }

            private static Action<object, object> LoadSetter<T>(PropertyInfo property, MethodInfo method)
            {
                var name = "set_{0}_{1}".Format(property.DeclaringType!.Name, property.Name);
                var DM = new DynamicMethod(name, typeof(void), new[] { typeof(object), typeof(object) }, property.DeclaringType, true);
                var IL = DM.GetILGenerator();

                if (!method.IsStatic)
                {
                    IL.Emit(OpCodes.Ldarg_0);
                    IL.Emit(property.DeclaringType.IsValueType ? OpCodes.Unbox : OpCodes.Castclass, property.DeclaringType);
                }

                IL.Emit(OpCodes.Ldarg_1);
                IL.Emit(property.PropertyType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, typeof(T));
                IL.EmitCall(method.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, method, null);
                IL.Emit(OpCodes.Ret);
                return (Action<object, object>)DM.CreateDelegate(typeof(Action<object, object>));
            }
        }
    }
}