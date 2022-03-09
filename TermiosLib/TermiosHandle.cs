using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

// TODO: Complete documentation in before next commit
namespace TermiosLib;

public class TermiosHandle
{
    [DllImport("libc", SetLastError = true)]
    private static extern nint read(nuint fileDes, out byte b, nuint count);

    [DllImport("libc", SetLastError = true)]
    private static extern nint tcgetattr(nuint fileDes, out Termios termios);

    [DllImport("libc", SetLastError = true)]
    private static extern nint tcsetattr(nuint fileDes, nuint optionalActions, in Termios termios);

    [DllImport("libc", SetLastError = true)]
    private static extern nint tcdrain(nuint fileDes);

    [DllImport("libc", SetLastError = true)]
    private static extern nint tcflow(nuint fileDes, nuint action);

    [DllImport("libc", SetLastError = true)]
    private static extern nint tcflush(nuint fileDes, nuint queueSelector);

    [DllImport("libc", SetLastError = true)]
    private static extern nint tcgetsid(nuint fileDes);

    [DllImport("libc", SetLastError = true)]
    private static extern nint tcsendbreak(nuint fileDes, nuint duration);
    
    public readonly dynamic Constants;
    private readonly nuint _fileDes;
    private readonly Termios _termios;

    private static readonly Dictionary<nuint, Termios> UsedFileDescriptors = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileDes">File descriptor of the TTY compliant device </param>
    /// <param name="termiosPath">Path to the location of the header containing Termios Constants</param>
    public TermiosHandle(nuint fileDes, string termiosPath)
    {
        _fileDes = fileDes;

        if (UsedFileDescriptors.ContainsKey(fileDes))
            _termios = UsedFileDescriptors[fileDes];
        else
        {
            GetAttrs(out _termios);
            UsedFileDescriptors.Add(fileDes, _termios);
        }

        Constants = new Constants(termiosPath);
    }

    /// <summary>
    /// 
    /// </summary>
    public void EnableRaw()
    {
        GetAttrs(out Termios newState);

        newState.c_iflag &= (nuint)~(Constants.IMAXBEL | Constants.IXOFF | Constants.INPCK | Constants.BRKINT |
                                     Constants.PARMRK | Constants.ISTRIP | Constants.INLCR | Constants.IGNCR |
                                     Constants.ICRNL | Constants.IXON | Constants.IGNPAR);
        newState.c_iflag |= (nuint)Constants.IGNBRK;
        newState.c_oflag &= (nuint)~Constants.OPOST;
        newState.c_lflag &= (nuint)~(Constants.ECHO | Constants.ECHOE | Constants.ECHOK | Constants.ECHONL |
                                     Constants.ICANON | Constants.ISIG | Constants.IEXTEN | Constants.NOFLSH |
                                     Constants.TOSTOP | Constants.PENDIN);
        newState.c_cflag &= (nuint)~(Constants.CSIZE | Constants.PARENB);
        newState.c_cflag |= (nuint)(Constants.CS8 | Constants.CREAD);
        newState.c_cc[(nuint)Constants.VMIN] = 1;
        newState.c_cc[(nuint)Constants.VTIME] = 0;

        SetAttrs((nuint)Constants.TCSANOW, in newState);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ResetTerm()
    {
        SetAttrs((nuint)Constants.TCSANOW, in _termios);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    public nint ReadByte(out byte b)
    {
        return read(_fileDes, out b, 1);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="termios"></param>
    /// <exception cref="Win32Exception"></exception>
    public void GetAttrs(out Termios termios)
    {
        if (tcgetattr(_fileDes, out termios) == -1)
            throw new Exception(Marshal.GetLastWin32Error().ToString());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="optionalActions"></param>
    /// <param name="termios"></param>
    /// <exception cref="Win32Exception"></exception>
    private void SetAttrs(nuint optionalActions, in Termios termios)
    {
        if (tcsetattr(_fileDes, optionalActions, in termios) == -1)
            throw new Exception(Marshal.GetLastSystemError().ToString());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="optionalActions"></param>
    /// <param name="modify"></param>
    public void ModifyGlobalAttrs(nuint optionalActions, ModifyAction modify)
    {
        GetAttrs(out Termios termios);
        modify(ref termios);
        SetAttrs(optionalActions, in termios);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="Win32Exception"></exception>
    public void DrainOutput()
    {
        if (tcdrain(_fileDes) == -1)
            throw new Exception(Marshal.GetLastSystemError().ToString());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="action"></param>
    /// <exception cref="Win32Exception"></exception>
    public void FlowOutput(nuint action)
    {
        if (tcflow(_fileDes, action) == -1)
            throw new Exception(Marshal.GetLastSystemError().ToString());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="queueSelector"></param>
    /// <exception cref="Win32Exception"></exception>
    public void FlushOutput(nuint queueSelector)
    {
        if (tcflush(_fileDes, queueSelector) == -1)
            throw new Exception(Marshal.GetLastSystemError().ToString());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Win32Exception"></exception>
    public nint GetProcessGroupId()
    {
        var result = tcgetsid(_fileDes);
        return result == -1 ? throw new Exception(Marshal.GetLastSystemError().ToString()) : result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="duration"></param>
    /// <exception cref="Win32Exception"></exception>
    public void SendBreak(nuint duration)
    {
        if (tcsendbreak(_fileDes, duration) == -1)
            throw new Exception(Marshal.GetLastSystemError().ToString());
    }

    /// <summary>
    /// 
    /// </summary>
    public delegate void ModifyAction(ref Termios t);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="function"></param>
    public void StateSandbox(Action function)
    {
        GetAttrs(out Termios termios);
        function();
        SetAttrs((nuint)Constants.TCSANOW, in termios);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="fallbackState"></param>
    public void FallbackOnFailure(Func<bool> predicate, ModifyAction fallbackState)
    {
        GetAttrs(out Termios termios);
        if (!predicate())
        {
            SetAttrs((nuint)Constants.TCSANOW, in termios);
            ModifyGlobalAttrs((nuint)Constants.TCSANOW, fallbackState);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="predicate"></param>
    public void RevertOnFailure(Func<bool> predicate)
    {
        GetAttrs(out Termios prevState);
        if (!predicate())
            SetAttrs((nuint)Constants.TCSANOW, in prevState);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string GlobalStateString()
    {
        GetAttrs(out var termios);
        return termios.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string OriginalStateString()
    {
        return _termios.ToString();
    }
}