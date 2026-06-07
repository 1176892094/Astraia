using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Astraia.Editor
{
    internal static class CustomGenerator
    {
        private const string GEN_FUN = "CustomProcessor";

        public static bool Processed(AssemblyDefinition assembly, TypeDefinition td, Module module, ILogPostProcessor debugger)
        {
            if (td.Methods.Any(m => m.Name == GEN_FUN))
            {
                return false;
            }

            var modified = false;
            foreach (var field in td.Fields)
            {
                if (field.HasAttribute<InjectAttribute>())
                {
                    InjectField(GetOrAddMethod(assembly, td, "Dequeue", module), field, module.Inject.MakeGeneric(field.FieldType));
                    modified = true;
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
                        InjectEvent(GetOrAddMethod(assembly, td, "OnShow", module), module.Listen.MakeGeneric(eventType));
                        InjectEvent(GetOrAddMethod(assembly, td, "OnHide", module), module.Remove.MakeGeneric(eventType));
                        modified = true;
                    }
                }
            }

            if (modified)
            {
                var method = new MethodDefinition(GEN_FUN, MethodAttributes.Private, module.Import(typeof(void)));
                var worker = method.Body.GetILProcessor();
                worker.Emit(OpCodes.Ret);
                td.Methods.Add(method);
            }

            return modified;
        }

        private static void InjectField(MethodDefinition md, FieldDefinition field, MethodReference method)
        {
            var worker = md.Body.GetILProcessor();
            var firstInstruction = md.Body.Instructions[0];
            var name = char.ToUpper(field.Name[0]) + field.Name.Substring(1);
            worker.InsertBefore(firstInstruction, worker.Create(OpCodes.Ldarg_0));
            worker.InsertBefore(firstInstruction, worker.Create(OpCodes.Ldarg_0));
            worker.InsertBefore(firstInstruction, worker.Create(OpCodes.Ldstr, name));
            worker.InsertBefore(firstInstruction, worker.Create(OpCodes.Call, method));
            worker.InsertBefore(firstInstruction, worker.Create(OpCodes.Stfld, field));
        }

        private static void InjectEvent(MethodDefinition md, MethodReference method)
        {
            var worker = md.Body.GetILProcessor();
            var firstInstruction = md.Body.Instructions[0];
            worker.InsertBefore(firstInstruction, worker.Create(OpCodes.Ldarg_0));
            worker.InsertBefore(firstInstruction, worker.Create(OpCodes.Call, method));
        }

        private static MethodDefinition GetOrAddMethod(AssemblyDefinition assembly, TypeDefinition td, string name, Module module)
        {
            var existing = td.Methods.FirstOrDefault(m => m.Name == name && m.Parameters.Count == 0);
            if (existing != null)
            {
                return existing;
            }

            var method = new MethodDefinition(name, Weaver.GEN_VAR, module.Import(typeof(void)));
            var parent = td.BaseType;

            MethodReference methodRef = null;

            if (parent is GenericInstanceType generic)
            {
                var resolvedType = generic.ElementType.Resolve();
                var methodDef = resolvedType.GetBaseMethod(name);
                if (methodDef != null && IsMethodAccessible(methodDef))
                {
                    methodRef = methodDef.GenericInstance(assembly.MainModule, generic);
                }
            }
            else
            {
                var resolvedType = parent.Resolve();
                var methodDef = resolvedType.GetBaseMethod(name);
                if (methodDef != null && IsMethodAccessible(methodDef))
                {
                    methodRef = module.Import(methodDef);
                }
            }

            var worker = method.Body.GetILProcessor();
            if (methodRef != null)
            {
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Call, methodRef);
            }

            worker.Emit(OpCodes.Ret);
            td.Methods.Add(method);
            return method;
        }

        private static bool IsMethodAccessible(MethodDefinition method)
        {
            return method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly || method.IsFamilyAndAssembly;
        }
    }
}