using System.IO;
using UnityEngine;

[System.Serializable]
public class Range
{
    public float min;
    public float max;
}

[System.Serializable]
public class ViewZone
{
    public Range pitch; // x
    public Range yaw; // y
}

[System.Serializable]
public class MonitorConfig
{
    public bool Show;
    public Vector3 Position;
    public Vector3 Rotation;
    public Vector3 Scale;
    public bool EnableViewZone;
    public ViewZone ViewZone; // show monitor only when camera's rotation in this range
    public bool Bend;
    public float BendRadius;
}

[System.Serializable]
public class HotKeyConfig
{
    public bool Enabled;
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerhotkey
    public int Modifier;
    // https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
    public uint Key;
}

[System.Serializable]
public class Config
{
    public bool ShowOnAllDesktops;
    public bool AutoLookAtCamera;
    public float TipMessageTimeout;
    public int PanelLuminance;
    public bool CursorSpaceMove;   //鼠标空间移动功能，false时在扩展屏之间移动（windows扩展屏的鼠标移动方式），true时鼠标移出一个扩展屏时会跳转到ar空间，并从空间移动跳转到另一个屏幕
    public bool CursorSpaceMoveEdit; //鼠标空间移动功能下的编辑模式
    public HotKeyConfig ResetViewHotKey;
    public MonitorConfig[] Monitors;

    public static Config Load()
    {
        Config config;
        try
        {
            config = JsonUtility.FromJson<Config>(File.ReadAllText("config.json"));
        }
        catch
        {
            config = null;
        }
        var changed = false;
        if (config == null)
        {
            config = new Config();
            config.ShowOnAllDesktops = true;
            config.AutoLookAtCamera = true;
            config.TipMessageTimeout = 5;
            config.PanelLuminance = 0;
            config.CursorSpaceMove = true;
            config.CursorSpaceMoveEdit = false;
            config.Monitors = new MonitorConfig[0];
            changed = true;
        }
        if (config.ResetViewHotKey == null)
        {
            config.ResetViewHotKey = new HotKeyConfig();
            config.ResetViewHotKey.Enabled = true;
            config.ResetViewHotKey.Modifier = 0x0002 | 0x0008; // MOD_CONTROL + MOD_WIN
            config.ResetViewHotKey.Key = 0x52; // R
            changed = true;
        }
        if (changed) Save(config);
        return config;
    }

    public static void Save()
    {
        Save(Config.instance);
    }

    public static void Save(Config config)
    {
        File.WriteAllText("config.json", JsonUtility.ToJson(config, true));
    }

    // singleton
    private static Config _instance;
    public static Config instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Load();
            }
            return _instance;
        }
    }
}