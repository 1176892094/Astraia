using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Astraia.Core;

namespace Astraia.Editor
{
    internal static class CustomGenerator
    {
        private const string GEN_FUN = "CustomProcessor";

        public static bool Processed(AssemblyDefinition assembly, TypeDefinition td, Module module, ILogPostProcessor Log)
        {
            if (td.Methods.Any(m => m.Name == GEN_FUN))
            {
                return false;
            }

            var m1 = assembly.MainModule.ImportReference(typeof(Module<>));
            var m2 = assembly.MainModule.ImportReference(typeof(Module<>).GetField(nameof(Module<object>.owner)));
            var m4 = assembly.MainModule.ImportReference(GetOwnerFieldActualType(td));
            var m5 = m2.GenericField(assembly.MainModule, m1.MakeGeneric(m4));

            var modified = false;
            foreach (var f in td.Fields)
            {
                if (f.HasAttribute<InjectAttribute>())
                {
                    InjectField(GetMethod(td, module, module.Dequeue), module.Inject.MakeGeneric(f.FieldType), f, m5);
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
                        InjectEvent(GetMethod(td, module, module.OnShow), module.Listen.MakeGeneric(eventType));
                        InjectEvent(GetMethod(td, module, module.OnHide), module.Remove.MakeGeneric(eventType));
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

        public static TypeReference GetOwnerFieldActualType(TypeDefinition concreteType)
        {
            TypeReference current = concreteType;

            while (current != null)
            {
                var currentTypeDef = current.Resolve();
                if (currentTypeDef == null)
                {
                    break;
                }

                if (currentTypeDef.BaseType.Is(typeof(Module<>)) && currentTypeDef.BaseType is GenericInstanceType genericBase)
                {
                    return genericBase.GenericArguments[0];
                }

                current = currentTypeDef.BaseType;
            }

            return null;
        }

        private static void InjectField(MethodDefinition md, MethodReference method, FieldDefinition field, FieldReference owner)
        {
            var worker = md.Body.GetILProcessor();
            var target = md.Body.Instructions[0];
            worker.InsertBefore(target, worker.Create(OpCodes.Ldarg_0));
            worker.InsertBefore(target, worker.Create(OpCodes.Ldarg_0));
            worker.InsertBefore(target, worker.Create(OpCodes.Ldfld, owner));
            worker.InsertBefore(target, worker.Create(OpCodes.Ldstr, char.ToUpper(field.Name[0]) + field.Name.Substring(1)));
            worker.InsertBefore(target, worker.Create(OpCodes.Call, method));
            worker.InsertBefore(target, worker.Create(OpCodes.Stfld, field));
        }

        private static void InjectEvent(MethodDefinition md, MethodReference method)
        {
            var worker = md.Body.GetILProcessor();
            var target = md.Body.Instructions[0];
            worker.InsertBefore(target, worker.Create(OpCodes.Ldarg_0));
            worker.InsertBefore(target, worker.Create(OpCodes.Call, method));
        }

        private static MethodDefinition GetMethod(TypeDefinition td, Module module, MethodReference md)
        {
            var method = td.Methods.FirstOrDefault(m => m.Name == md.Name && m.Parameters.Count == 0);
            if (method == null)
            {
                method = new MethodDefinition(md.Name, Weaver.GEN_S1, module.Import(typeof(void)));
                var worker = method.Body.GetILProcessor();
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Call, md);
                worker.Emit(OpCodes.Ret);
                td.Methods.Add(method);
            }

            return method;
        }
    }
}