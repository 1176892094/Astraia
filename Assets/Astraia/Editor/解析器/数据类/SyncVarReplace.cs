// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-09-06 23:09:57
// # Recently: 2026-09-06 23:19:57
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Astraia
{
    internal static class SyncVarReplace
    {
        public static void Process(ModuleDefinition md, SyncVarAccess access)
        {
            foreach (var td in md.Types.Where(td => td.IsClass))
            {
                ProcessClass(td, access);
            }
        }

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

        private static void ProcessMethod(MethodDefinition md, SyncVarAccess access)
        {
            if (md.Name == ".cctor" || md.Name == Weaver.MED_T1 || md.Name.StartsWith(Weaver.MED_V2))
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

        private static void ProcessSetter(MethodDefinition md, Instruction i, FieldDefinition opcode, SyncVarAccess access)
        {
            if (md.Name == Weaver.MED_C1)
            {
                return;
            }

            if (access.setter.TryGetValue(opcode, out var method))
            {
                i.OpCode = OpCodes.Call;
                i.Operand = method;
            }
        }

        private static void ProcessGetter(MethodDefinition md, Instruction i, FieldDefinition opcode, SyncVarAccess access)
        {
            if (md.Name == Weaver.MED_C1)
            {
                return;
            }

            if (access.getter.TryGetValue(opcode, out var method))
            {
                i.OpCode = OpCodes.Call;
                i.Operand = method;
            }
        }

        private static int ProcessAddress(MethodDefinition md, Instruction instr, FieldDefinition opcode, SyncVarAccess access, int index)
        {
            if (md.Name == Weaver.MED_C1)
            {
                return 1;
            }

            if (access.setter.TryGetValue(opcode, out var method))
            {
                var next = md.Body.Instructions[index + 1];

                if (next.OpCode == OpCodes.Initobj)
                {
                    var worker = md.Body.GetILProcessor();
                    var define = new VariableDefinition(opcode.FieldType);
                    md.Body.Variables.Add(define);

                    worker.InsertBefore(instr, worker.Create(OpCodes.Ldloca, define));
                    worker.InsertBefore(instr, worker.Create(OpCodes.Initobj, opcode.FieldType));
                    worker.InsertBefore(instr, worker.Create(OpCodes.Ldloc, define));
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