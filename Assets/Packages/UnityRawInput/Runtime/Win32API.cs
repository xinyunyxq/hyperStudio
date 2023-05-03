using System;
using System.Runtime.InteropServices;

namespace UnityRawInput
{
    public static class Win32API
    {
        public delegate int HookProc(int code, IntPtr wParam, IntPtr lParam);

        [DllImport("User32")]
        public static extern IntPtr SetWindowsHookEx(HookType code, HookProc func, IntPtr hInstance, int threadID);
        [DllImport("User32")]
        public static extern int UnhookWindowsHookEx(IntPtr hhook);
        [DllImport("User32")]
        public static extern int CallNextHookEx(IntPtr hhook, int code, IntPtr wParam, IntPtr lParam);
        [DllImport("Kernel32")]
        public static extern uint GetCurrentThreadId();
        [DllImport("Kernel32")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int mouse_event(int dwflags, int dx, int dy, int cbuttons, int dwextrainfo);

        public const int mouseeventf_move = 0x0001;      //移动鼠标
        public const int mouseeventf_leftdown = 0x0002; //模拟鼠标左键按下
        public const int mouseeventf_leftup = 0x0004; //模拟鼠标左键抬起
        public const int mouseeventf_rightdown = 0x0008; //模拟鼠标右键按下
        public const int mouseeventf_rightup = 0x0010; //模拟鼠标右键抬起
        public const int mouseeventf_middledown = 0x0020; //模拟鼠标中键按下
        public const int mouseeventf_middleup = 0x0040; //模拟鼠标中键抬起
        public const int mouseeventf_absolute = 0x8000; //标示是否采用绝对坐标

    }
}
