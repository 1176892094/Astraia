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

namespace Astraia
{
    internal static class ExportProcess
    {
        public static bool Processed(AssemblyDefinition assembly, TypeDefinition td, Module module, AssemblyDebugger debugger)
        {
            if (td.Methods.Any(m => m.Name == Weaver.MED_T2))
            {
                return false;
            }

            var changed = false;
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

            return changed;
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

                var reason = FindBaseMethod(td.BaseType, assembly, name);
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

        private static void InjectField(this MethodDefinition md, MethodReference method, FieldDefinition field)
        {
            var worker = md.Body.GetILProcessor();
            var target = md.Body.Instructions[0];
            worker.InsertBefore(target, worker.Create(OpCodes.Ldarg_0));
            worker.InsertBefore(target, worker.Create(OpCodes.Ldarg_0));
            worker.InsertBefore(target, worker.Create(OpCodes.Ldstr, char.ToUpper(field.Name[0]) + field.Name.Substring(1)));
            worker.InsertBefore(target, worker.Create(OpCodes.Call, method));
            worker.InsertBefore(target, worker.Create(OpCodes.Stfld, field));
        }

        private static void InjectEvent(this MethodDefinition md, MethodReference method)
        {
            var worker = md.Body.GetILProcessor();
            var target = md.Body.Instructions[0];
            worker.InsertBefore(target, worker.Create(OpCodes.Ldarg_0));
            worker.InsertBefore(target, worker.Create(OpCodes.Call, method));
        }

        private static MethodDefinition GetMethod(this TypeDefinition td, AssemblyDefinition ad, MethodAttributes attrs, string name)
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

        private static MethodReference FindBaseMethod(TypeReference current, AssemblyDefinition ad, string name)
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
                
                if (type.IsSubclassOf<Export>() && WillGenerateMethod(type, name))
                {
                    var reason = new MethodReference(name, ad.MainModule.ImportReference(typeof(void)), current);
                    reason.HasThis = true;
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
