// *********************************************************************************
// # Project: Forest
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-12 15:01:52
// # Recently: 2025-01-12 15:01:52
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using Astraia.Net;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Astraia.Editor
{
    internal enum InvokeMode : byte
    {
        ServerRpc,
        ClientRpc,
        TargetRpc,
    }

    internal static class NetworkMethodGen
    {
        public static MethodDefinition ClientRpcV1(Module module, Writer writer, ILogPostProcessor Log, TypeDefinition create, MethodDefinition method, CustomAttribute args, ref bool failed)
        {
            var result = InvokeV1(Log, create, method, ref failed);
            var worker = method.Body.GetILProcessor();
            NetworkModuleGen.WriterDequeue(worker, module);
            if (ArgumentWriter(worker, writer, Log, method, InvokeMode.ClientRpc, ref failed))
            {
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldstr, method.FullName);
                worker.Emit(OpCodes.Ldc_I4, (int)NetworkMessage.Id(method.FullName));
                worker.Emit(OpCodes.Ldloc_0);
                worker.Emit(OpCodes.Ldc_I4, args.GetArgument<int>());
                worker.Emit(OpCodes.Callvirt, module.SendClientRpcInternal);
                NetworkModuleGen.WriterEnqueue(worker, module);
                worker.Emit(OpCodes.Ret);
                return result;
            }

            return null;
        }

        public static MethodDefinition ClientRpcV2(Module module, Reader reader, ILogPostProcessor Log, TypeDefinition create, MethodDefinition method, MethodDefinition func, ref bool failed)
        {
            var result = new MethodDefinition(method.GetName(Weaver.MED_V2), Weaver.GEN_V1, module.Import(typeof(void)));
            var worker = result.Body.GetILProcessor();
            IsClientActive(worker, module, method.Name, nameof(InvokeMode.ClientRpc));

            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Castclass, create);

            if (ArgumentReader(worker, reader, Log, method, InvokeMode.ClientRpc, ref failed))
            {
                worker.Emit(OpCodes.Callvirt, func);
                worker.Emit(OpCodes.Ret);
                NetworkModuleGen.AddParameters(module, result.Parameters);
                create.Methods.Add(result);
                return result;
            }

            return null;
        }

        public static MethodDefinition ServerRpcV1(Module module, Writer writer, ILogPostProcessor Log, TypeDefinition create, MethodDefinition method, CustomAttribute args, ref bool failed)
        {
            var result = InvokeV1(Log, create, method, ref failed);
            var worker = method.Body.GetILProcessor();
            NetworkModuleGen.WriterDequeue(worker, module);
            if (ArgumentWriter(worker, writer, Log, method, InvokeMode.ServerRpc, ref failed))
            {
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldstr, method.FullName);
                worker.Emit(OpCodes.Ldc_I4, (int)NetworkMessage.Id(method.FullName));
                worker.Emit(OpCodes.Ldloc_0);
                worker.Emit(OpCodes.Ldc_I4, args.GetArgument<int>());
                worker.Emit(OpCodes.Call, module.SendServerRpcInternal);
                NetworkModuleGen.WriterEnqueue(worker, module);
                worker.Emit(OpCodes.Ret);
                return result;
            }

            return null;
        }

        public static MethodDefinition ServerRpcV2(Module module, Reader reader, ILogPostProcessor Log, TypeDefinition create, MethodDefinition method, MethodDefinition func, ref bool failed)
        {
            var result = new MethodDefinition(method.GetName(Weaver.MED_V2), Weaver.GEN_V1, module.Import(typeof(void)));
            var worker = result.Body.GetILProcessor();
            IsServerActive(worker, module, method.Name, nameof(InvokeMode.ServerRpc));

            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Castclass, create);

            if (ArgumentReader(worker, reader, Log, method, InvokeMode.ServerRpc, ref failed))
            {
                worker.Emit(OpCodes.Callvirt, func);
                worker.Emit(OpCodes.Ret);
                NetworkModuleGen.AddParameters(module, result.Parameters);
                create.Methods.Add(result);
                return result;
            }

            return null;
        }

        public static MethodDefinition TargetRpcV1(Module module, Writer writer, ILogPostProcessor Log, TypeDefinition create, MethodDefinition method, CustomAttribute args, ref bool failed)
        {
            var result = InvokeV1(Log, create, method, ref failed);
            var worker = method.Body.GetILProcessor();
            NetworkModuleGen.WriterDequeue(worker, module);
            if (ArgumentWriter(worker, writer, Log, method, InvokeMode.TargetRpc, ref failed))
            {
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(IsNetworkClient(method) ? OpCodes.Ldarg_1 : OpCodes.Ldnull);
                worker.Emit(OpCodes.Ldstr, method.FullName);
                worker.Emit(OpCodes.Ldc_I4, (int)NetworkMessage.Id(method.FullName));
                worker.Emit(OpCodes.Ldloc_0);
                worker.Emit(OpCodes.Ldc_I4, args.GetArgument<int>());
                worker.Emit(OpCodes.Callvirt, module.SendTargetRpcInternal);
                NetworkModuleGen.WriterEnqueue(worker, module);
                worker.Emit(OpCodes.Ret);
                return result;
            }

            return null;
        }

        public static MethodDefinition TargetRpcV2(Module module, Reader reader, ILogPostProcessor Log, TypeDefinition create, MethodDefinition method, MethodDefinition func, ref bool failed)
        {
            var result = new MethodDefinition(method.GetName(Weaver.MED_V2), Weaver.GEN_V1, module.Import(typeof(void)));
            var worker = result.Body.GetILProcessor();
            IsClientActive(worker, module, method.Name, nameof(InvokeMode.TargetRpc));

            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Castclass, create);

            if (IsNetworkClient(method))
            {
                worker.Emit(OpCodes.Ldnull);
            }

            if (ArgumentReader(worker, reader, Log, method, InvokeMode.TargetRpc, ref failed))
            {
                worker.Emit(OpCodes.Callvirt, func);
                worker.Emit(OpCodes.Ret);
                NetworkModuleGen.AddParameters(module, result.Parameters);
                create.Methods.Add(result);
                return result;
            }

            return null;
        }

        private static MethodDefinition InvokeV1(ILogPostProcessor debugger, TypeDefinition create, MethodDefinition method, ref bool failed)
        {
            var md = new MethodDefinition(method.GetName(Weaver.MED_V1), method.Attributes, method.ReturnType);
            md.IsPublic = false;
            md.IsFamily = true;

            foreach (var pd in method.Parameters)
            {
                md.Parameters.Add(new ParameterDefinition(pd.Name, ParameterAttributes.None, pd.ParameterType));
            }

            (md.Body, method.Body) = (method.Body, md.Body);

            foreach (var point in method.DebugInformation.SequencePoints)
            {
                md.DebugInformation.SequencePoints.Add(point);
            }

            method.DebugInformation.SequencePoints.Clear();

            foreach (var info in method.CustomDebugInformations)
            {
                md.CustomDebugInformations.Add(info);
            }

            method.CustomDebugInformations.Clear();
            (method.DebugInformation.Scope, md.DebugInformation.Scope) = (md.DebugInformation.Scope, method.DebugInformation.Scope);
            create.Methods.Add(md);
            InvokeV2(debugger, create, md, ref failed);
            return md;
        }

        private static void InvokeV2(ILogPostProcessor debugger, TypeDefinition create, MethodDefinition method, ref bool failed)
        {
            var fullName = method.Name;
            if (fullName.EndsWith(Weaver.MED_V1))
            {
                var name = method.Name.Substring(Weaver.MED_V1.Length);

                foreach (var instruction in method.Body.Instructions)
                {
                    if (instruction.OpCode == OpCodes.Call && instruction.Operand is MethodDefinition result)
                    {
                        if (result.GetName(string.Empty) == name)
                        {
                            var md = create.BaseType.GetBaseMethod(fullName);
                            if (md == null)
                            {
                                debugger.Error("找不到base方法: {0}".Format(fullName), method);
                                failed = true;
                                return;
                            }

                            if (!md.IsVirtual)
                            {
                                debugger.Error("找不到virtual的方法: {0}".Format(fullName), method);
                                failed = true;
                                return;
                            }

                            instruction.Operand = md;
                        }
                    }
                }
            }
        }

        private static bool ArgumentWriter(ILProcessor worker, Writer writer, ILogPostProcessor Log, MethodDefinition md, InvokeMode mode, ref bool failed)
        {
            var counter = 1;
            var skipped = mode == InvokeMode.TargetRpc && IsNetworkClient(md);
            foreach (var pd in md.Parameters)
            {
                if (counter == 1 && skipped)
                {
                    counter += 1;
                    continue;
                }

                var func = writer.GetFunction(pd.ParameterType, ref failed);
                if (func == null)
                {
                    Log.Error("{0} 有无效的参数 {1}。不支持类型 {2}。".Format(md.Name, pd, pd.ParameterType), md);
                    failed = true;
                    return false;
                }

                worker.Emit(OpCodes.Ldloc_0);
                worker.Emit(OpCodes.Ldarg, counter);
                worker.Emit(OpCodes.Call, func);
                counter += 1;
            }

            return true;
        }

        private static bool ArgumentReader(ILProcessor worker, Reader reader, ILogPostProcessor Log, MethodDefinition md, InvokeMode mode, ref bool failed)
        {
            var counter = 1;
            var skipped = mode == InvokeMode.TargetRpc && IsNetworkClient(md);
            foreach (var pd in md.Parameters)
            {
                if (counter == 1 && skipped)
                {
                    counter += 1;
                    continue;
                }

                var func = reader.GetFunction(pd.ParameterType, ref failed);
                if (func == null)
                {
                    Log.Error("{0} 有无效的参数 {1}。不支持类型 {2}。".Format(md.Name, pd, pd.ParameterType), md);
                    failed = true;
                    return false;
                }

                worker.Emit(OpCodes.Ldarg_1);
                worker.Emit(OpCodes.Call, func);
                if (pd.ParameterType.Is<float>())
                {
                    worker.Emit(OpCodes.Conv_R4);
                }
                else if (pd.ParameterType.Is<double>())
                {
                    worker.Emit(OpCodes.Conv_R8);
                }
            }

            return true;
        }

        private static bool IsNetworkClient(MethodDefinition md)
        {
            if (md.Parameters.Count > 0)
            {
                var tr = md.Parameters[0].ParameterType;
                return tr.Is<NetworkClient>() || tr.IsSubclassOf<NetworkClient>();
            }

            return false;
        }

        private static void IsClientActive(ILProcessor worker, Module module, string name, string target)
        {
            var nop = worker.Create(OpCodes.Nop);
            worker.Emit(OpCodes.Call, module.GetClientActive);
            worker.Emit(OpCodes.Brtrue, nop);
            worker.Emit(OpCodes.Ldstr, "{0} 远程调用 {1} 方法，但是客户端不是活跃的。".Format(target, name));
            worker.Emit(OpCodes.Call, module.LogError);
            worker.Emit(OpCodes.Ret);
            worker.Append(nop);
        }

        private static void IsServerActive(ILProcessor worker, Module module, string name, string target)
        {
            var nop = worker.Create(OpCodes.Nop);
            worker.Emit(OpCodes.Call, module.GetServerActive);
            worker.Emit(OpCodes.Brtrue, nop);
            worker.Emit(OpCodes.Ldstr, "{0} 远程调用 {1} 方法，但是服务器不是活跃的。".Format(target, name));
            worker.Emit(OpCodes.Call, module.LogError);
            worker.Emit(OpCodes.Ret);
            worker.Append(nop);
        }
    }
}