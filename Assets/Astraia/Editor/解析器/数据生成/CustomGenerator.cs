using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Astraia.Editor
{
    internal static class CustomGenerator
    {
        private const string GEN_FUN = "EventProcessor";

        public static bool Processed(TypeDefinition td, Module module)
        {
            if (td.Methods.Any(m => m.Name == GEN_FUN))
            {
                return false;
            }

            var modified = false;
            foreach (var i in td.Interfaces)
            {
                if (i.InterfaceType is GenericInstanceType genericType)
                {
                    var elementType = genericType.ElementType.Resolve();
                    if (elementType.Is(typeof(IEvent<>)))
                    {
                        var eventType = genericType.GenericArguments[0];
                        Inject(GetOrAddMethod(td, "OnEnable", module), module.Listen.MakeGeneric(eventType));
                        Inject(GetOrAddMethod(td, "OnDisable", module), module.Remove.MakeGeneric(eventType));
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

        private static void Inject(MethodDefinition method, MethodReference targetMethod)
        {
            var worker = method.Body.GetILProcessor();
            var instructions = method.Body.Instructions;

            var retInstruction = instructions.LastOrDefault(i => i.OpCode == OpCodes.Ret);

            if (retInstruction == null)
            {
                worker.Emit(OpCodes.Ret);
                retInstruction = instructions.Last();
            }

            worker.InsertBefore(retInstruction, worker.Create(OpCodes.Ldarg_0));
            worker.InsertBefore(retInstruction, worker.Create(OpCodes.Call, targetMethod));
        }

        private static MethodDefinition GetOrAddMethod(TypeDefinition type, string name, Module module)
        {
            var existing = type.Methods.FirstOrDefault(m => m.Name == name && m.Parameters.Count == 0);

            if (existing != null)
            {
                return existing;
            }

            var method = new MethodDefinition(name, Weaver.GEN_VAR, module.Import(typeof(void)));

            var baseMethod = type.BaseType.Resolve().GetBaseMethod(name);
            var worker = method.Body.GetILProcessor();
            if (baseMethod != null)
            {
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Call, baseMethod);
            }

            worker.Emit(OpCodes.Ret);
            type.Methods.Add(method);
            return method;
        }
    }
}