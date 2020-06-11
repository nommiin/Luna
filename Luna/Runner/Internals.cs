using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace Luna.Runner
{
    /// <summary>
    /// A class purely meant for storing methods and subclasses that functions will use
    /// </summary>
    public class FunctionInternals
    {
        
    }

    /// <summary>
    /// Implementation of the Well512a RNG algorithm
    /// </summary>
    /// <remarks>
    /// http://www.iro.umontreal.ca/~panneton/well/WELL512a.c
    /// </remarks>
    class WellGenerator
    {
        private static readonly int W = 32;
        private static readonly int R = 16;
        private static readonly int P = 0;
        private static readonly int M1 = 13;
        private static readonly int M2 = 9;
        private static readonly int M3 = 5;
        private static readonly double FACT = 2.32830643653869628906e-10;
        private static readonly WellGenerator Instance;
        
        private int state_i = 0;
        private List<uint> State = Enumerable.Repeat(0u,R).ToList();
        private uint z0, z1, z2;

        static WellGenerator() 
        {
            Instance = new WellGenerator();
            
        }
        private WellGenerator(){}

        private uint Mat0Pos(int t, uint v) => v ^ v >> t;
        private uint Mat0Neg(int t, uint v) => v ^ v << -t;
        private uint Mat3Neg(int t, uint v) => v << -t;
        private uint Mat4Neg(int t, uint b, uint v) => v ^ v << -t & b;

        protected double next()
        {
            z0 = State[(state_i + 15) & 15];
            z1 = Mat0Neg(-16,State[state_i]) ^ Mat0Neg(-15, State[(state_i + M1) & 15]);
            z2 = 
        }
    }
}