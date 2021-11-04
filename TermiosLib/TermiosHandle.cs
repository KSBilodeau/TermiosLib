using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using TermiosLib.TermiosEnums;

// TODO: Complete documentation in before next commit
namespace TermiosLib
{
    public class TermiosHandle
    {
        [DllImport("libSystem.B.dylib", EntryPoint = "read", SetLastError = true)]
        private static extern long read_mac(long fileDes, out byte b, long count);
        
        [DllImport("libc.so.6", EntryPoint = "read", SetLastError = true)]
        private static extern long read_linux(long fileDes, out byte b, long count);
        
        [DllImport("libSystem.B.dylib", EntryPoint = "tcgetattr", SetLastError = true)]
        private static extern long get_attr_mac(long fileDes, out Termios termios);
        
        [DllImport("libc.so.6", EntryPoint = "tcgetattr", SetLastError = true)]
        private static extern long get_attr_linux(long fileDes, out Termios termios);
        
        [DllImport("libSystem.B.dylib", EntryPoint = "tcsetattr", SetLastError = true)]
        private static extern long set_attr_mac(long fileDes, OptionalActions optionalActions, in Termios termios);
        
        [DllImport("libc.so.6", EntryPoint = "tcsetattr", SetLastError = true)]
        private static extern long set_attr_linux(long fileDes, OptionalActions optionalActions, in Termios termios);
        
        [DllImport("libSystem.B.dylib", EntryPoint = "tcdrain", SetLastError = true)]
        private static extern long tc_drain_mac(long fileDes);
        
        [DllImport("libc.so.6", EntryPoint = "tcdrain", SetLastError = true)]
        private static extern long tc_drain_linux(long fileDes);
        
        [DllImport("libSystem.B.dylib", EntryPoint = "tcflow", SetLastError = true)]
        private static extern long tc_flow_mac(long fileDes, LineCtrlFlags action);
        
        [DllImport("libc.so.6", EntryPoint = "tcflow", SetLastError = true)]
        private static extern long tc_flow_linux(long fileDes, LineCtrlFlags action);
        
        [DllImport("libSystem.B.dylib", EntryPoint = "tcflush", SetLastError = true)]
        private static extern long tc_flush_mac(long fileDes, LineCtrlFlags queueSelector);
        
        [DllImport("libc.so.6", EntryPoint = "tcflush", SetLastError = true)]
        private static extern long tc_flush_linux(long fileDes, LineCtrlFlags queueSelector);
        
        [DllImport("libSystem.B.dylib", EntryPoint = "tcgetsid", SetLastError = true)]
        private static extern long tc_get_sid_mac(long fileDes);
        
        [DllImport("libc.so.6", EntryPoint = "tcgetsid", SetLastError = true)]
        private static extern long tc_get_sid_linux(long fileDes);
        
        [DllImport("libSystem.B.dylib", EntryPoint = "tcsendbreak", SetLastError = true)]
        private static extern long tc_send_break_mac(long fileDes, long duration);
        
        [DllImport("libc.so.6", EntryPoint = "tcsendbreak", SetLastError = true)]
        private static extern long tc_send_break_linux(long fileDes, long duration);
        
        private readonly long _fileDes;
        private readonly Termios _termios;

        private static readonly bool IsOsx;
        private static readonly Dictionary<long, Termios> UsedFileDescriptors = new();

        /// <summary>
        /// 
        /// </summary>
        static TermiosHandle()
        {
            IsOsx = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileDes">File descriptor of the TTY compliant device </param>
        public TermiosHandle(long fileDes)
        {
            _fileDes = fileDes;

            if (UsedFileDescriptors.ContainsKey(fileDes))
                _termios = UsedFileDescriptors[fileDes];
            else
            {
                GetAttrs(out _termios);
                UsedFileDescriptors.Add(fileDes, _termios);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void EnableRaw()
        {
            GetAttrs(out Termios newState);

            newState.c_lflag &= ~(LocalFlags.ICanon | LocalFlags.IExten | LocalFlags.Echo | LocalFlags.ISig);
            newState.c_oflag &= ~OutputFlags.OPost;
            newState.c_iflag &= ~(InputFlags.IxOn | InputFlags.IStrip | InputFlags.ICrNl);
            newState.c_cflag |= ControlFlags.Cs8;
            
            SetAttrs(OptionalActions.TcSaNow, in newState);
        }
        
        /// <summary>
        /// 
        /// </summary>
        public void ResetTerm()
        {
            SetAttrs(OptionalActions.TcSaNow, in _termios);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public long ReadByte(out byte b)
        {
            return IsOsx ? read_mac(_fileDes, out b, 1) : read_linux(_fileDes, out b, 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="termios"></param>
        /// <exception cref="Win32Exception"></exception>
        public void GetAttrs(out Termios termios)
        {
            if (IsOsx)
            {
                if (get_attr_mac(_fileDes, out termios) == -1)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            else
            {
                if (get_attr_linux(_fileDes, out termios) == -1)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="optionalActions"></param>
        /// <param name="termios"></param>
        /// <exception cref="Win32Exception"></exception>
        private void SetAttrs(OptionalActions optionalActions, in Termios termios)
        {
            if (IsOsx)
            {
                if (set_attr_mac(_fileDes, optionalActions, in termios) == -1)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            else
            {
                if (set_attr_linux(_fileDes, optionalActions, in termios) == -1)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="optionalActions"></param>
        /// <param name="modify"></param>
        public void ModifyGlobalAttrs(OptionalActions optionalActions, ModifyAction modify)
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
            if (IsOsx)
            {
                if (tc_drain_mac(_fileDes) == -1)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            else
            {
                if (tc_drain_linux(_fileDes) == -1)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <exception cref="Win32Exception"></exception>
        public void FlowOutput(LineCtrlFlags action)
        {
            if (IsOsx)
            {
                if (tc_flow_mac(_fileDes, action) == -1)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            else
            {
                if (tc_flow_linux(_fileDes, action) == -1)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="queueSelector"></param>
        /// <exception cref="Win32Exception"></exception>
        public void FlushOutput(LineCtrlFlags queueSelector)
        {
            if (IsOsx)
            {
                if (tc_flush_mac(_fileDes, queueSelector) == -1)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            else
            {
                if (tc_flush_linux(_fileDes, queueSelector) == -1)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Win32Exception"></exception>
        public long GetProcessGroupId()
        {
            if (IsOsx)
            {
                var result = tc_get_sid_mac(_fileDes);
                return result == -1 ? throw new Win32Exception(Marshal.GetLastWin32Error()) : result;
            }
            else
            {
                var result = tc_get_sid_linux(_fileDes);
                return result == -1 ? throw new Win32Exception(Marshal.GetLastWin32Error()) : result;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="duration"></param>
        /// <exception cref="Win32Exception"></exception>
        public void SendBreak(long duration)
        {
            if (IsOsx)
            {
                if (tc_send_break_mac(_fileDes, duration) == -1)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            else
            {
                if (tc_send_break_linux(_fileDes, duration) == -1)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }
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
            SetAttrs(OptionalActions.TcSaNow, in termios);
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
                SetAttrs(OptionalActions.TcSaNow, in termios);
                ModifyGlobalAttrs(OptionalActions.TcSaNow, fallbackState);
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
                SetAttrs(OptionalActions.TcSaNow, in prevState);
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
}