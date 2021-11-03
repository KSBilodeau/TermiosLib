# <div style="text-align: center">TermiosLib</div>
A C# wrapper of the renowned C Termios Library for the manipulation of terminal interfaces in a manner defined by POSIX. This library can be used for all serial devices that implement TTY, and or simulate it in one way or another.

See the man page for [<termios.h>](https://pubs.opengroup.org/onlinepubs/7908799/xsh/termios.h.html) for more information regarding the library itself.

### <div style="text-align: center">Example Program:</div>

```c#
using System;
using TermiosLib;
using TermiosLib.TermiosEnums;

namespace Termios
{
    internal static class Program
    {
        public static void Main()
        {
            TermiosLib.Termios termios = new(0);
            
            termios.StateSandbox(() =>
            {
                termios.ModifyGlobalAttrs(OptionalActions.TcSaNow, (ref TermiosAttrs newState) =>
                {
                    newState.c_lflag &= ~(LocalFlags.ICanon | LocalFlags.IExten | LocalFlags.Echo | LocalFlags.ISig);
                    newState.c_oflag &= ~OutputFlags.OPost;
                    newState.c_cflag &= ControlFlags.Cs8;
                    newState.c_iflag &= ~(InputFlags.IxOn | InputFlags.IStrip | InputFlags.ICrNl);
                });
                
                System.IO.StreamWriter sw = new(Console.OpenStandardOutput()) {NewLine = "\r\n", AutoFlush = true};
                while (termios.ReadByte(out var b) != -1 && b != 3)
                {
                    sw.WriteLine(b + $" ({(char) b})");
                }
            });
        }
    }
}
```

## <div style="text-align: center">Standard Defined Functions:</div>
#### [void GetAttrs(out TermiosAttrs termios)](https://pubs.opengroup.org/onlinepubs/7908799/xsh/tcgetattr.html)

*Description:*
```
Returns the current global termios state in the 
C# managed struct through out

Should not fail in under any circumstances unless the
system defines the terminal file descriptor as a number
other than 0, or some strange business regarding the 
group process being orphaned
```

*Example:*
```c#
GetAttrs(out TermiosAttrs termios);

// This is for example purposes only. See ModifyGlobalAttrs(...) for why this is
// should not be done like this.
ModifyGlobalAttrs((ref TermiosAttrs newState) => {
    newState = termios;
});
```

#### [void ModifyGlobalAttrs(OptionalActions optionalActions, ModifyAction modify)](https://pubs.opengroup.org/onlinepubs/7908799/xsh/tcsetattr.html)

ModifyAction is defined as `delegate void ModifyAction(ref TermiosAttrs t)`


Due to calling `tcsetattr(int, int, struct termios*)` before `tcgetattr(int, struct termios*)`
being undefined, this function will always call `GetAttrs(...)` internally.

Hence, it is almost always unnecessary to call `GetAttrs(...)` before `ModifyGlobalAttrs(...)` (especially with the extended lib functions).
Just call the function by itself.

*Description:*
```
Sets the global state of termios through the C# managed
TermiosAttrs sturct 

Should not fail in under any circumstances unless the
system defines the terminal file descriptor as a number
other than 0, or some strange business regarding the 
group process being orphaned
```

*Example:*
```c#
ModifyGlobalAttrs(OptionalActions.TcSaNow, (ref TermiosAttrs newState) =>
{
    newState.c_lflag &= ~(LocalFlags.ICanon | LocalFlags.IExten | LocalFlags.Echo | LocalFlags.ISig);
    newState.c_oflag &= ~OutputFlags.OPost;
    newState.c_cflag &= ControlFlags.Cs8;
    newState.c_iflag &= ~(InputFlags.IxOn | InputFlags.IStrip | InputFlags.ICrNl);
});
```

#### [void DrainOutput()](https://pubs.opengroup.org/onlinepubs/7908799/xsh/tcdrain.html)

*Description:*
```
Waits for all output to be sent to the serial device

Should not fail in under any circumstances unless the
system defines the terminal file descriptor as a number
other than 0, or some strange business regarding the 
group process being orphaned
```

#### [void FlushOutput(LineCtrlFlags queueSelector)](https://pubs.opengroup.org/onlinepubs/7908799/xsh/tcflush.html)

*Description:*
```
Discards data written to the object referred to by 
the file descriptor but not transmitted, or 
data recieved but not read depending on the value
of queue selector

Should not fail in under any circumstances unless the
system defines the terminal file descriptor as a number
other than 0, or some strange business regarding the 
group process being orphaned
```

#### [long GetProcessGroupId()](https://pubs.opengroup.org/onlinepubs/7908799/xsh/tcgetsid.html)

*Description:*
```
Should not fail in under any circumstances unless the
system defines the terminal file descriptor as a number
other than 0
```

#### [void SendBreak(long duration)](https://pubs.opengroup.org/onlinepubs/7908799/xsh/tcsendbreak.html)

*Description:*
```
Will initiate the transmission of zero-valued bits for a specified duration,
which for a duration of zero is between 0.25 and 0.5 seconds and for durations
greater than zero it is implementation defined

Should not fail in under any circumstances unless the
system defines the terminal file descriptor as a number
other than 0, or some strange business regarding the 
group process being orphaned
```

## <div style="text-align: center">Non-POSIX Functions:</div>

#### [void EnableRaw()](https://linux.die.net/man/3/cfmakeraw)

Related to the function `cfmakeraw(3)` defined on certain systems 
that enables flags that enables raw input.

*Description:*
```
Disables the ICANON, IEXTEN, ECHO, ISIG, OPOST, IXON, ISTRIP, and ICRNL flags 
and enables the CS8 flag in order to redirect all input to stdin in 8 bit bytes.

This function fails only if the underlying GetAttrs and SetAttrs calls fail.
```

#### [void ResetTerm()](https://www.freebsd.org/cgi/man.cgi?query=cfmakesane&apropos=0&sektion=3&manpath=FreeBSD+7-current&format=html)

Related to the function `cfmakesane(3)` defined on certain systems
that reverts the global termios flags to a state similar to a newly created
terminal device.

*Description:*
```
Retunrs the terminal to the state it was in at the time of the *orginal* 
constructor call for any specified file descriptor.

This function fails only if the underlying SetAttrs call fails.
```

*Extended Mechanics Explanation:*

Due to this function being necessary for reverting to the original state of the terminal prior to any form of modification,
the global termios struct's state for any given file descriptor is statically stored and applied
to all future instances of a Termios struct with the same file descriptor.  

This guarantees that the behavior of `ResetTerm()` is the same irregardless of any action taken upon the struct
prior to calling.

## <div style="text-align: center">IEEE Defined Functions:</div>

#### [long ReadByte(out byte b)](https://pubs.opengroup.org/onlinepubs/7908799/xsh/read.html)

*Description:*
```
Reads in a single byte from stdin, returning through out with b.  Intended for use 
when Termios raw mode is enabled as it causes Console.Read() to work in an fashion
that may not be expected or desirable.

Failure is NOT handled in the library, so it is necessary to make sure that
the return value of the function is not -1.  Errno may be queried through 
Marshal.GetLastWin32Error() regardless of operating system
```

## <div style="text-align: center">Wrapper Specific Functions:</div>

The following functions are not implemented out of necessity in regards to any specific
standard or library, but rather out of convenience for those who will use this library.

#### void StateSandbox(Action function)

*Description:*
```
Given a function, StateSandbox will execute it in a way that reverts the global
termios state to what it was prior to calling the function, regardless of what 
occurs within the passed function. 

Will only fail if its internal GetAttrs or SetAttrs calls fail.
```

*Examples:*
```c#
using TermiosLib;

TermiosHandle terminalHandle = new(0);

terminalHandle.StateSandbox(() => {
    // Reckless global termios modifications just for the fun of it
    terminalHandle.EnableRaw();
    terminalHandle.ModifyGlobalAttrs((ref TermiosLib.Termios newState) => {
        newState.c_cflag &= ControlFlags.Cs7;
    });
});
// No need to call termios.ResetTerm() because its already been reverted :)
```

#### void FallbackOnFailure(Func<bool> predicate, ModifyAction fallbackState)

*Description:*
```
Given a function that returns a bool, if the function fails (denoted by a return
value of false), then the global state will reflect whatever flags are set in the
fallback state.

Will only fail if its internal GetAttrs or SetAttrs calls fail.
```

*Examples:*
```c#
using TermiosLib;

TermiosHandle terminalHandle = new(0);

terminalHandle.FallbackOnFailure(() => {
    newState.c_cflag &= ~ControlFlags.Cs8;
    newState.c_cflag |= ControlFlags.Cs6;
    // Something causes a failure
    return false;
}, 
(ref TermiosLib.Termios newState) => {
    newState.c_cflag &= ~ControlFlags.Cs8;
    // This is ultimately the flag that gets set
    newState.c_cflag |= ControlFlags.Cs7;
});
// No need to call termios.ResetTerm() because its already been reverted :)
```

#### string GlobalStateString()

*Description:*
```
Returns a formatted string of the struct that represents 
the current global termios struct.

May fail if the internal GetAttrs call fails.
```

#### string OriginalStateString()

*Description:*
```
Returns a formatted string of the struct that represents 
the the global termios struct as it was at the beginning
of the program.

If the constructor for the file descriptor succeeds,
then this function is guaranteed to never fail.
```