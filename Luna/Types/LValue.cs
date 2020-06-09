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

        public LValue(LType _type, object _val) {
            Number = 0;
            String = "";
            I32 = 0;
            I64 = 0;
            
            Type = _type;
            switch (Type) {
                case LType.Number: Number = (double)_val; break;
                case LType.String: String = (string)_val; break;
                case LType.Int32: I32 = (Int32)_val; break;
                case LType.Int64: I64 = (Int64)_val; break;
            }
        }

        public object Value {
            get {
                switch (Type) {
                    case LType.Number: return Number;
                    case LType.String: return String;
                    case LType.Int32: return I32;
                    case LType.Int64: return I64;
                }
                throw new Exception(String.Format("Could not return value for type: {0}", Type));
            }
        }

        public static implicit operator double(LValue _val) => _val.Number;
        public static implicit operator string(LValue _val) => _val.String;
        public static implicit operator Int32(LValue _val) => _val.I32;
        public static implicit operator Int64(LValue _val) => _val.I64;

        public static LValue operator ==(LValue a, LValue b) {
            switch (a.Type)
            {
                //see NearlyEqual
                case LType.Number: return new LValue(LType.Number, (double) (NearlyEqual(a.Number, b.Number) ? 1 : 0));
                case LType.String: return new LValue(LType.String, (double) (a.String == b.String ? 1 : 0));
            }

            throw new Exception("Could not compare");
        }
        
        //best to find a better place to house this function
        //stolen from SO
        //https://stackoverflow.com/questions/3874627/floating-point-comparison-functions-for-c-sharp
        private static bool NearlyEqual(double a, double b, double epsilon = 0.00001f)
        {
            const double MinNormal = 2.2250738585072014E-308d;
            double absA = Math.Abs(a);
            double absB = Math.Abs(b);
            double diff = Math.Abs(a - b);

            if (a.Equals(b))
            { // shortcut, handles infinities
                return true;
            }
            if (a == 0 || b == 0 || absA + absB < MinNormal) 
            {
                // a or b is zero or both are extremely close to it
                // relative error is less meaningful here
                return diff < (epsilon * MinNormal);
            }
            // use relative error
            return diff / (absA + absB) < epsilon;
        }

        public static LValue operator !=(LValue a, LValue b) {
            switch (a.Type) {
                //switched 0 and 1 around for Number, doesn't even require a negate
                case LType.Number: return new LValue(LType.Number, (double)(NearlyEqual(a.Number,b.Number) ? 0 : 1));
                case LType.String: return new LValue(LType.String, (double)(a.String != b.String ? 1 : 0));
            }
            throw new Exception("Could not compare");
        }

        public static LValue operator +(LValue a, LValue b) {
            if (a.Type == LType.Number && b.Type == LType.Number) {
                return new LValue(LType.Number, a.Number + b.Number);
            }
            if (a.Type == LType.String && b.Type == LType.String)
            {
                return new LValue(LType.String, String.Concat(a.String, b.String));//safest way to do string concat
            }
            throw new Exception("Could not add 2 values");
        }

        public static LValue operator -(LValue a, LValue b) {
            if (a.Type == LType.Number && b.Type == LType.Number) {
                return new LValue(LType.Number, a.Number - b.Number);
            }
            throw new Exception("Could not subtract 2 values");
        }

        public static LValue operator <(LValue a, LValue b) {
            return new LValue(LType.Number, (double)a.Number < b.Number ? (double)1 : (double)0);
        }

        public static LValue operator >(LValue a, LValue b) {
            return new LValue(LType.Number, a.Number > b.Number ? (double)1 : (double)0);
        }

        public static LValue operator <=(LValue a, LValue b) {
            return new LValue(LType.Number, a.Number <= b.Number ? (double)1 : (double)0);
        }

        public static LValue operator >=(LValue a, LValue b) {
            return new LValue(LType.Number, a.Number >= b.Number ? (double)1 : (double)0);
        }
    }
}
