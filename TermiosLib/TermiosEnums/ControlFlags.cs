using System;

namespace TermiosLib.TermiosEnums
{
    [Flags]
    public enum ControlFlags
    {
        CSize  = 0x00000300,
        Cs5    = 0x00000000,
        Cs6    = 0x00000100,
        Cs7    = 0x00000200,
        Cs8    = 0x00000300,
        CStopB = 0x00000400,
        CRead  = 0x00000800,
        ParEnB = 0x00001000,
        ParOdd = 0x00002000,
        HUpCl  = 0x00004000,
        CLocal = 0x00008000
    }
}