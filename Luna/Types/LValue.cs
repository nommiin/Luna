using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime;
using System.Runtime.InteropServices;

namespace Luna {
    public enum LType {
        Number,
        String,
        Array,
        Pointer,
        Vec3,
        Undefined,
        Int32 = 7,
        Vec4,
        Matrix,
        Int64,
        Accessor,
        Null,
        Bool,
        Iterator
    }

    struct LValue {
        public LType Type;
        public double Number;
        public Int32 I32;
        public Int64 I64;
        public string String;
        public LValue[] Array;

        public LValue(LType _type, object _val) {
            this.Number = 0;
            this.String = "";
            this.I32 = 0;
            this.I64 = 0;
            this.Array = null;
            
            this.Type = _type;
            switch (this.Type) {
                case LType.Number: this.Number = (double)_val; break;
                case LType.String: this.String = (string)_val; break;
                case LType.Int32: this.I32 = (Int32)_val; break;
                case LType.Int64: this.I64 = (Int64)_val; break;
                case LType.Array: this.Array = (LValue[])_val; break;
            }
        }

        public object Value {
            get {
                switch (this.Type) {
                    case LType.Number: return this.Number;
                    case LType.String: return this.String;
                    case LType.Int32: return this.I32;
                    case LType.Int64: return this.I64;
                    case LType.Array: return this.Array;
                }
                throw new Exception(String.Format("Could not return value for type: {0}", this.Type));
            }
        }

        public static implicit operator double(LValue _val) => _val.Number;
        public static implicit operator string(LValue _val) => _val.String;
        public static implicit operator Int32(LValue _val) => _val.I32;
        public static implicit operator Int64(LValue _val) => _val.I64;
        public static implicit operator LValue[](LValue _val) => _val.Array;

        public static LValue operator ==(LValue a, LValue b) {
            switch (a.Type) {
                case LType.Number: return new LValue(LType.Number, (double)((double)a.Value == (double)b.Value ? 1 : 0));
                case LType.String: return new LValue(LType.String, (double)((string)a.Value == (string)b.Value ? 1 : 0));
            }
            throw new Exception("Could not compare");
        }

        public static LValue operator !=(LValue a, LValue b) {
            switch (a.Type) {
                case LType.Number: return new LValue(LType.Number, (double)((double)a.Value != (double)b.Value ? 1 : 0));
                case LType.String: return new LValue(LType.String, (double)((string)a.Value != (string)b.Value ? 1 : 0));
            }
            throw new Exception("Could not compare");
        }

        public static LValue operator +(LValue a, LValue b) {
            if (a.Type == LType.Number && a.Type == LType.Number) {
                return new LValue(LType.Number, (double)((double)a.Value + (double)b.Value));
            } else if (a.Type == LType.String && b.Type == LType.String) {
                return new LValue(LType.String, (string)((string)a.Value + (string)b.Value));
            }
            throw new Exception("Could not add 2 values");
        }

        public static LValue operator -(LValue a, LValue b) {
            if (a.Type == LType.Number && b.Type == LType.Number) {
                return new LValue(LType.Number, ((double)a.Value - (double)b.Value));
            }
            throw new Exception("Could not subtract 2 values");
        }

        public static LValue operator <(LValue a, LValue b) {
            return new LValue(LType.Number, ((double)a.Value < (double)b.Value) ? (double)1 : (double)0);
        }

        public static LValue operator >(LValue a, LValue b) {
            return new LValue(LType.Number, ((double)a.Value > (double)b.Value) ? (double)1 : (double)0);
        }

        public static LValue operator <=(LValue a, LValue b) {
            return new LValue(LType.Number, ((double)a.Value <= (double)b.Value) ? (double)1 : (double)0);
        }

        public static LValue operator >=(LValue a, LValue b) {
            return new LValue(LType.Number, ((double)a.Value >= (double)b.Value) ? (double)1 : (double)0);
        }
        #region Static Initializers
        public static LValue Real(double _value)
        {
            return new LValue(LType.Number, _value);
        }
        public static LValue Text(string _value)
        {
            return new LValue(LType.String, _value);
        }
        public static LValue Values(params LValue[] _value)
        {
            return new LValue(LType.Array, _value);
        }
        #endregion

    }
}
