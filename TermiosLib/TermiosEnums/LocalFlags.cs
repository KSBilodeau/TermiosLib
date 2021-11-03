using System;

namespace TermiosLib.TermiosEnums
{
    [Flags]
    public enum LocalFlags : ulong
    {
        Echo   = 0x00000008,
        EchoE  = 0x00000002,
        EchoK  = 0x00000004,
        EchoNl = 0x00000010,
        ICanon = 0x00000100,
        IExten = 0x00000400,
        ISig   = 0x00000400,
        NoFlsh = 0x80000000,
        ToStop = 0x00400000
    }
}