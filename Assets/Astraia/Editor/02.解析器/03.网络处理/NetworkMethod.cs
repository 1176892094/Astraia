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

    internal static class NetworkMethod
    {
        public static MethodDefinition ClientRpcV1(Module module, Writer writer, ILogPostProcessor Log, TypeDefinition expand, MethodDefinition method, CustomAttribute args, ref bool failed)
        {
            var result = InvokeV1(Log, expand, method, ref failed);
            var worker = method.Body.GetILProcessor();
            NetworkMember.WriterDequeue(worker, module);
            if (!ArgumentWriter(worker, writer, Log, method, InvokeMode.ClientRpc, ref failed))
            {
                return null;
            }

            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Ldstr, method.FullName);
            worker.Emit(OpCodes.Ldc_I4, (int)NetworkMessage.Id(method.FullName));
            worker.Emit(OpCodes.Ldloc_0);
            worker.Emit(OpCodes.Ldc_I4, (int)args.GetArgument());
            worker.Emit(OpCodes.Callvirt, module.SendClientRpcInternal);
            NetworkMember.WriterEnqueue(worker, module);
            worker.Emit(OpCodes.Ret);
            return result;
        }

        public static MethodDefinition ClientRpcV2(Module module, Reader reader, ILogPostProcessor Log, TypeDefinition expand, MethodDefinition method, MethodDefinition func, ref bool failed)
        {
            var result = new MethodDefinition(method.GetName(Weaver.MED_INV), Weaver.GEN_RPC, module.Import(typeof(void)));
            var worker = result.Body.GetILProcessor();
            IsClientActive(worker, module, method.Name, nameof(InvokeMode.ClientRpc));

            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Castclass, expand);

            if (!ArgumentReader(worker, reader, Log, method, InvokeMode.ClientRpc, ref failed))
            {
                return null;
            }

            worker.Emit(OpCodes.Callvirt, func);
            worker.Emit(OpCodes.Ret);
            NetworkMember.AddParameters(module, result.Parameters);
            expand.Methods.Add(result);
            return result;
        }

        public static MethodDefinition ServerRpcV1(Module module, Writer writer, ILogPostProcessor Log, TypeDefinition expand, MethodDefinition method, CustomAttribute args, ref bool failed)
        {
            var result = InvokeV1(Log, expand, method, ref failed);
            var worker = method.Body.GetILProcessor();
            NetworkMember.WriterDequeue(worker, module);
            if (!ArgumentWriter(worker, writer, Log, method, InvokeMode.ServerRpc, ref failed))
            {
                return null;
            }

            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Ldstr, method.FullName);
            worker.Emit(OpCodes.Ldc_I4, (int)NetworkMessage.Id(method.FullName));
            worker.Emit(OpCodes.Ldloc_0);
            worker.Emit(OpCodes.Ldc_I4, (int)args.GetArgument());
            worker.Emit(OpCodes.Call, module.SendServerRpcInternal);
            NetworkMember.WriterEnqueue(worker, module);
            worker.Emit(OpCodes.Ret);
            return result;
        }

        public static MethodDefinition ServerRpcV2(Module module, Reader reader, ILogPostProcessor Log, TypeDefinition expand, MethodDefinition method, MethodDefinition func, ref bool failed)
        {
            var result = new MethodDefinition(method.GetName(Weaver.MED_INV), Weaver.GEN_RPC, module.Import(typeof(void)));
            var worker = result.Body.GetILProcessor();
            IsServerActive(worker, module, method.Name, nameof(InvokeMode.ServerRpc));

            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Castclass, expand);

            if (!ArgumentReader(worker, reader, Log, method, InvokeMode.ServerRpc, ref failed))
            {
                return null;
            }

            worker.Emit(OpCodes.Callvirt, func);
            worker.Emit(OpCodes.Ret);
            NetworkMember.AddParameters(module, result.Parameters);
            expand.Methods.Add(result);
            return result;
        }

        public static MethodDefinition TargetRpcV1(Module module, Writer writer, ILogPostProcessor Log, TypeDefinition expand, MethodDefinition method, CustomAttribute args, ref bool failed)
        {
            var result = InvokeV1(Log, expand, method, ref failed);
            var worker = method.Body.GetILProcessor();
            NetworkMember.WriterDequeue(worker, module);
            if (!ArgumentWriter(worker, writer, Log, method, InvokeMode.TargetRpc, ref failed))
            {
                return null;
            }

            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(IsNetworkClient(method) ? OpCodes.Ldarg_1 : OpCodes.Ldnull);
            worker.Emit(OpCodes.Ldstr, method.FullName);
            worker.Emit(OpCodes.Ldc_I4, (int)NetworkMessage.Id(method.FullName));
            worker.Emit(OpCodes.Ldloc_0);
            worker.Emit(OpCodes.Ldc_I4, (int)args.GetArgument());
            worker.Emit(OpCodes.Callvirt, module.SendTargetRpcInternal);
            NetworkMember.WriterEnqueue(worker, module);
            worker.Emit(OpCodes.Ret);
            return result;
        }

        public static MethodDefinition TargetRpcV2(Module module, Reader reader, ILogPostProcessor Log, TypeDefinition expand, MethodDefinition method, MethodDefinition func, ref bool failed)
        {
            var result = new MethodDefinition(method.GetName(Weaver.MED_INV), Weaver.GEN_RPC, module.Import(typeof(void)));
            var worker = result.Body.GetILProcessor();
            IsClientActive(worker, module, method.Name, nameof(InvokeMode.TargetRpc));

            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Castclass, expand);

            if (IsNetworkClient(method))
            {
                worker.Emit(OpCodes.Ldnull);
            }

            if (!ArgumentReader(worker, reader, Log, method, InvokeMode.TargetRpc, ref failed))
            {
                return null;
            }

            worker.Emit(OpCodes.Callvirt, func);
            worker.Emit(OpCodes.Ret);
            NetworkMember.AddParameters(module, result.Parameters);
            expand.Methods.Add(result);
            return result;
        }

        private static MethodDefinition InvokeV1(ILogPostProcessor debugger, TypeDefinition expand, MethodDefinition method, ref bool failed)
        {
            var md = new MethodDefinition(method.GetName(Weaver.MED_RPC), method.Attributes, method.ReturnType)
            {
                IsPublic = false,
                IsFamily = true
            };

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
            expand.Methods.Add(md);
            InvokeV2(debugger, expand, md, ref failed);
            return md;
        }

        private static void InvokeV2(ILogPostProcessor debugger, TypeDefinition expand, MethodDefinition method, ref bool failed)
        {
            var fullName = method.Name;
            if (fullName.EndsWith(Weaver.MED_RPC))
            {
                var name = method.Name.Substring(Weaver.MED_RPC.Length);

                foreach (var instruction in method.Body.Instructions)
                {
                    if (instruction.OpCode == OpCodes.Call && instruction.Operand is MethodDefinition result)
                    {
                        if (result.GetName(string.Empty) == name)
                        {
                            var md = expand.BaseType.Resolve().GetBaseMethod(fullName);
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

        private static bool ArgumentWriter(ILProcessor worker, Writer writer, ILogPostProcessor Log, MethodDefinition method, InvokeMode mode, ref bool failed)
        {
            var counter = 1;
            var skipped = mode == InvokeMode.TargetRpc && IsNetworkClient(method);
            foreach (var pd in method.Parameters)
            {
                if (counter == 1 && skipped)
                {
                    counter += 1;
                    continue;
                }

                var func = writer.GetFunction(pd.ParameterType, ref failed);
                if (func == null)
                {
                    Log.Error("{0} 有无效的参数 {1}。不支持类型 {2}。".Format(method.Name, pd, pd.ParameterType), method);
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

        private static bool ArgumentReader(ILProcessor worker, Reader reader, ILogPostProcessor Log, MethodDefinition method, InvokeMode mode, ref bool failed)
        {
            var counter = 1;
            var skipped = mode == InvokeMode.TargetRpc && IsNetworkClient(method);
            foreach (var pd in method.Parameters)
            {
                if (counter == 1 && skipped)
                {
                    counter += 1;
                    continue;
                }

                var func = reader.GetFunction(pd.ParameterType, ref failed);
                if (func == null)
                {
                    Log.Error("{0} 有无效的参数 {1}。不支持类型 {2}。".Format(method.Name, pd, pd.ParameterType), method);
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