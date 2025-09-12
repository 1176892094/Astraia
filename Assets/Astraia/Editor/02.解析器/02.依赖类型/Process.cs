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
using Astraia.Net;
using Mono.Cecil;

namespace Astraia.Editor
{
    internal class Process
    {
        private bool failed;
        private Module module;
        private Writer writer;
        private Reader reader;
        private SyncVarAccess access;
        private TypeDefinition process;
        private AssemblyDefinition assembly;
        private readonly ILog log;

        public Process(ILog log)
        {
            this.log = log;
        }

        public bool Execute(AssemblyDefinition assembly, IAssemblyResolver resolver, out bool change)
        {
            failed = false;
            change = false;
            try
            {
                this.assembly = assembly;

                foreach (var type in assembly.MainModule.GetTypes())
                {
                    if (type.Namespace == Const.GEN_TYPE && type.Name == Const.GEN_NAME)
                    {
                        return true;
                    }
                }

                access = new SyncVarAccess();
                module = new Module(assembly, log, ref failed);
                process = new TypeDefinition(Const.GEN_TYPE, Const.GEN_NAME, Const.GEN_ATTRS, module.Import<object>());
                writer = new Writer(assembly, module, process, log);
                reader = new Reader(assembly, module, process, log);
                change = RuntimeAttribute.Process(assembly, resolver, log, writer, reader, ref failed);

                var mainModule = assembly.MainModule;

                change |= ProcessModule(mainModule);
                if (failed)
                {
                    return false;
                }

                if (change)
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
                if (td.IsClass && td.BaseType.IsResolve())
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