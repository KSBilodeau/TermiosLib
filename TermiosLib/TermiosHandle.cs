using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

// TODO: Complete documentation in before next commit
namespace TermiosLib;

public class TermiosHandle
{
    [DllImport("libc", SetLastError = true)]
    private static extern long read(long fileDes, out byte b, long count);

    [DllImport("libc", SetLastError = true)]
    private static extern long tcgetattr(long fileDes, out Termios termios);

    [DllImport("libc", SetLastError = true)]
    private static extern long tcsetattr(long fileDes, nuint optionalActions, in Termios termios);

    [DllImport("libc", SetLastError = true)]
    private static extern long tcdrain(long fileDes);

    [DllImport("libc", SetLastError = true)]
    private static extern long tcflow(long fileDes, nuint action);

    [DllImport("libc", SetLastError = true)]
    private static extern long tcflush(long fileDes, nuint queueSelector);

    [DllImport("libc", SetLastError = true)]
    private static extern long tcgetsid(long fileDes);

    [DllImport("libc", SetLastError = true)]
    private static extern long tcsendbreak(long fileDes, long duration);

    [DllImport("libc", SetLastError = true)]
    private static extern void cfmakeraw(out Termios termios);

    private readonly dynamic _constants;
    private readonly long _fileDes;
    private readonly Termios _termios;

    private static readonly Dictionary<long, Termios> UsedFileDescriptors = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileDes">File descriptor of the TTY compliant device </param>
    /// <param name="termiosPath">Path to the location of the header containing Termios Constants</param>
    public TermiosHandle(long fileDes, string termiosPath)
    {
        _fileDes = fileDes;

        if (UsedFileDescriptors.ContainsKey(fileDes))
            _termios = UsedFileDescriptors[fileDes];
        else
        {
            GetAttrs(out _termios);
            UsedFileDescriptors.Add(fileDes, _termios);
        }

        _constants = new Constants(termiosPath);
    }

    /// <summary>
    /// 
    /// </summary>
    public void EnableRaw()
    {
        GetAttrs(out Termios newState);

        newState.c_iflag &= (nuint)~(_constants.IMAXBEL | _constants.IXOFF | _constants.INPCK | _constants.BRKINT |
                                     _constants.PARMRK | _constants.ISTRIP | _constants.INLCR | _constants.IGNCR |
                                     _constants.ICRNL | _constants.IXON | _constants.IGNPAR);
        newState.c_iflag |= (nuint)_constants.IGNBRK;
        newState.c_oflag &= (nuint)~_constants.OPOST;
        newState.c_lflag &= (nuint)~(_constants.ECHO | _constants.ECHOE | _constants.ECHOK | _constants.ECHONL |
                                     _constants.ICANON | _constants.ISIG | _constants.IEXTEN | _constants.NOFLSH |
                                     _constants.TOSTOP | _constants.PENDIN);
        newState.c_cflag &= (nuint)~(_constants.CSIZE | _constants.PARENB);
        newState.c_cflag |= (nuint)(_constants.CS8 | _constants.CREAD);
        newState.c_cc[(nuint)_constants.VMIN] = 1;
        newState.c_cc[(nuint)_constants.VTIME] = 0;

        SetAttrs((nuint)_constants.TCSANOW, in newState);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ResetTerm()
    {
        SetAttrs((nuint)_constants.TCSANOW, in _termios);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    public long ReadByte(out byte b)
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
    public long GetProcessGroupId()
    {
        var result = tcgetsid(_fileDes);
        return result == -1 ? throw new Exception(Marshal.GetLastSystemError().ToString()) : result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="duration"></param>
    /// <exception cref="Win32Exception"></exception>
    public void SendBreak(long duration)
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
        SetAttrs((nuint)_constants.TCSANOW, in termios);
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
            SetAttrs((nuint)_constants.TCSANOW, in termios);
            ModifyGlobalAttrs((nuint)_constants.TCSANOW, fallbackState);
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
            SetAttrs((nuint)_constants.TCSANOW, in prevState);
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