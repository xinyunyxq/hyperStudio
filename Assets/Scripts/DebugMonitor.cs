using System;
using System.Text;
using UnityEngine;
using System.IO;
using System.Diagnostics;
public sealed class DebugMonitor : MonoBehaviour
{
    //
    private StringBuilder _builder = new StringBuilder();

    //文件保存路径
    private string _logPath = "";

    //是否自动清理
    public bool isAutoClear = true;

    //多久后清理
    public int maxDay = 2;

    private void Start()
    {
#if UNITY_EDITOR
        
#else
        Init();
#endif
    }
    private void OnDestroy()
    {
        Application.logMessageReceived -= LogMessage;
        Application.quitting -= ApplicationQuit;
    }

    //初始化
    private void Init()
    {
        //不同平台下，设置不同路径
        //_logPath = Application.streamingAssetsPath + "/DebugMessage.txt";
        _logPath = "DebugMessage.txt";

        // debug 日志 回调
        Application.logMessageReceived += LogMessage;

        //程序退出中 回调
        Application.quitting += ApplicationQuit;

        //
        if (isAutoClear && File.Exists(_logPath))
        {
            try
            {
                DateTime _lastTime = File.GetLastWriteTime(_logPath);
                if ((DateTime.Now - _lastTime).TotalDays > maxDay)
                {
                    DeleteFile(_logPath);
                }
            }
            catch { }
        }
    }

    /// <summary>
    /// 程序退出
    /// </summary>
    private void ApplicationQuit()
    {
        //
        _builder.Append(DateTime.Now + "-----------------------退出程序---------------------------" + "\n");

        //
        SaveToTxt(_logPath, _builder.ToString());
    }

    /// <summary>
    /// 实时监听 打印日志
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="stackTrace"></param>
    /// <param name="type"></param>
    private void LogMessage(string condition, string stackTrace, LogType type)
    {
        //string all = string.Format("第一个参数 : {0} 。 第二个参数 : {1} 。 第三个参数 : {2}", condition, stackTrace, type.ToString());

        _builder.Append(type);
        _builder.Append("->");
        _builder.Append("Info->");
        _builder.Append(condition);
        _builder.Append("\n");

        //获取堆栈调用信息
        //StackTrace st = new StackTrace(true);
        //var frames = st.GetFrames();

        //for (int i = 0; i < frames.Length; i++)
        //{
        //    _builder.Append(i);
        //    _builder.Append("\n");

        //    //方法
        //    var method = frames[i].GetMethod();
        //    _builder.Append("Method->");
        //    _builder.Append(method);
        //    _builder.Append("\n");

        //    //文件类
        //    var fileName = frames[i].GetFileName();
        //    _builder.Append("File->");
        //    _builder.Append(fileName);
        //    _builder.Append("\n");
        //}

        //_builder.Append("\n");
    }

    private void DeleteFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            try
            {
                File.SetAttributes(filePath, FileAttributes.Normal);
                File.Delete(filePath);
            }
            catch (IOException e)
            {
                UnityEngine.Debug.LogError(e.Message + "\n" + filePath);
            }
        }
    }

    private bool SaveToTxt(string filePath, string source)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            UnityEngine.Debug.Log("filePath is null");
            return false;
        }

        if (source == null)
        {
            UnityEngine.Debug.Log("source is not allowed to be null");
            return false;
        }

        try
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Append, FileAccess.Write))
            {
                byte[] arr = Encoding.UTF8.GetBytes(source);
                stream.Write(arr, 0, arr.Length);
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError(e.Message);
            return false;
        }

        UnityEngine.Debug.Log(source.GetType() + " Txt Save Success~");
        return true;
    }
}

