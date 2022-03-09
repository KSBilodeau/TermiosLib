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