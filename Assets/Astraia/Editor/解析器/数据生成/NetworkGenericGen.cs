using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Astraia.Editor
{
    internal static class EntityGenerator
    {
        public static bool Processed(AssemblyDefinition assembly, TypeDefinition td, Module module, ILogPostProcessor Log)
        {
            if (td.Methods.Any(m => m.Name == Weaver.MED_T2))
            {
                return false;
            }

            var modified = false;
            foreach (var f in td.Fields)
            {
                if (f.HasAttribute<ExportAttribute>())
                {
                    var awake = module.Export.MakeGeneric(assembly.MainModule, f.FieldType);
                    td.GetMethod(assembly, Weaver.GEN_S2, "Awake").InjectField(awake, f);
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
                        var onEnable = module.Listen.MakeGeneric(assembly.MainModule, eventType);
                        var onDisable = module.Remove.MakeGeneric(assembly.MainModule, eventType);
                        td.GetMethod(assembly, Weaver.GEN_S2, "OnEnable").InjectEvent(onEnable);
                        td.GetMethod(assembly, Weaver.GEN_S2, "OnDisable").InjectEvent(onDisable);
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
                var result = td.BaseType.GetMethod(ad, name);
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
    }
}