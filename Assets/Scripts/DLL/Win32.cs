using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct POINT
{
    public int X;
    public int Y;

    public POINT(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    public override string ToString()
    {
        return ("X:" + X + ", Y:" + Y);
    }
}
public static class Win32 {
  [DllImport("user32.dll")]
  public static extern int GetActiveWindow();

    [DllImport("user32.dll")]
    public static extern int SetActiveWindow(int hwnd);
    [DllImport("user32.dll")]
  public static extern int SetWindowLongA(int hwnd, int index, long style);

    [DllImport("user32.dll")]
    public static extern int SetCursorPos(int x, int y);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern bool GetCursorPos(out POINT pt);

    [StructLayout(LayoutKind.Sequential)]
    public class MouseHookStruct
    {
        public POINT pt;
        public int hwnd;
        public int wHitTestCode;
        public int dwExtraInfo;
    }
    public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);
    //安装钩子
    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);
    //卸载钩子
    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern bool UnhookWindowsHookEx(int idHook);
    //调用下一个钩子
    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);

}


public class MouseHook
{

    private POINT point;

    private int hHook;
    public const int WH_MOUSE_LL = 14;
    public const int WM_MOUSEMOVE = 0x0200;
    public const int WM_RBUTTONDOWN = 0x0204;
    public const int WM_LBUTTONDOWN = 0x0201;
    public Win32.HookProc hProc;

    public MouseHook()
    {
        this.point = new POINT();
    }
    public int SetHook()
    {
        hProc = new Win32.HookProc(MouseHookProc);
        hHook = Win32.SetWindowsHookEx(WH_MOUSE_LL, hProc, IntPtr.Zero, 0);
        return hHook;
    }
    public void UnHook()
    {
        Win32.UnhookWindowsHookEx(hHook);
    }
    private int MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
        Win32.MouseHookStruct MyMouseHookStruct = (Win32.MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(Win32.MouseHookStruct));
        if (nCode < 0)
        {
            return Win32.CallNextHookEx(hHook, nCode, wParam, lParam);
        }
        else
        {
            switch ((int)wParam)
            {
                case (int)WM_LBUTTONDOWN:
                    if (MouseClickEvent != null)
                    {
                        this.point = new POINT(MyMouseHookStruct.pt.X, MyMouseHookStruct.pt.Y);
                        MouseClickEvent(this,0, point);
                    }
                    break;
                case (int)WM_RBUTTONDOWN:
                    if (MouseClickEvent != null)
                    {
                        this.point = new POINT(MyMouseHookStruct.pt.X, MyMouseHookStruct.pt.Y);
                        MouseClickEvent(this, 1,point);
                    }
                    break;
                case (int)WM_MOUSEMOVE:
                    if (MouseMoveEvent != null)
                    {
                        this.point = new POINT(MyMouseHookStruct.pt.X, MyMouseHookStruct.pt.Y);
                        MouseMoveEvent(this, point);
                    }
                    break;
                default:
                    break;
            }
            return Win32.CallNextHookEx(hHook, nCode, wParam, lParam);
        }
    }
    //委托+事件（把钩到的消息封装为事件，由调用者处理）
    public delegate void MouseMoveHandler(object sender, POINT point);
    public event MouseMoveHandler MouseMoveEvent;

    public delegate void MouseClickHandler(object sender, int buttonType,POINT point);
    public event MouseClickHandler MouseClickEvent;
}
