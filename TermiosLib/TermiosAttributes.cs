using System.Runtime.InteropServices;
using System.Text;
using TermiosLib.TermiosEnums;

namespace TermiosLib
{
    /// <summary>
    /// A managed struct meant for passing backing and forth between the C Termios API and this managed C# wrapper.
    /// 
    /// All members are ulong as this is the largest representable type for the variable width types present in the
    /// Termios C API struct.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Termios
    {
        public InputFlags c_iflag;
        public OutputFlags c_oflag;
        public ControlFlags c_cflag;
        public LocalFlags c_lflag;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] c_cc;
        public ulong c_ispeed;
        public ulong c_ospeed;

        public override string ToString()
        {
            StringBuilder builder = new("struct Termios {\r\n");
            builder.AppendFormat("    InputFlags c_iflag = {0},\r\n", c_iflag);
            builder.AppendFormat("    OutputFlags c_oflag = {0},\r\n", c_oflag);
            builder.AppendFormat("    OutputFlags c_cflag = {0},\r\n", c_cflag);
            builder.AppendFormat("    OutputFlags c_lflag = {0},\r\n", c_lflag);
            builder.AppendFormat("    byte[] c_cc = {0},\r\n", c_cc);
            builder.AppendFormat("    ulong c_ispeed = {0},\r\n", c_ispeed);
            builder.AppendFormat("    ulong c_ospeed = {0}\r\n", c_ospeed);
            builder.Append('}');
            return builder.ToString();
        }
    }
}