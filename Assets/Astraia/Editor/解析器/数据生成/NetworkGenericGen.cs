using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Astraia.Editor
{
    internal static class EntityGenerator
    {
        public static bool Processed(AssemblyDefinition assembly, TypeDefinition td, Module module)
        {
            if (td.Methods.Any(m => m.Name == Weaver.MED_T2))
            {
                return false;
            }

            var modified = false;
            foreach (var f in td.Fields)
            {
                if (f.HasAttribute<InjectAttribute>())
                {
                    td.GetMethod(assembly.MainModule, Weaver.GEN_S2, "Awake").InjectField(module.Inject.MakeGeneric(assembly.MainModule, f.FieldType), f);
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
                        td.GetMethod(assembly.MainModule, Weaver.GEN_S2, "OnEnable").InjectEvent(module.Listen.MakeGeneric(assembly.MainModule, eventType));
                        td.GetMethod(assembly.MainModule, Weaver.GEN_S2, "OnDisable").InjectEvent(module.Remove.MakeGeneric(assembly.MainModule, eventType));
                        modified = true;
                    }
                }
            }

            if (modified)
            {
                var method = new MethodDefinition(Weaver.MED_T2, MethodAttributes.Private, module.Import(typeof(void)));
                var worker = method.Body.GetILProcessor();
                worker.Emit(OpCodes.Ret);
                td.Methods.Add(method);
            }

            return modified;
        }
    }

    internal static class ModuleGenerator
    {
        public static bool Processed(AssemblyDefinition assembly, TypeDefinition td, Module module, ILogPostProcessor Log)
        {
            if (td.Methods.Any(m => m.Name == Weaver.MED_T2))
            {
                return false;
            }

            if ((td.Attributes & TypeAttributes.Serializable) == 0)
            {
                td.Attributes |= TypeAttributes.Serializable;
            }

            var modified = false;
            var owner = td.GetMethod(assembly, "get_owner");
            foreach (var f in td.Fields)
            {
                if (f.HasAttribute<InjectAttribute>())
                {
                    td.GetMethod(assembly.MainModule, Weaver.GEN_S1, "Dequeue").InjectField(module.Inject.MakeGeneric(assembly.MainModule, f.FieldType), f, owner);
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
                        td.GetMethod(assembly.MainModule, Weaver.GEN_S1, "OnShow").InjectEvent(module.Listen.MakeGeneric(assembly.MainModule, eventType));
                        td.GetMethod(assembly.MainModule, Weaver.GEN_S1, "OnHide").InjectEvent(module.Remove.MakeGeneric(assembly.MainModule, eventType));
                        modified = true;
                    }
                }
            }

            if (modified)
            {
                var method = new MethodDefinition(Weaver.MED_T2, MethodAttributes.Private, module.Import(typeof(void)));
                var worker = method.Body.GetILProcessor();
                worker.Emit(OpCodes.Ret);
                td.Methods.Add(method);
            }

            return modified;
        }
    }

    internal static class CustomExtensions
    {
        public static void InjectField(this MethodDefinition md, MethodReference method, FieldDefinition field, MethodReference owner = null)
        {
            var worker = md.Body.GetILProcessor();
            var target = md.Body.Instructions[0];
            worker.InsertBefore(target, worker.Create(OpCodes.Ldarg_0));
            worker.InsertBefore(target, worker.Create(OpCodes.Ldarg_0));
            if (owner != null)
            {
                worker.InsertBefore(target, worker.Create(OpCodes.Call, owner));
            }

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

        public static MethodDefinition GetMethod(this TypeDefinition td, ModuleDefinition module, MethodAttributes attrs, string name)
        {
            var method = td.Methods.FirstOrDefault(m => m.Name == name && m.Parameters.Count == 0);
            if (method == null)
            {
                method = new MethodDefinition(name, attrs, module.ImportReference(typeof(void)));
                var result = td.BaseType.Resolve().GetBaseMethod(name);
                var worker = method.Body.GetILProcessor();
                if (result != null)
                {
                    worker.Emit(OpCodes.Ldarg_0);
                    worker.Emit(OpCodes.Call, module.ImportReference(result));
                }

                worker.Emit(OpCodes.Ret);
                td.Methods.Add(method);
            }

            return method;
        }
    }
}