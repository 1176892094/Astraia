// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-14 22:08:19
// # Recently: 2026-09-06 15:32:39
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Linq;
using System.Collections.Generic;
using Astraia.Editor;
using Mono.Cecil;
using Astraia.Net;
using UnityEngine;

namespace Astraia
{
    using TA = TypeAttributes;
    using MA = MethodAttributes;

    internal static class Weaver
    {
        public const string WEAVER = "Astraia.Net";
        public const string MED_V1 = "V1";
        public const string MED_V2 = "V2";
        public const string MED_C1 = ".ctor";
        public const string MED_C2 = ".cctor";
        public const string MED_S1 = "SerializeSyncVars";
        public const string MED_S2 = "DeserializeSyncVars";
        public const string MED_T2 = nameof(EntityProcess);
        public const string MED_T1 = nameof(AssemblyProcessor);
        public const MA GEN_V1 = MA.HideBySig | MA.Family | MA.Static;
        public const MA GEN_V2 = MA.HideBySig | MA.Public | MA.Static;
        public const MA GEN_S1 = MA.HideBySig | MA.Public | MA.Virtual;
        public const MA GEN_S2 = MA.HideBySig | MA.Family | MA.Virtual;
        public const MA GEN_S3 = MA.HideBySig | MA.Public | MA.SpecialName;
        public const MA GEN_C2 = MA.HideBySig | MA.Static | MA.SpecialName | MA.Private | MA.RTSpecialName;
        public const TA GEN_T1 = TA.AutoClass | TA.Public | TA.Class | TA.AnsiClass | TA.Abstract | TA.Sealed | TA.BeforeFieldInit;

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
                TypeDefinition create = null;

                if (verified)
                {
                    if (assembly.MainModule.Types.Any(td => td.Namespace == WEAVER && td.Name == MED_T1))
                    {
                        verified = false;
                    }
                    else
                    {
                        access = new SyncVarAccess();
                        create = new TypeDefinition(WEAVER, MED_T1, GEN_T1, module.Import<object>());
                        writer = new Writer(assembly, module, create, debugger);
                        reader = new Reader(assembly, module, create, debugger);
                        change = StreamProcess.Process(assembly, resolver, debugger, writer, reader, ref failed);
                    }
                }

                var mainModule = assembly.MainModule;
                foreach (var td in mainModule.Types)
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
                        change |= EntityProcess.Processed(assembly, td, module, debugger);
                    }
                }

                if (failed)
                {
                    return false;
                }

                if (verified && change)
                {
                    SyncVarReplace.Process(mainModule, access);
                    mainModule.Types.Add(create);
                    StreamProcess.Processed(assembly, module, writer, reader, create);
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
}