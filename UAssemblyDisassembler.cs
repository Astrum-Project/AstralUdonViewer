// Modified version of `VRC.Udon.Compiler.dll` version `1.0.0.0`

using System;
using System.Collections.Generic;
using VRC.Udon.Common.Interfaces;
using VRC.Udon.VM.Common;

namespace VRC.Udon.UAssembly.Disassembler
{
    public class UAssemblyDisassembler
    {
        public static string[] DisassembleProgram(IUdonProgram program)
        {
            List<string> list = new List<string>();
            for (uint num = 0; num < program.ByteCode.Length;)
                list.Add(DisassembleInstruction(program, ref num));
            return list.ToArray();
        }

        public static string DisassembleInstruction(IUdonProgram program, ref uint offset)
        {
            OpCode opCode = (OpCode)UIntFromBytes(program.ByteCode, offset);
            if (opCode == OpCode.NOP)
                return SimpleInstruction(ref offset, "NOP");
            else if (opCode == OpCode.PUSH)
                return DirectInstruction(ref offset, "PUSH", program);
            else if (opCode == OpCode.POP)
                return SimpleInstruction(ref offset, "POP");
            else if (opCode == OpCode.JUMP_IF_FALSE)
                return DirectInstruction(ref offset, "JUMP_IF_FALSE", program);
            else if (opCode == OpCode.JUMP)
                return DirectInstruction(ref offset, "JUMP", program);
            else if (opCode == OpCode.EXTERN)
                return ExternInstruction(ref offset, "EXTERN", program);
            else if (opCode == OpCode.ANNOTATION)
                return AnnotationInstruction(ref offset, "ANNOTATION", program);
            else if (opCode == OpCode.JUMP_INDIRECT)
                return JumpIndirectInstruction(ref offset, "JUMP_INDIRECT", program);
            else if (opCode == OpCode.COPY)
                return SimpleInstruction(ref offset, "COPY");
            else return $"0x{(offset += 4) - 4:X}: INVALID (0x{opCode:X})";
        }

        private static string SimpleInstruction(ref uint offset, string name) 
            => string.Format("0x{0:X8}: {1}", (offset += 4) - 4, name);
        private static string DirectInstruction(ref uint offset, string name, IUdonProgram program)
            => string.Format("0x{0:X8}: {1}, 0x{2}", (offset += 8) - 8, name, Convert.ToString((long)(ulong)UIntFromBytes(program.ByteCode, offset - 4), 16).PadLeft(8, '0').ToUpper());
        private static string AnnotationInstruction(ref uint offset, string name, IUdonProgram program)
            => ExternInstruction(ref offset, name, program);
        private static string ExternInstruction(ref uint offset, string name, IUdonProgram program)
            => string.Format("0x{0:X8}: {1}, \"{2}\"", (offset += 8) - 8, name, program.Heap.GetHeapVariable<string>(UIntFromBytes(program.ByteCode, offset - 4)));

        private static string JumpIndirectInstruction(ref uint offset, string name, IUdonProgram program)
        {
            uint addr = UIntFromBytes(program.ByteCode, (offset += 8) - 4);
            if (program.SymbolTable.HasSymbolForAddress(addr))
                return string.Format("0x{0:X8}: {1}, {2}", offset - 8, name, program.SymbolTable.GetSymbolFromAddress(addr));
            else return string.Format("0x{0:X8}: {1}, 0x{2}", offset - 8, name, Convert.ToString((long)(ulong)addr, 16).PadLeft(8, '0').ToUpper());
        }

        private unsafe static uint UIntFromBytes(byte[] bytes, uint startIndex)
            => (uint)((bytes[(int)startIndex] << 24) + (bytes[(int)(startIndex + 1)] << 16) + (bytes[(int)(startIndex + 2)] << 8) + bytes[(int)(startIndex + 3)]);
    }
}

