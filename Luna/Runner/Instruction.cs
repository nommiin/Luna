using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luna {
    public enum LOpcode {
        popv = 5,
        conv = 7,
        mul = 8,
        div = 9,
        rem = 10,
        mod = 11,
        add = 12,
        sub = 13,
        and = 14,
        or = 15,
        xor = 16,
        neg = 17,
        not = 18,
        shl = 19,
        shr = 20,
        set = 21,
        pop = 69,
        pushv = 128,
        pushi = 132,
        dup = 134,
        callv = 153,
        ret = 156,
        exit = 157,
        popz = 158,
        b = 182,
        bt = 183,
        bf = 184,
        pushenv = 186,
        popenv = 187,
        push = 192,
        pushl = 193,
        pushg = 194,
        pushb = 195,
        call = 217,
        brk = 255,
        unknown = 1000
    }

    class Instruction {
        public LOpcode Opcode;
        public byte Argument;
        public Int16 Data;
        public Int32 Raw;

        public static Instruction Decode(Int32 _instruction) {
            return new Instruction((LOpcode)((_instruction >> 24) & 0xFF), (byte)((_instruction >> 16) & 0xFF), (Int16)(_instruction & 0xFFFF), _instruction);
        }

        public Instruction(LOpcode _opcode, byte _argument, Int16 _data, Int32 _raw) {
            this.Opcode = _opcode;
            this.Argument = _argument;
            this.Data = _data;
            this.Raw = _raw;
        }
        
        public override string ToString() {
            return $"Opcode: {((Enum.IsDefined(typeof(LOpcode), this.Opcode) == true) ? "LOpcode." + Enum.GetName(typeof(LOpcode), this.Opcode) : "???")}, Argument: {this.Argument}, Data: {this.Data}";
        }
    }
}
