using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Astraia.Editor
{
    internal static class CustomGenerator
    {
        private const string GEN_FUN = "CustomProcessor";

        public static bool Processed(TypeDefinition td, Module module, ILogPostProcessor debugger)
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
                    InjectField(GetOrAddMethod(td, "Awake", module), field, module);
                    modified = true;
                }
            }

            foreach (var i in td.Interfaces)
            {
                if (i.InterfaceType is GenericInstanceType genericType)
                {
                    var elementType = genericType.ElementType.Resolve();
                    if (elementType.Is(typeof(IEvent<>)))
                    {
                        var eventType = genericType.GenericArguments[0];
                        InjectEvent(GetOrAddMethod(td, "OnEnable", module), module.Listen.MakeGeneric(eventType));
                        InjectEvent(GetOrAddMethod(td, "OnDisable", module), module.Remove.MakeGeneric(eventType));
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

        private static void InjectField(MethodDefinition md, FieldDefinition field, Module module)
        {
            var worker = md.Body.GetILProcessor();
            var firstInstruction = md.Body.Instructions[0];

            var name = char.ToUpper(field.Name[0]) + field.Name.Substring(1);
            var instructions = new List<Instruction>
            {
                worker.Create(OpCodes.Ldarg_0),
                worker.Create(OpCodes.Ldarg_0),
                worker.Create(!string.IsNullOrEmpty(name) ? OpCodes.Ldstr : OpCodes.Ldnull, name),
                worker.Create(OpCodes.Call, module.Inject.MakeGeneric(field.FieldType)),
                worker.Create(OpCodes.Stfld, field)
            };
            foreach (var i in instructions)
            {
                worker.InsertBefore(firstInstruction, i);
            }
        }

        private static void InjectEvent(MethodDefinition md, MethodReference method)
        {
            var worker = md.Body.GetILProcessor();
            var firstInstruction = md.Body.Instructions[0];
            var instructions = new List<Instruction> { worker.Create(OpCodes.Ldarg_0), worker.Create(OpCodes.Call, method), };
            foreach (var i in instructions)
            {
                worker.InsertBefore(firstInstruction, i);
            }
        }

        private static MethodDefinition GetOrAddMethod(TypeDefinition td, string name, Module module)
        {
            var existing = td.Methods.FirstOrDefault(m => m.Name == name && m.Parameters.Count == 0);
            if (existing != null)
            {
                return existing;
            }

            var method = new MethodDefinition(name, Weaver.GEN_VAR, module.Import(typeof(void)));
            var result = td.BaseType.Resolve().GetBaseMethod(name);
            var worker = method.Body.GetILProcessor();
            if (result != null && IsMethodAccessible(result))
            {
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Call, result);
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