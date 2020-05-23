﻿using System;
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

        public LValue(LType _type, dynamic _value) {
            this.Set(_type, _value);
        }

        public void Set(LType _type, dynamic _value) {
            this.Type = _type;
            switch (this.Type) {
                case LType.Number: this.Number = (double)_value; break;
                case LType.String: this.String = (string)_value; break;
            }
        }

        public void Convert(LType _type) {
            if (this.Type == _type) return;
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
                            this.Number = System.Convert.ToDouble(this.String);
                            break;
                        }
                    }
                    break;
                }

                default: {
                    throw new Exception(String.Format("Could not convert {0} from {1} to {2}", this.Value, this.Type, _type));
                }
            }
            this.Type = _type;
            /*
            switch (_type) {
                case LType.Number: {
                    switch (this.Type) {
                        case LType.String: {
                            this.String = this.Value.ToString();
                            Console.WriteLine("SetToString: {0}", this.String);
                            break;
                        }
                    }
                    this.Type = _type;
                    break;
                }

                case LType.String: {
                    switch (this.Type) {
                        case LType.Number: {
                            Console.WriteLine("this.Type = {0}", this.Type);
                            Console.WriteLine("this.Value = {0}", this.Value);
                            this.Number = System.Convert.ToDouble(this.Value);
                            this.Type = _type;
                            Console.WriteLine("this.Type = {0}", this.Type);
                            Console.WriteLine("this.Value = {0}", this.Value);
                            break;
                        }

                        default: {
                            throw new Exception(String.Format("Unsupported conversion from {0} to {1}", this.Type, _type));
                        }
                    }
                    this.Type = _type;
                    break;
                }

                default: {
                    throw new Exception(String.Format("Could not convert {0} from {1} to {2}", this.Value, this.Type, _type));
                }
            }*/
            Console.WriteLine("CovertedAs: {0} ({1})", this.Value, _type); 
        }

        public dynamic Value {
            get {
                switch (this.Type) {
                    case LType.Number: return Number;
                    case LType.String: return String;
                }
                throw new Exception(String.Format("Could not return LValue.Value for type {0}", Type));
            }
        }

        public static LValue operator +(LValue a, LValue b) {
            if (a.Type == LType.Number && b.Type == LType.Number) {
                return new LValue(LType.Number, a.Value + a.Value);
            } else if (a.Type == LType.String && b.Type == LType.String) {
                return new LValue(LType.String, a.Value + b.Value);
            }
            throw new Exception(String.Format("Could not add {0} (Type: {2}) to {1} (Type: {3})", a.Value, b.Value, a.Type, b.Type));
        }

        public static LValue operator -(LValue a, LValue b) {
            if (a.Type == LType.Number && b.Type == LType.Number) {
                return new LValue(LType.Number, a.Value - a.Value);
            }
            throw new Exception(String.Format("Could not subtract {0} (Type: {2}) from {1} (Type: {3})", a.Value, b.Value, a.Type, b.Type));
        }

        public static LValue operator *(LValue a, LValue b) {
            if (a.Type == LType.Number && b.Type == LType.Number) {
                return new LValue(LType.Number, a.Value * a.Value);
            }
            throw new Exception(String.Format("Could not multiply {0} (Type: {2}) by {1} (Type: {3})", a.Value, b.Value, a.Type, b.Type));
        }

        public static LValue operator /(LValue a, LValue b) {
            if (a.Type == LType.Number && b.Type == LType.Number) {
                return new LValue(LType.Number, a.Value / a.Value);
            }
            throw new Exception(String.Format("Could not divide {0} (Type: {2}) by {1} (Type: {3})", a.Value, b.Value, a.Type, b.Type));
        }

        public static LValue operator %(LValue a, LValue b) {
            if (a.Type == LType.Number && b.Type == LType.Number) {
                return new LValue(LType.Number, a.Value / a.Value);
            }
            throw new Exception(String.Format("Could not modulo {0} (Type: {2}) by {1} (Type: {3})", a.Value, b.Value, a.Type, b.Type));
        }

        public static LValue operator <<(LValue a, int b) {
            if (a.Type == LType.Number) {
                return new LValue(LType.Number, a.Value << b);
            }
            throw new Exception(String.Format("Could not bitwise shift-left {0} (Type: {1}) by {2} bits", a.Value, a.Type, b));
        }

        public static LValue operator >>(LValue a, int b) {
            if (a.Type == LType.Number) {
                return new LValue(LType.Number, a.Value << b);
            }
            throw new Exception(String.Format("Could not bitwise shift-right {0} (Type: {1}) by {2} bits", a.Value, a.Type, b));
        }

        public static LValue operator ^(LValue a, int b) {
            if (a.Type == LType.Number) {
                return new LValue(LType.Number, a.Value << b);
            }
            throw new Exception(String.Format("Could not bitwise xor {0} (Type: {1}) with {2}", a.Value, a.Type, b));
        }

        public static LValue operator |(LValue a, int b) {
            if (a.Type == LType.Number) {
                return new LValue(LType.Number, a.Value << b);
            }
            throw new Exception(String.Format("Could not bitwise or {0} (Type: {1}) with {2}", a.Value, a.Type, b));
        }

        public static LValue operator ==(LValue a, LValue b) {
            if (a.Type != b.Type) {
                b.Convert(a.Type);
            }
            return new LValue(LType.Number, (a.Value == b.Value) ? 1 : 0);
        }

        public static LValue operator !=(LValue a, LValue b) {
            if (a.Type != b.Type) {
                b.Convert(a.Type);
            }
            return new LValue(LType.Number, (a.Value != b.Value) ? 1 : 0);
        }

        public static LValue operator <(LValue a, LValue b) {
            a.Convert(LType.Number);
            b.Convert(LType.Number);
            return new LValue(LType.Number, (a.Value > b.Value) ? 1 : 0);
        }

        public static LValue operator >(LValue a, LValue b) {
            a.Convert(LType.Number);
            b.Convert(LType.Number);
            return new LValue(LType.Number, (a.Value < b.Value) ? 1 : 0);
        }

        public static LValue operator <=(LValue a, LValue b) {
            a.Convert(LType.Number);
            b.Convert(LType.Number);
            return new LValue(LType.Number, (a.Value <= b.Value) ? 1 : 0);
        }

        public static LValue operator >=(LValue a, LValue b) {
            a.Convert(LType.Number);
            b.Convert(LType.Number);
            return new LValue(LType.Number, (a.Value >= b.Value) ? 1 : 0);
        }

        public override string ToString() {
            return $"Type: ({this.Type}, Value: {this.Value})";
        }
    }
}
