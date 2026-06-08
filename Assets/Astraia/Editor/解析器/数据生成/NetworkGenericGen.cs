using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Astraia.Editor
{
    internal static class EntityGenerator
    {
        public static bool Processed(TypeDefinition td, Module module)
        {
            if (td.Methods.Any(m => m.Name == Weaver.MED_T1))
            {
                return false;
            }

            var modified = false;
            foreach (var f in td.Fields)
            {
                if (f.HasAttribute<InjectAttribute>())
                {
                    InjectField(GetMethod(td, module, module.Awake), module.Inject.MakeGeneric(f.FieldType), f);
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
                        InjectEvent(GetMethod(td, module, module.OnEnable), module.Listen.MakeGeneric(eventType));
                        InjectEvent(GetMethod(td, module, module.OnDisable), module.Remove.MakeGeneric(eventType));
                        modified = true;
                    }
                }
            }

            if (modified)
            {
                var method = new MethodDefinition(Weaver.MED_T1, MethodAttributes.Private, module.Import(typeof(void)));
                var worker = method.Body.GetILProcessor();
                worker.Emit(OpCodes.Ret);
                td.Methods.Add(method);
            }

            return modified;
        }

        private static void InjectField(MethodDefinition md, MethodReference method, FieldDefinition field)
        {
            var worker = md.Body.GetILProcessor();
            var target = md.Body.Instructions[0];
            worker.InsertBefore(target, worker.Create(OpCodes.Ldarg_0));
            worker.InsertBefore(target, worker.Create(OpCodes.Ldarg_0));
            worker.InsertBefore(target, worker.Create(OpCodes.Ldarg_0));
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

    internal static class ModuleGenerator
    {
        public static bool Processed(AssemblyDefinition assembly, TypeDefinition td, Module module, ILogPostProcessor Log)
        {
            if (td.Methods.Any(m => m.Name == Weaver.MED_T1))
            {
                return false;
            }

            var verified = Common.GetMethod(td, assembly, "get_owner");
            var modified = false;
            foreach (var f in td.Fields)
            {
                if (f.HasAttribute<InjectAttribute>())
                {
                    InjectField(GetMethod(td, module, module.Dequeue), module.Inject.MakeGeneric(f.FieldType), f, verified);
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
                var method = new MethodDefinition(Weaver.MED_T1, MethodAttributes.Private, module.Import(typeof(void)));
                var worker = method.Body.GetILProcessor();
                worker.Emit(OpCodes.Ret);
                td.Methods.Add(method);
            }

            return modified;
        }

        private static void InjectField(MethodDefinition md, MethodReference method, FieldDefinition field, MethodReference owner)
        {
            var worker = md.Body.GetILProcessor();
            var target = md.Body.Instructions[0];
            worker.InsertBefore(target, worker.Create(OpCodes.Ldarg_0));
            worker.InsertBefore(target, worker.Create(OpCodes.Ldarg_0));
            worker.InsertBefore(target, worker.Create(OpCodes.Call, owner));
            worker.InsertBefore(target, worker.Create(OpCodes.Ldarg_0));
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