using System;

namespace TermiosLib.TermiosEnums
{
    [Flags]
    public enum LineCtrlFlags : short
    {
        TcIFlush  = 1,
        TcIoFlush = 3,
        TcOFlush  = 2,
        TcIOff    = 3,
        TcIOn     = 4,
        TcOOff    = 1,
        TcOOn     = 2
    }
}