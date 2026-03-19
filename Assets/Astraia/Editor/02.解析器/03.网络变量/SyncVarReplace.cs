// *********************************************************************************
// # Project: Forest
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-12 15:01:03
// # Recently: 2025-01-12 15:01:03
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Astraia.Editor
{
    internal static class SyncVarReplace
    {
        /// <summary>
        /// 修正SyncVar
        /// </summary>
        public static void Process(ModuleDefinition md, SyncVarAccess access)
        {
            foreach (var td in md.Types)
            {
                if (td.IsClass)
                {
                    ProcessClass(td, access);
                }
            }
        }

        /// <summary>
        /// 处理类
        /// </summary>
        private static void ProcessClass(TypeDefinition td, SyncVarAccess access)
        {
            foreach (var md in td.Methods)
            {
                ProcessMethod(md, access);
            }

            foreach (var nested in td.NestedTypes)
            {
                ProcessClass(nested, access);
            }
        }

        /// <summary>
        /// 处理方法
        /// </summary>
        private static void ProcessMethod(MethodDefinition md, SyncVarAccess access)
        {
            if (md.Name == ".cctor" || md.Name == Weaver.GEN_FUNC || md.Name.StartsWith(Weaver.INV_METHOD))
            {
                return;
            }

            if (md.IsAbstract)
            {
                return;
            }

            if (md.Body?.Instructions != null)
            {
                for (var i = 0; i < md.Body.Instructions.Count;)
                {
                    var instr = md.Body.Instructions[i];
                    i += ProcessInstruction(md, instr, i, access);
                }
            }
        }

        /// <summary>
        /// 处理指令
        /// </summary>
        private static int ProcessInstruction(MethodDefinition md, Instruction instr, int index, SyncVarAccess access)
        {
            if (instr.OpCode == OpCodes.Stfld && instr.Operand is FieldDefinition OpStfLd)
            {
                ProcessSetter(md, instr, OpStfLd, access);
            }

            if (instr.OpCode == OpCodes.Ldfld && instr.Operand is FieldDefinition OpLdfLd)
            {
                ProcessGetter(md, instr, OpLdfLd, access);
            }

            if (instr.OpCode == OpCodes.Ldflda && instr.Operand is FieldDefinition OpLdfLda)
            {
                return ProcessAddress(md, instr, OpLdfLda, access, index);
            }

            return 1;
        }

        /// <summary>
        /// 设置指令
        /// </summary>
        private static void ProcessSetter(MethodDefinition md, Instruction i, FieldDefinition opField, SyncVarAccess access)
        {
            if (md.Name == Weaver.CTOR)
            {
                return;
            }

            if (access.setter.TryGetValue(opField, out var method))
            {
                i.OpCode = OpCodes.Call;
                i.Operand = method;
            }
        }

        /// <summary>
        /// 获取指令
        /// </summary>
        private static void ProcessGetter(MethodDefinition md, Instruction i, FieldDefinition opField, SyncVarAccess access)
        {
            if (md.Name == Weaver.CTOR)
            {
                return;
            }

            if (access.getter.TryGetValue(opField, out var method))
            {
                i.OpCode = OpCodes.Call;
                i.Operand = method;
            }
        }

        /// <summary>
        /// 处理加载地址指令
        /// </summary>
        private static int ProcessAddress(MethodDefinition md, Instruction instr, FieldDefinition opField, SyncVarAccess access, int index)
        {
            if (md.Name == Weaver.CTOR)
            {
                return 1;
            }

            if (access.setter.TryGetValue(opField, out var method))
            {
                var next = md.Body.Instructions[index + 1];

                if (next.OpCode == OpCodes.Initobj)
                {
                    var worker = md.Body.GetILProcessor();
                    var variable = new VariableDefinition(opField.FieldType);
                    md.Body.Variables.Add(variable);

                    worker.InsertBefore(instr, worker.Create(OpCodes.Ldloca, variable));
                    worker.InsertBefore(instr, worker.Create(OpCodes.Initobj, opField.FieldType));
                    worker.InsertBefore(instr, worker.Create(OpCodes.Ldloc, variable));
                    worker.InsertBefore(instr, worker.Create(OpCodes.Call, method));

                    worker.Remove(instr);
                    worker.Remove(next);
                    return 4;
                }
            }

            return 1;
        }
    }
}