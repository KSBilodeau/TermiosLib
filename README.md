# TermiosLib
A C# wrapper of the renowned C Termios Library for the manipulation of terminal interfaces in a manner defined by POSIX. This library can be used for all serial devices that implement TTY, and or simulate it in one way or another.

See the man page for [<termios.h>](https://pubs.opengroup.org/onlinepubs/7908799/xsh/termios.h.html) for more information regarding the library itself.

### Dependencies:

[ExpressionEvaluator]()

Thank you to CodingSeb for their fantastic ExpressionEvaluator library that allows for the quick evaluation of C Termios constants across all platforms.

### Example Program:

```c#
using System;
using TermiosLib;

var termios = new TermiosHandle(0, "path/to/termios.h");
var constants = termios.Constants;

termios.StateSandbox(() =>
{
    termios.ModifyGlobalAttrs((nuint)constants.TCSANOW, (ref Termios newState) =>
    {
        newState.c_lflag &= (nuint)~(constants.ICANON | constants.IEXTEN | constants.ECHO | constants.ISIG);
        newState.c_oflag &= (nuint)~constants.OPOST;
        newState.c_cflag &= (nuint)constants.CS8;
        newState.c_iflag &= (nuint)~(constants.IXON | constants.ISTRIP | constants.ICRNL);
    });
                
    StreamWriter sw = new(Console.OpenStandardOutput()) {NewLine = "\r\n", AutoFlush = true};
    while (termios.ReadByte(out var b) != -1 && b != 3)
    {
        sw.WriteLine(b + $" ({(char) b})");
    }
});
```

## Standard Defined Functions:
#### [void GetAttrs(out Termios termios)](https://pubs.opengroup.org/onlinepubs/7908799/xsh/tcgetattr.html)

*Description:*

Returns the current global termios state in the 
C# managed struct through out.

*Failure cases:*

Should not fail in under any circumstances unless the
system defines the terminal file descriptor as a number
other than 0, or some strange business regarding the 
group process being orphaned.

*Example:*
```c#
termios.GetAttrs(out Termios oldState);

// This is for example purposes only. See ModifyGlobalAttrs(...) for why this is
// should not be done like this.
termios.ModifyGlobalAttrs((nuint) constants.TCSANOW, (ref Termios newState) => {
    newState = oldState;
});
```

#### [void ModifyGlobalAttrs(nuint optionalActions, ModifyAction modify)](https://pubs.opengroup.org/onlinepubs/7908799/xsh/tcsetattr.html)

ModifyAction is defined as `delegate void ModifyAction(ref Termios t)`


Due to calling `tcsetattr(int, int, struct termios*)` before `tcgetattr(int, struct termios*)`
being undefined, this function will always call `GetAttrs(...)` internally. Hence, it is almost always unnecessary to call `GetAttrs(...)` before `ModifyGlobalAttrs(...)` (especially with the extended lib functions).
Just call the function by itself.

*Description:*

Sets the global state of termios through the C# managed
Termios struct 

*Failure cases:*

Should not fail in under any circumstances unless the
system defines the terminal file descriptor as a number
other than 0, or some strange business regarding the 
group process being orphaned


*Example:*
```c#
termios.ModifyGlobalAttrs((nuint)constants.TCSANOW, (ref Termios newState) =>
{
    newState.c_lflag &= (nuint)~(constants.ICANON | constants.IEXTEN | constants.ECHO | constants.ISIG);
    newState.c_oflag &= (nuint)~constants.OPOST;
    newState.c_cflag &= (nuint)constants.CS8;
    newState.c_iflag &= (nuint)~(constants.IXON | constants.ISTRIP | constants.ICRNL);
});
```

#### [void DrainOutput()](https://pubs.opengroup.org/onlinepubs/7908799/xsh/tcdrain.html)

*Description:*

Waits for all output to be sent to the serial device

*Failure cases:*

Should not fail in under any circumstances unless the
system defines the terminal file descriptor as a number
other than 0, or some strange business regarding the 
group process being orphaned

#### [void FlushOutput(nuint queueSelector)](https://pubs.opengroup.org/onlinepubs/7908799/xsh/tcflush.html)

*Description:*

Discards data written to the object referred to by 
the file descriptor but not transmitted, or 
data received but not read depending on the value
of queue selector.

*Failure cases:*

Should not fail under any circumstances unless the
system defines the terminal file descriptor as a number
other than 0, or some strange business regarding the 
group process being orphaned.

#### [nint GetProcessGroupId()](https://pubs.opengroup.org/onlinepubs/7908799/xsh/tcgetsid.html)

*Description:*

Returns the group process id for the terminal.

*Failure cases:*

Should not fail in under any circumstances unless the
system defines the terminal file descriptor as a number
other than 0.

#### [void SendBreak(nuint duration)](https://pubs.opengroup.org/onlinepubs/7908799/xsh/tcsendbreak.html)

*Description:*

Will initiate the transmission of zero-valued bits for a specified duration,
which for a duration of zero is between 0.25 and 0.5 seconds and for durations
greater than zero it is implementation defined

*Failure cases:*

Should not fail under any circumstances unless the system defines the terminal file descriptor as a number other than 0, or some strange business regarding the 
group process being orphaned

## Non-POSIX Functions:

#### [void EnableRaw()](https://linux.die.net/man/3/cfmakeraw)

Related to the function `cfmakeraw(3)` defined on certain systems 
that enables flags that enables raw input.

*Description:*
Disables the ICANON, IEXTEN, ECHO, ISIG, OPOST, IXON, ISTRIP, and ICRNL flags and enables the CS8 flag in order to redirect all input to stdin in 8 bit bytes.

*Failure cases:*

This function fails only if the underlying GetAttrs and SetAttrs calls fail.

#### [void ResetTerm()](https://www.freebsd.org/cgi/man.cgi?query=cfmakesane&apropos=0&sektion=3&manpath=FreeBSD+7-current&format=html)

Related to the function `cfmakesane(3)` defined on certain systems
that reverts the global termios flags to a state similar to a newly created
terminal device.

*Description:*

Returns the terminal to the state it was in at the time of the *original* constructor call for any specified file descriptor.

*Failure cases:*

This function fails only if the underlying SetAttrs call fails.

*Extended Mechanics Explanation:*

Due to this function being necessary for reverting to the original state of the terminal prior to any form of modification,
the global termios struct's state for any given file descriptor is statically stored and applied
to all future instances of a Termios struct with the same file descriptor.  

This guarantees that the behavior of `ResetTerm()` is the same irregardless of any action taken upon the struct
prior to calling.

## IEEE Defined Functions:

#### [nint ReadByte(out byte b)](https://pubs.opengroup.org/onlinepubs/7908799/xsh/read.html)

*Description:*

Reads in a single byte from stdin, returning through out with b.  Intended for use 
when Termios raw mode is enabled as it causes Console.Read() to work in an fashion
that may not be expected or desirable.

*Failure cases:*

Failure is NOT handled in the library, so it is necessary to make sure that
the return value of the function is not -1.  Errno may be queried through 
Marshal.GetLastWin32Error() regardless of operating system

## Wrapper Specific Functions:

The following functions are not implemented out of necessity in regards to any specific
standard or library, but rather out of convenience for those who will use this library.

#### void StateSandbox(Action function)

*Description:*

Given a function, StateSandbox will execute it in a way that reverts the global termios state to what it was prior to calling the function, regardless of what occurs within the passed function. 

*Failure Cases:*

Will only fail if its internal GetAttrs or SetAttrs calls fail.

*Examples:*
```c#
using TermiosLib;

var termios = new TermiosHandle(0, "path/to/termios.h");
var constants = termios.Constants;

termios.StateSandbox(() => {
    // Reckless global termios modifications just for the fun of it
    termios.EnableRaw();
    termios.ModifyGlobalAttrs((ref TermiosLib.Termios newState) => {
        newState.c_cflag &= (nuint)constants.Cs7;
    });
});
// No need to call termios.ResetTerm() because its already been reverted :)
```

#### void FallbackOnFailure(Func<bool> predicate, ModifyAction fallbackState)

*Description:*

Given a function that returns a bool, if the function fails (denoted by a return
value of false), then the global state will reflect whatever flags are set in the
fallback state.

*Failure cases:*

Will only fail if its internal GetAttrs or SetAttrs calls fail.

*Examples:*
```c#
using TermiosLib;

var termios = new TermiosHandle(0,
    "/Library/Developer/CommandLineTools/SDKs/MacOSX11.3.sdk/System/Library/Frameworks/Kernel.framework/Versions/A/Headers/sys/termios.h");
var constants = termios.Constants;

termios.FallbackOnFailure(() =>
    {
        termios.ModifyGlobalAttrs((nuint)constants.TCSANOW, (ref Termios oldState) =>
        {
            oldState.c_cflag &= (nuint)~constants.CS8;
            oldState.c_cflag |= (nuint)constants.CS6;
        });
        
        // Something causes a failure
        return false;
    },
    (ref Termios newState) =>
    {
        newState.c_cflag &= (nuint)~constants.CS8;
        // This is ultimately the flag that gets set
        newState.c_cflag |= (nuint)constants.CS7;
    }
);
```

#### string GlobalStateString()

*Description:*

Returns a formatted string of the struct that represents 
the current global termios struct.

*Failure cases:*

May fail if the internal GetAttrs call fails.

#### string OriginalStateString()

*Description:*

Returns a formatted string of the struct that represents 
the the global termios struct as it was at the beginning
of the program.

*Failure Cases:*

If the constructor for the file descriptor succeeds,
then this function is guaranteed to never fail.