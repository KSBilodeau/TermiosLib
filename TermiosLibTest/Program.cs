using TermiosLib;

var handle = new TermiosHandle(0, "/Library/Developer/CommandLineTools/SDKs/MacOSX11.3.sdk/System/Library/Frameworks/Kernel.framework/Versions/A/Headers/sys/termios.h");

handle.StateSandbox(() =>
{
    handle.EnableRaw();
    
    while (handle.ReadByte(out var b) != -1)
    {
        Console.Write(b + "\r\n");
        Console.Out.Flush();
    }
});

Console.WriteLine("SUCCESS");