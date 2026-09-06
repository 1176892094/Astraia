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
    using T = TypeAttributes;
    using M = MethodAttributes;

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
        public const M GEN_V1 = M.HideBySig | M.Family | M.Static;
        public const M GEN_V2 = M.HideBySig | M.Public | M.Static;
        public const M GEN_S1 = M.HideBySig | M.Public | M.Virtual;
        public const M GEN_S2 = M.HideBySig | M.Family | M.Virtual;
        public const M GEN_S3 = M.HideBySig | M.Public | M.SpecialName;
        public const M GEN_C2 = M.HideBySig | M.Static | M.SpecialName | M.Private | M.RTSpecialName;
        public const T GEN_T1 = T.AutoClass | T.Public | T.Class | T.AnsiClass | T.Abstract | T.Sealed | T.BeforeFieldInit;

        public static bool Weave(AssemblyDefinition assembly, AssemblyDebugger debugger, IAssemblyResolver resolver, bool verified)
        {
            try
            {
                var change = false;
                var failed = false;
                var module = new Module(assembly, debugger, ref failed);

                Writer writer = null;
                Reader reader = null;
                SyncVarAccess access = null;
                TypeDefinition member = null;

                if (verified)
                {
                    if (assembly.MainModule.Types.Any(td => td.Namespace == WEAVER && td.Name == MED_T1))
                    {
                        verified = false;
                    }
                    else
                    {
                        access = new SyncVarAccess();
                        member = new TypeDefinition(WEAVER, MED_T1, GEN_T1, module.Import<object>());
                        writer = new Writer(assembly, module, member, debugger);
                        reader = new Reader(assembly, module, member, debugger);
                        change = StreamProcess.Process(assembly, resolver, debugger, writer, reader, ref failed);
                    }
                }

                foreach (var td in assembly.MainModule.Types)
                {
                    if (verified)
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

                                change |= new ModuleProcess(assembly, access, module, writer, reader, debugger, current).Process(ref failed);
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

                            change |= ExportProcess.Processed(assembly, current, module, debugger);
                            current = current.BaseType?.Resolve();
                        }
                    }
                }

                if (failed)
                {
                    return false;
                }

                if (verified && change)
                {
                    SyncVarReplace.Process(assembly.MainModule, access);
                    assembly.MainModule.Types.Add(member);
                    StreamProcess.Processed(assembly, module, writer, reader, member);
                }

                return change;
            }
            catch (Exception e)
            {
                debugger.Error(e.ToString());
                return false;
            }
        }
    }

    internal enum InvokeMode : byte
    {
        ServerRpc,
        ClientRpc,
        TargetRpc,
    }
}