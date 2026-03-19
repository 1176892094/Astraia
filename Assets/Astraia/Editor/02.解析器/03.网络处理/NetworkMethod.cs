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

using System.Linq;
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
        public static MethodDefinition ClientRpc(Module module, Reader reader, ILogPostProcessor log, TypeDefinition td, MethodDefinition md, MethodDefinition func, ref bool failed)
        {
            var rpcName = md.GetName(Weaver.MED_INV);
            var rpc = new MethodDefinition(rpcName, Weaver.GEN_RPC, module.Import(typeof(void)));
            var worker = rpc.Body.GetILProcessor();
            var label = worker.Create(OpCodes.Nop);
            IsActiveClient(worker, module, md.Name, label, "ClientRpc");

            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Castclass, td);

            if (!ReadArguments(md, reader, log, worker, InvokeMode.ClientRpc, ref failed))
            {
                return null;
            }

            worker.Emit(OpCodes.Callvirt, func);
            worker.Emit(OpCodes.Ret);
            NetworkMember.AddInvokeParameters(module, rpc.Parameters);
            td.Methods.Add(rpc);
            return rpc;
        }

        public static MethodDefinition ClientRpcInvoke(Module module, Writer writer, ILogPostProcessor log, TypeDefinition td, MethodDefinition md, CustomAttribute ca, ref bool failed)
        {
            var rpc = BaseMethodInvoke(log, td, md, ref failed);
            var worker = md.Body.GetILProcessor();
            NetworkMember.WriteInitLocals(worker, module);
            NetworkMember.WritePopSetter(worker, module);

            if (!WriteArguments(worker, writer, log, md, InvokeMode.ClientRpc, ref failed))
            {
                return null;
            }

            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Ldstr, md.FullName);
            worker.Emit(OpCodes.Ldc_I4, (int)NetworkMessage.Id(md.FullName));
            worker.Emit(OpCodes.Ldloc_0);
            worker.Emit(OpCodes.Ldc_I4, (int)ca.GetArgument());
            worker.Emit(OpCodes.Callvirt, module.SendClientRpcInternal);
            NetworkMember.WritePushSetter(worker, module);
            worker.Emit(OpCodes.Ret);
            return rpc;
        }

        public static MethodDefinition ServerRpc(Module module, Reader reader, ILogPostProcessor log, TypeDefinition td, MethodDefinition md, MethodDefinition func, ref bool failed)
        {
            var rpcName = md.GetName(Weaver.MED_INV);
            var rpc = new MethodDefinition(rpcName, Weaver.GEN_RPC, module.Import(typeof(void)));
            var worker = rpc.Body.GetILProcessor();
            var label = worker.Create(OpCodes.Nop);
            IsActiveServer(worker, module, md.Name, label, "ServerRpc");

            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Castclass, td);

            if (!ReadArguments(md, reader, log, worker, InvokeMode.ServerRpc, ref failed))
            {
                return null;
            }

            foreach (var definition in md.Parameters)
            {
                if (IsNetworkClient(definition, InvokeMode.ServerRpc))
                {
                    worker.Emit(OpCodes.Ldarg_2);
                }
            }

            worker.Emit(OpCodes.Callvirt, func);
            worker.Emit(OpCodes.Ret);
            NetworkMember.AddInvokeParameters(module, rpc.Parameters);
            td.Methods.Add(rpc);
            return rpc;
        }

        public static MethodDefinition ServerRpcInvoke(Module module, Writer writer, ILogPostProcessor log, TypeDefinition td, MethodDefinition md, CustomAttribute ca, ref bool failed)
        {
            var rpc = BaseMethodInvoke(log, td, md, ref failed);
            var worker = md.Body.GetILProcessor();
            NetworkMember.WriteInitLocals(worker, module);
            NetworkMember.WritePopSetter(worker, module);

            if (!WriteArguments(worker, writer, log, md, InvokeMode.ServerRpc, ref failed))
            {
                return null;
            }

            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Ldstr, md.FullName);
            worker.Emit(OpCodes.Ldc_I4, (int)NetworkMessage.Id(md.FullName));
            worker.Emit(OpCodes.Ldloc_0);
            worker.Emit(OpCodes.Ldc_I4, (int)ca.GetArgument());
            worker.Emit(OpCodes.Call, module.SendServerRpcInternal);
            NetworkMember.WritePushSetter(worker, module);
            worker.Emit(OpCodes.Ret);

            return rpc;
        }

        public static MethodDefinition TargetRpc(Module module, Reader reader, ILogPostProcessor log, TypeDefinition td, MethodDefinition md, MethodDefinition func, ref bool failed)
        {
            var rpcName = md.GetName(Weaver.MED_INV);
            var rpc = new MethodDefinition(rpcName, Weaver.GEN_RPC, module.Import(typeof(void)));
            var worker = rpc.Body.GetILProcessor();
            var label = worker.Create(OpCodes.Nop);
            IsActiveClient(worker, module, md.Name, label, "TargetRpc");

            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Castclass, td);

            if (HasNetworkClient(md))
            {
                worker.Emit(OpCodes.Ldnull);
            }

            if (!ReadArguments(md, reader, log, worker, InvokeMode.TargetRpc, ref failed))
            {
                return null;
            }

            worker.Emit(OpCodes.Callvirt, func);
            worker.Emit(OpCodes.Ret);
            NetworkMember.AddInvokeParameters(module, rpc.Parameters);
            td.Methods.Add(rpc);
            return rpc;
        }

        public static MethodDefinition TargetRpcInvoke(Module module, Writer writer, ILogPostProcessor log, TypeDefinition td, MethodDefinition md, CustomAttribute ca, ref bool failed)
        {
            var rpc = BaseMethodInvoke(log, td, md, ref failed);
            var worker = md.Body.GetILProcessor();
            NetworkMember.WriteInitLocals(worker, module);
            NetworkMember.WritePopSetter(worker, module);

            if (!WriteArguments(worker, writer, log, md, InvokeMode.TargetRpc, ref failed))
            {
                return null;
            }

            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(HasNetworkClient(md) ? OpCodes.Ldarg_1 : OpCodes.Ldnull);
            worker.Emit(OpCodes.Ldstr, md.FullName);
            worker.Emit(OpCodes.Ldc_I4, (int)NetworkMessage.Id(md.FullName));
            worker.Emit(OpCodes.Ldloc_0);
            worker.Emit(OpCodes.Ldc_I4, (int)ca.GetArgument());
            worker.Emit(OpCodes.Callvirt, module.SendTargetRpcInternal);
            NetworkMember.WritePushSetter(worker, module);
            worker.Emit(OpCodes.Ret);
            return rpc;
        }

        private static bool WriteArguments(ILProcessor worker, Writer writer, ILogPostProcessor log, MethodDefinition method, InvokeMode mode, ref bool failed)
        {
            var skipFirst = mode == InvokeMode.TargetRpc && HasNetworkClient(method);
            var argument = 1;
            foreach (var pd in method.Parameters)
            {
                if (argument == 1 && skipFirst)
                {
                    argument += 1;
                    continue;
                }

                if (IsNetworkClient(pd, mode))
                {
                    argument += 1;
                    continue;
                }

                var cached = writer.GetFunction(pd.ParameterType, ref failed);
                if (cached == null)
                {
                    log.Error("{0} 有无效的参数 {1}。不支持类型 {2}。".Format(method.Name, pd, pd.ParameterType), method);
                    failed = true;
                    return false;
                }

                worker.Emit(OpCodes.Ldloc_0);
                worker.Emit(OpCodes.Ldarg, argument);
                worker.Emit(OpCodes.Call, cached);
                argument += 1;
            }

            return true;
        }

        private static bool ReadArguments(MethodDefinition method, Reader reader, ILogPostProcessor log, ILProcessor worker, InvokeMode mode, ref bool failed)
        {
            var skipFirst = mode == InvokeMode.TargetRpc && HasNetworkClient(method);
            var argument = 1;
            foreach (var pd in method.Parameters)
            {
                if (argument == 1 && skipFirst)
                {
                    argument += 1;
                    continue;
                }

                if (IsNetworkClient(pd, mode))
                {
                    argument += 1;
                    continue;
                }

                var cached = reader.GetFunction(pd.ParameterType, ref failed);

                if (cached == null)
                {
                    log.Error("{0} 有无效的参数 {1}。不支持类型 {2}。".Format(method.Name, pd, pd.ParameterType), method);
                    failed = true;
                    return false;
                }

                worker.Emit(OpCodes.Ldarg_1);
                worker.Emit(OpCodes.Call, cached);

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

        private static bool HasNetworkClient(MethodDefinition md)
        {
            if (md.Parameters.Count <= 0)
            {
                return false;
            }

            var tr = md.Parameters[0].ParameterType;
            return tr.Is<NetworkClient>() || tr.IsSubclassOf<NetworkClient>();
        }

        public static bool IsNetworkClient(ParameterDefinition pd, InvokeMode mode)
        {
            if (mode != InvokeMode.ServerRpc)
            {
                return false;
            }

            var tr = pd.ParameterType;
            return tr.Is<NetworkClient>() || tr.Resolve().IsSubclassOf<NetworkClient>();
        }

        private static void IsActiveClient(ILProcessor worker, Module module, string mdName, Instruction label, string error)
        {
            worker.Emit(OpCodes.Call, module.GetClientActive);
            worker.Emit(OpCodes.Brtrue, label);
            worker.Emit(OpCodes.Ldstr, "{0} 远程调用 {1} 方法，但是客户端不是活跃的。".Format(error, mdName));
            worker.Emit(OpCodes.Call, module.LogError);
            worker.Emit(OpCodes.Ret);
            worker.Append(label);
        }

        private static void IsActiveServer(ILProcessor worker, Module module, string mdName, Instruction label, string error)
        {
            worker.Emit(OpCodes.Call, module.GetServerActive);
            worker.Emit(OpCodes.Brtrue, label);
            worker.Emit(OpCodes.Ldstr, "{0} 远程调用 {1} 方法，但是服务器不是活跃的。".Format(error, mdName));
            worker.Emit(OpCodes.Call, module.LogError);
            worker.Emit(OpCodes.Ret);
            worker.Append(label);
        }

        private static void BaseMethod(ILogPostProcessor log, TypeDefinition td, MethodDefinition md, ref bool failed)
        {
            var fullName = md.Name;
            if (!fullName.StartsWith(Weaver.MED_RPC))
            {
                return;
            }

            var name = md.Name.Substring(Weaver.MED_RPC.Length);

            foreach (var instruction in md.Body.Instructions)
            {
                if (CanInvoke(instruction, out var method))
                {
                    var newName = method.GetName(string.Empty);
                    if (newName == name)
                    {
                        var baseType = td.BaseType.Resolve();
                        var baseMethod = baseType.GetMethod(fullName);

                        if (baseMethod == null)
                        {
                            log.Error("找不到base方法: {0}".Format(fullName), md);
                            failed = true;
                            return;
                        }

                        if (!baseMethod.IsVirtual)
                        {
                            log.Error("找不到virtual的方法: {0}".Format(fullName), md);
                            failed = true;
                            return;
                        }

                        instruction.Operand = baseMethod;
                    }
                }
            }
        }

        private static MethodDefinition BaseMethodInvoke(ILogPostProcessor log, TypeDefinition td, MethodDefinition md, ref bool failed)
        {
            var newName = md.GetName(Weaver.MED_RPC);
            var method = new MethodDefinition(newName, md.Attributes, md.ReturnType)
            {
                IsPublic = false,
                IsFamily = true
            };

            foreach (var pd in md.Parameters)
            {
                method.Parameters.Add(new ParameterDefinition(pd.Name, ParameterAttributes.None, pd.ParameterType));
            }

            (method.Body, md.Body) = (md.Body, method.Body);

            foreach (var point in md.DebugInformation.SequencePoints)
            {
                method.DebugInformation.SequencePoints.Add(point);
            }

            md.DebugInformation.SequencePoints.Clear();

            foreach (var info in md.CustomDebugInformations)
            {
                method.CustomDebugInformations.Add(info);
            }

            md.CustomDebugInformations.Clear();
            (md.DebugInformation.Scope, method.DebugInformation.Scope) = (method.DebugInformation.Scope, md.DebugInformation.Scope);
            td.Methods.Add(method);
            BaseMethod(log, td, method, ref failed);
            return method;
        }

        private static bool CanInvoke(Instruction instr, out MethodDefinition md)
        {
            if (instr.OpCode == OpCodes.Call && instr.Operand is MethodDefinition method)
            {
                md = method;
                return true;
            }

            md = null;
            return false;
        }
    }
}