using System.Runtime.InteropServices;
using System.Text;

namespace TermiosLib;

/// <summary>
/// A managed struct meant for passing backing and forth between the C Termios API and this managed C# wrapper.
/// 
/// All members are ulong as this is the largest representable type for the variable width types present in the
/// Termios C API struct.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Termios
{
    public nuint c_iflag;
    public nuint c_oflag;
    public nuint c_cflag;
    public nuint c_lflag;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    public byte[] c_cc;

    public nuint c_ispeed;
    public nuint c_ospeed;

    public override string ToString()
    {
        StringBuilder builder = new("struct Termios {\r\n");
        builder.Append($"    InputFlags c_iflag = {c_iflag},\r\n");
        builder.Append($"    OutputFlags c_oflag = {c_oflag},\r\n");
        builder.Append($"    OutputFlags c_cflag = {c_cflag},\r\n");
        builder.Append($"    OutputFlags c_lflag = {c_iflag},\r\n");
        builder.Append($"    byte[] c_cc = {c_cc},\r\n");
        builder.Append($"    ulong c_ispeed = {c_ispeed},\r\n");
        builder.Append($"    ulong c_ospeed = {c_ospeed}\r\n");
        builder.Append('}');
        return builder.ToString();
    }
}