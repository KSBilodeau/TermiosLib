using System;

namespace TermiosLib.TermiosEnums
{
    [Flags] 
    public enum OutputFlags : ulong
    {
        OPost  = 0x00000001,
        ONlCr  = 0x00000002,
        OCrNl  = 0x00000010,
        ONoCr  = 0x00000020,
        ONlRet = 0x00000040,
        OFill  = 0x00000080,
        NlDly  = 0x00000300,
        Nl0    = 0x00000000,
        Nl1    = 0x00000100,
        CrDly  = 0x00003000,
        Cr0    = 0x00000000,
        Cr1    = 0x00001000,
        Cr2    = 0x00002000,
        Cr3    = 0x00003000,
        TabDly = 0x00000c04,
        Tab0   = 0x00000000,
        Tab1   = 0x00000400,
        Tab2   = 0x00000800,
        Tab3   = 0x00000004,
        BsDly  = 0x00008000,
        Bs0    = 0x00000000,
        Bs1    = 0x00008000,
        VtDly  = 0x00010000,
        Vt0    = 0x00000000,
        Vt1    = 0x00010000,
        FfDly  = 0x00004000,
        Ff0    = 0x00000000,
        Ff1    = 0x00004000
    }
}