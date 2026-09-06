// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-07-10 13:07:12
// # Recently: 2026-09-06 15:32:39
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Astraia.Editor
{
    internal static class EntityProcess
    {
        public static bool Processed(AssemblyDefinition assembly, TypeDefinition td, Module module, AssemblyDebugger Log)
        {
            if (td.Methods.Any(m => m.Name == Weaver.MED_T2))
            {
                return false;
            }

            // 程序集内的元数据顺序不保证基类在前。若基类的 Awake 由 IL 织入生成，
            // 而派生类先被处理，GetMethod 会在基类链上直接找到更上层的 Export.Awake。
            // 因此先递归处理当前程序集中的基类，确保派生类能找到最近的一层 Awake。
            var parent = td.BaseType?.Resolve();
            var modified = false;
            if (parent != null && parent.Module == assembly.MainModule && parent.IsSubclassOf<Export>())
            {
                modified = Processed(assembly, parent, module, Log);
            }

            var changed = false;
            // 显式声明的 Awake 里的 base.Awake() 在编译期只能引用当前存在的基类方法，
            // 跨程序集织入的 Actor.Awake 需要在这里把调用重定向到最终会生成的方法。
            changed |= RepairLifecycle(assembly, td, "Awake");
            changed |= RepairLifecycle(assembly, td, "OnEnable");
            changed |= RepairLifecycle(assembly, td, "OnDisable");

            foreach (var f in td.Fields)
            {
                if (f.HasAttribute<ExportAttribute>())
                {
                    var awake = module.Export.MakeGeneric(assembly.MainModule, f.FieldType);
                    td.GetMethod(assembly, Weaver.GEN_S2, "Awake").InjectField(awake, f);
                    changed = true;
                }
            }

            foreach (var i in td.Interfaces)
            {
                if (i.InterfaceType is GenericInstanceType generic)
                {
                    var elementType = generic.ElementType.Resolve();
                    if (elementType.Is(typeof(IEvent<>)))
                    {
                        var eventType = generic.GenericArguments[0];
                        var onEnable = module.Listen.MakeGeneric(assembly.MainModule, eventType);
                        var onDisable = module.Remove.MakeGeneric(assembly.MainModule, eventType);
                        td.GetMethod(assembly, Weaver.GEN_S2, "OnEnable").InjectEvent(onEnable);
                        td.GetMethod(assembly, Weaver.GEN_S2, "OnDisable").InjectEvent(onDisable);
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                var method = new MethodDefinition(Weaver.MED_T2, MethodAttributes.Private, module.Import(typeof(void)));
                var worker = method.Body.GetILProcessor();
                worker.Emit(OpCodes.Ret);
                td.Methods.Add(method);
            }

            return modified || changed;
        }

        private static bool RepairLifecycle(AssemblyDefinition assembly, TypeDefinition td, string name)
        {
            var method = td.Methods.FirstOrDefault(md => md.Name == name && md.Parameters.Count == 0);
            if (method == null || !method.HasBody)
            {
                return false;
            }

            var changed = false;
            foreach (var instruction in method.Body.Instructions)
            {
                if (instruction.OpCode != OpCodes.Call || instruction.Operand is not MethodReference call)
                {
                    continue;
                }

                if (call.Name != name || call.Parameters.Count != 0 || !call.HasThis || !IsBaseCall(call.DeclaringType, td))
                {
                    continue;
                }

                var reason = CustomExtensions.FindBaseMethod(td.BaseType, assembly, name);
                if (reason != null && reason.FullName != call.FullName)
                {
                    instruction.Operand = reason;
                    changed = true;
                }
            }

            return changed;
        }

        private static bool IsBaseCall(TypeReference type, TypeDefinition td)
        {
            var current = td.BaseType;
            while (current != null)
            {
                if (current.FullName == type.FullName)
                {
                    return true;
                }

                current = current.GetBaseType();
            }

            return false;
        }
    }

    internal static class CustomExtensions
    {
        public static void InjectField(this MethodDefinition md, MethodReference method, FieldDefinition field)
        {
            var worker = md.Body.GetILProcessor();
            var target = md.Body.Instructions[0];
            worker.InsertBefore(target, worker.Create(OpCodes.Ldarg_0));
            worker.InsertBefore(target, worker.Create(OpCodes.Ldarg_0));
            worker.InsertBefore(target, worker.Create(OpCodes.Ldstr, char.ToUpper(field.Name[0]) + field.Name.Substring(1)));
            worker.InsertBefore(target, worker.Create(OpCodes.Call, method));
            worker.InsertBefore(target, worker.Create(OpCodes.Stfld, field));
        }

        public static void InjectEvent(this MethodDefinition md, MethodReference method)
        {
            var worker = md.Body.GetILProcessor();
            var target = md.Body.Instructions[0];
            worker.InsertBefore(target, worker.Create(OpCodes.Ldarg_0));
            worker.InsertBefore(target, worker.Create(OpCodes.Call, method));
        }

        public static MethodDefinition GetMethod(this TypeDefinition td, AssemblyDefinition ad, MethodAttributes attrs, string name)
        {
            var method = td.Methods.FirstOrDefault(m => m.Name == name && m.Parameters.Count == 0);
            if (method == null)
            {
                method = new MethodDefinition(name, attrs, ad.MainModule.ImportReference(typeof(void)));
                var result = FindBaseMethod(td.BaseType, ad, name);
                var worker = method.Body.GetILProcessor();
                if (result != null)
                {
                    worker.Emit(OpCodes.Ldarg_0);
                    worker.Emit(OpCodes.Call, result);
                }

                worker.Emit(OpCodes.Ret);
                td.Methods.Add(method);
            }

            return method;
        }

        internal static MethodReference FindBaseMethod(TypeReference current, AssemblyDefinition ad, string name)
        {
            while (current != null)
            {
                var type = current.Resolve();
                var method = type.Methods.FirstOrDefault(md => md.Name == name && md.Parameters.Count == 0);
                if (method != null)
                {
                    MethodReference result = method;
                    if (current is GenericInstanceType generic)
                    {
                        result = result.GenericInstance(current.Module, generic);
                    }

                    return ad.MainModule.ImportReference(result);
                }

                // 基类在另一个程序集中时，Unity ILPP 会并行处理各程序集，
                // 因此织入基类生成的 Awake/OnEnable 等方法在这里还看不到。
                // 但既然基类同样会被织入，直接引用这个尚不存在的方法即可，
                // 运行时基类程序集已完成织入，方法引用可以正常解析。
                if (type.Module != ad.MainModule && type.IsSubclassOf<Export>() && WillGenerateMethod(type, name))
                {
                    var reason = new MethodReference(name, ad.MainModule.ImportReference(typeof(void)), current)
                    {
                        HasThis = true
                    };
                    return ad.MainModule.ImportReference(reason);
                }

                current = current.GetBaseType();
            }

            return null;
        }

        private static bool WillGenerateMethod(TypeDefinition type, string name)
        {
            if (name == "Awake")
            {
                return type.Fields.Any(field => field.HasAttribute<ExportAttribute>());
            }

            if (name == "OnEnable" || name == "OnDisable")
            {
                return type.Interfaces.Any(item => item.InterfaceType is GenericInstanceType generic && generic.ElementType.Resolve().Is(typeof(IEvent<>)));
            }

            return false;
        }
    }
}
