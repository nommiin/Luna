using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime;

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
        Vec44,
        Int64,
        Accessor,
        Null,
        Bool,
        Iterator
    }

    class LValue {
        public LType Type;
        double Number;
        string String;
        LValue[] Array;
        // ?? Pointer;
        // ?? Vec3;
        byte Undefined;
        Int32 Int32;
        // ?? Vec4;
        // ?? Vec44;
        Int64 Int64;
        // ?? Accessor;
        byte Null;
        double Bool;
        // ?? Iterator;

        public LValue(LType _type, object _value) {
            this.Set(_type, _value);
        }

        public LValue(LType _type) {
            this.Set(_type, 0);
        }

        public LValue Set(LType _type, object _value) {
            this.Type = _type;
            switch (this.Type) {
                case LType.Number: this.Number = System.Convert.ToDouble(_value); break;
                case LType.String: this.String = System.Convert.ToString(_value); break;
                case LType.Undefined: this.Undefined = System.Convert.ToByte(_value); break;
            }
            return this;
        }

        public LValue Convert(LType _type) {
            if (this.Type == _type) return this;
            switch (this.Type) {
                case LType.Number: {
                    switch (_type) {
                        case LType.String: {
                            this.String = this.Number.ToString();
                            break;
                        }
                    }
                    break;
                }

                case LType.String: {
                    switch (_type) {
                        case LType.Number: {
                            int i = 0;
                            while (Char.IsNumber(this.String[i]) == true) {
                                i++;
                            }
                            if (i > 0) this.Number = System.Convert.ToDouble(this.String.Substring(0, i));
                            else throw new Exception(String.Format("Could not convert {0} from {1} to {2}", this.String, this.Type, _type));
                            break;
                        }
                    }
                    break;
                }

                case LType.Undefined: {
                    switch (_type) {
                        case LType.String: {
                            this.String = "undefined";
                            break;
                        }

                        default: {
                            throw new Exception(String.Format("Could not convert undefined to {0}", _type));
                        }
                    }
                    break;
                }

                default: {
                    throw new Exception(String.Format("Could not convert {0} from {1} to {2}", this.Value, this.Type, _type));
                }
            }
            this.Type = _type;
            return this;
        }

        public object Value {
            get {
                switch (this.Type) {
                    case LType.Number: return Number;
                    case LType.String: return String;
                    case LType.Undefined: return "undefined";
                }
                throw new Exception(String.Format("Could not return LValue.Value for type {0}", Type));
            }
        }

        public static LValue operator +(LValue a, LValue b) {
            if (a.Type == LType.Number && b.Type == LType.Number) {
                return new LValue(LType.Number, (double)a.Value + (double)b.Value);
            } else if (a.Type == LType.String && b.Type == LType.String) {
                return new LValue(LType.String, (string)a.Value + (string)b.Value);
            }
            throw new Exception(String.Format("Could not add {0} (Type: {2}) to {1} (Type: {3})", a.Value, b.Value, a.Type, b.Type));
        }

        public static LValue operator -(LValue a, LValue b) {
            if (a.Type == LType.Number && b.Type == LType.Number) {
                return new LValue(LType.Number, (double)a.Value - (double)b.Value);
            }
            throw new Exception(String.Format("Could not subtract {0} (Type: {2}) from {1} (Type: {3})", a.Value, b.Value, a.Type, b.Type));
        }

        public static LValue operator *(LValue a, LValue b) {
            if (a.Type == LType.Number && b.Type == LType.Number) {
                return new LValue(LType.Number, (double)a.Value * (double)b.Value);
            }
            throw new Exception(String.Format("Could not multiply {0} (Type: {2}) by {1} (Type: {3})", a.Value, b.Value, a.Type, b.Type));
        }

        public static LValue operator /(LValue a, LValue b) {
            if (a.Type == LType.Number && b.Type == LType.Number) {
                return new LValue(LType.Number, (double)a.Value / (double)b.Value);
            }
            throw new Exception(String.Format("Could not divide {0} (Type: {2}) by {1} (Type: {3})", a.Value, b.Value, a.Type, b.Type));
        }

        public static LValue operator %(LValue a, LValue b) {
            if (a.Type == LType.Number && b.Type == LType.Number) {
                return new LValue(LType.Number, (double)a.Value % (double)b.Value);
            }
            throw new Exception(String.Format("Could not modulo {0} (Type: {2}) by {1} (Type: {3})", a.Value, b.Value, a.Type, b.Type));
        }

        public static LValue operator <<(LValue a, int b) {
            if (a.Type == LType.Number) {
                return new LValue(LType.Number, (long)a.Value << (int)b);
            }
            throw new Exception(String.Format("Could not bitwise shift-left {0} (Type: {1}) by {2} bits", a.Value, a.Type, b));
        }

        public static LValue operator >>(LValue a, int b) {
            if (a.Type == LType.Number) {
                return new LValue(LType.Number, (long)a.Value << (int)b);
            }
            throw new Exception(String.Format("Could not bitwise shift-right {0} (Type: {1}) by {2} bits", a.Value, a.Type, b));
        }

        public static LValue operator ^(LValue a, int b) {
            if (a.Type == LType.Number) {
                return new LValue(LType.Number, (long)a.Value << (int)b);
            }
            throw new Exception(String.Format("Could not bitwise xor {0} (Type: {1}) with {2}", a.Value, a.Type, b));
        }

        public static LValue operator |(LValue a, int b) {
            if (a.Type == LType.Number) {
                return new LValue(LType.Number, (long)a.Value << (int)b);
            }
            throw new Exception(String.Format("Could not bitwise or {0} (Type: {1}) with {2}", a.Value, a.Type, b));
        }

        public static LValue operator ==(LValue a, LValue b) {
            if (a.Type != b.Type) b.Convert(a.Type);
            switch (a.Type) {
                case LType.Number: return new LValue(LType.Number, ((double)a.Value == (double)b.Value) ? 1 : 0);
                case LType.String: return new LValue(LType.Number, ((string)a.Value == (string)b.Value) ? 1 : 0);
            }
            throw new Exception(String.Format("Could not compare if {0} (Type: {1}) is equal to {2} (Type: {3})", a.Value, a.Type, b.Value, b.Type));
        }

        public static LValue operator !=(LValue a, LValue b) {
            if (a.Type != b.Type) b.Convert(a.Type);
            switch (a.Type) {
                case LType.Number: return new LValue(LType.Number, ((double)a.Value != (double)b.Value) ? 1 : 0);
                case LType.String: return new LValue(LType.Number, ((string)a.Value != (string)b.Value) ? 1 : 0);
            }
            throw new Exception(String.Format("Could not compare if {0} (Type: {1}) is not equal to {2} (Type: {3})", a.Value, a.Type, b.Value, b.Type));
        }

        public static LValue operator <(LValue a, LValue b) {
            a.Convert(LType.Number);
            b.Convert(LType.Number);
            return new LValue(LType.Number, ((double)a.Value < (double)b.Value) ? 1 : 0);
        }

        public static LValue operator >(LValue a, LValue b) {
            a.Convert(LType.Number);
            b.Convert(LType.Number);
            return new LValue(LType.Number, ((double)a.Value > (double)b.Value) ? 1 : 0);
        }

        public static LValue operator <=(LValue a, LValue b) {
            a.Convert(LType.Number);
            b.Convert(LType.Number);
            return new LValue(LType.Number, ((double)a.Value <= (double)b.Value) ? 1 : 0);
        }

        public static LValue operator >=(LValue a, LValue b) {
            a.Convert(LType.Number);
            b.Convert(LType.Number);
            return new LValue(LType.Number, ((double)a.Value >= (double)b.Value) ? 1 : 0);
        }

        public override string ToString() {
            return $"Type: ({this.Type}, Value: {this.Value})";
        }
    }
}
