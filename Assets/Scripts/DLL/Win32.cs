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

}

