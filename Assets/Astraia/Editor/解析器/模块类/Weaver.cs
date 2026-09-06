// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-09-06 20:09:44
// # Recently: 2026-09-06 20:33:44
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Linq;
using Mono.Cecil;
using Astraia.Net;

namespace Astraia
{
    using TA = TypeAttributes;
    using MA = MethodAttributes;

    [Serializable]
    internal static class Weaver
    {
        public const string WEAVER = "Astraia.Net";
        public const string MED_V1 = "V1";
        public const string MED_V2 = "V2";
        public const string MED_C1 = ".ctor";
        public const string MED_C2 = ".cctor";
        public const string MED_S1 = "SerializeSyncVars";
        public const string MED_S2 = "DeserializeSyncVars";
        public const string MED_T2 = "EntityProcessor";
        public const string MED_T1 = "NetworkProcessor";
        public const MA GEN_V1 = MA.HideBySig | MA.Family | MA.Static;
        public const MA GEN_V2 = MA.HideBySig | MA.Public | MA.Static;
        public const MA GEN_S1 = MA.HideBySig | MA.Public | MA.Virtual;
        public const MA GEN_S2 = MA.HideBySig | MA.Family | MA.Virtual;
        public const MA GEN_S3 = MA.HideBySig | MA.Public | MA.SpecialName;
        public const MA GEN_C2 = MA.HideBySig | MA.Static | MA.SpecialName | MA.Private | MA.RTSpecialName;
        public const TA GEN_T1 = TA.AutoClass | TA.Public | TA.Class | TA.AnsiClass | TA.Abstract | TA.Sealed | TA.BeforeFieldInit;

        public static bool Weave(AssemblyDefinition assembly, AssemblyDebugger debugger, IAssemblyResolver resolver, bool network)
        {
            try
            {
                var modify = false;
                var failed = false;
                var module = new Module(assembly, debugger, ref failed);

                Writer writer = null;
                Reader reader = null;
                SyncVarAccess access = null;
                TypeDefinition member = null;

                if (network)
                {
                    if (assembly.MainModule.Types.Any(td => td.Namespace == WEAVER && td.Name == MED_T1))
                    {
                        network = false;
                    }
                    else
                    {
                        access = new SyncVarAccess();
                        member = new TypeDefinition(WEAVER, MED_T1, GEN_T1, module.Import<object>());
                        writer = new Writer(assembly, module, member, debugger);
                        reader = new Reader(assembly, module, member, debugger);
                        modify = NetworkMemberGen.Process(assembly, resolver, debugger, writer, reader, ref failed);
                    }
                }

                foreach (var td in assembly.MainModule.Types)
                {
                    if (network)
                    {
                        if (td.IsSubclassOf<NetworkModule>())
                        {
                            var current = td;
                            while (current != null)
                            {
                                if (current.Is<NetworkModule>())
                                {
                                    break;
                                }

                                modify |= new NetworkModuleGen(assembly, access, module, writer, reader, debugger, current).Process(ref failed);
                                current = current.BaseType?.Resolve();
                            }
                        }
                    }

                    if (td.IsSubclassOf<Export>())
                    {
                        var current = td;
                        while (current != null)
                        {
                            if (current.Is<Export>())
                            {
                                break;
                            }

                            modify |= EntityGenerator.Processed(assembly, current, module, debugger);
                            current = current.BaseType?.Resolve();
                        }
                    }
                }

                if (failed)
                {
                    return false;
                }

                if (network && modify)
                {
                    SyncVarReplace.Process(assembly.MainModule, access);
                    assembly.MainModule.Types.Add(member);
                    NetworkMemberGen.Processed(assembly, module, writer, reader, member);
                }

                return modify;
            }
            catch (Exception e)
            {
                debugger.Error(e.ToString());
                return false;
            }
        }
    }
}