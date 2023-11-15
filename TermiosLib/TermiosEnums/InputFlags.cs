using System;

namespace TermiosLib.TermiosEnums
{
    [Flags] 
    public enum InputFlags : ulong
    {
        BrkInt = 0x00000002,
        ICrNl  = 0x00000100,
        IgnBrk = 0x00000001,
        IgnCr  = 0x00000080,
        IgnPar = 0x00000004,
        INlCr  = 0x00000040,
        InPCk  = 0x00000010,
        IStrip = 0x00000020,
        IxAny  = 0x00000800,
        IxOff  = 0x00000400,
        IxOn   = 0x00000200,
        ParMrk = 0x00000008
    }
}