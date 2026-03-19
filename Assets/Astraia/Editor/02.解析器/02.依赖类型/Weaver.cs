// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-19 03:12:36
// # Recently: 2024-12-22 20:12:33
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Astraia.Net;
using Mono.Cecil;
using Member = Mono.Cecil.TypeAttributes;
using Method = Mono.Cecil.MethodAttributes;

namespace Astraia.Editor
{
    internal class Weaver
    {
        public const int SYNC_LIMIT = 64;
        public const string CTOR = ".ctor";
        public const string GEN_TYPE = "Astraia.Net";
        public const string GEN_SKIP = "ILPP_IGNORE";
        public const string GEN_FUNC = "NetworkProcessor";
        public const string INV_METHOD = "_0";
        public const string RPC_METHOD = "_1";
        public const string SER_METHOD = "SerializeSyncVars";
        public const string DES_METHOD = "DeserializeSyncVars";
        public const Method GEN_RPC = Method.HideBySig | Method.Family | Method.Static;
        public const Method GEN_RAW = Method.HideBySig | Method.Public | Method.Static;
        public const Method GEN_VAR = Method.HideBySig | Method.Public | Method.Virtual;
        public const Method GEN_SYNC = Method.HideBySig | Method.Public | Method.SpecialName;
        public const Method GEN_CTOR = Method.HideBySig | Method.Static | Method.SpecialName | Method.Private | Method.RTSpecialName;
        public const Member GEN_ATTR = Member.AutoClass | Member.Public | Member.Class | Member.AnsiClass | Member.Abstract | Member.Sealed | Member.BeforeFieldInit;


        private bool failed;
        private Module module;
        private Writer writer;
        private Reader reader;
        private SyncVarAccess access;
        private TypeDefinition process;
        private AssemblyDefinition assembly;
        private readonly ILogPostProcessor log;

        public Weaver(ILogPostProcessor log)
        {
            this.log = log;
        }

        public bool Weave(AssemblyDefinition assembly, IAssemblyResolver resolver, out bool modified)
        {
            failed = false;
            modified = false;
            try
            {
                this.assembly = assembly;

                if (assembly.MainModule.GetTypes().Any(type => type.Namespace == GEN_TYPE && type.Name == nameof(NetworkProcessor)))
                {
                    return true;
                }

                access = new SyncVarAccess();
                module = new Module(assembly, log, ref failed);
                process = new TypeDefinition(GEN_TYPE, nameof(NetworkProcessor), GEN_ATTR, module.Import<object>());
                writer = new Writer(assembly, module, process, log);
                reader = new Reader(assembly, module, process, log);
                modified = RuntimeAttribute.Process(assembly, resolver, log, writer, reader, ref failed);

                var mainModule = assembly.MainModule;

                modified |= ProcessModule(mainModule);
                if (failed)
                {
                    return false;
                }

                if (modified)
                {
                    SyncVarReplace.Process(mainModule, access);
                    mainModule.Types.Add(process);
                    RuntimeAttribute.RuntimeInitializeOnLoad(assembly, module, writer, reader, process);
                }

                return true;
            }
            catch (Exception e)
            {
                failed = true;
                log.Error(e.ToString());
                return false;
            }
        }

        /// <summary>
        ///     处理 NetworkModule
        /// </summary>
        /// <param name="td"></param>
        /// <param name="failed"></param>
        /// <returns></returns>
        private bool ProcessNetworkModule(TypeDefinition td, ref bool failed)
        {
            if (!td.IsClass)
            {
                return false;
            }

            if (!td.IsDerivedFrom<NetworkModule>())
            {
                return false;
            }

            var modules = new List<TypeDefinition>();

            var parent = td;
            while (parent != null)
            {
                if (parent.Is<NetworkModule>())
                {
                    break;
                }

                try
                {
                    modules.Insert(0, parent);
                    parent = parent.BaseType.Resolve();
                }
                catch (AssemblyResolutionException)
                {
                    break;
                }
            }

            var changed = false;
            foreach (var m in modules)
            {
                changed |= new NetworkModuleProcess(assembly, access, module, writer, reader, log, m).Process(ref failed);
            }

            return changed;
        }

        /// <summary>
        ///     处理功能
        /// </summary>
        /// <param name="md"></param>
        /// <returns></returns>
        private bool ProcessModule(ModuleDefinition md)
        {
            var result = false;
            foreach (var td in md.Types)
            {
                if (td.IsClass && td.BaseType.CanResolve())
                {
                    result |= ProcessNetworkModule(td, ref failed);
                }
            }

            return result;
        }

        /// <summary>
        ///     处理方法中的参数
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="md"></param>
        /// <returns></returns>
        public static string GenerateMethodName(string prefix, MethodDefinition md)
        {
            return md.Name + prefix;
        }
    }
}