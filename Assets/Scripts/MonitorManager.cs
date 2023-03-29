using DT.General;
using uDesktopDuplication;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Collections;
using UnityRawInput;

public class MonitorManager : CBC
{
    [SerializeField] GameObject monitorPrefab;
    public LineRenderer LineRenderer;
    public float radius; //屏幕分布的球体半径
    private List<uDesktopDuplication.Texture> textures;
    private List<MonitorControl> controls;
    private uDesktopDuplication.Monitor ArMonitor;
    private bool CursorInArMonitor;
    private bool WaitCursor;
    private Vector2 destPoint;
    private int destTextureID;
    int width = 1920;
    int height = 1080;
    EventBus eb;

    public RectTransform debugpoint;
    void Start()
    {
        // center of primary monitor
        //width = Screen.currentResolution.width;
        //height = Screen.currentResolution.height;
        this.CursorInArMonitor = true;
        WaitCursor = false;
        this.GetComponent<SphereCollider>().radius = radius;
        this.eb = this.Get<EventBus>();
        textures = new List<uDesktopDuplication.Texture>(); 
        controls = new List<MonitorControl>();
        var primaryCenter = this.GetCenter(Manager.primary);

        for (var i = 0; i < Manager.monitors.Count; i++)
        {
            // skip hidden monitors
            if (Config.instance.Monitors.Length > i)
            {
                var config = Config.instance.Monitors[i];
                if (!config.Show) continue;
            }

            var monitor = Manager.monitors[i];
            if (monitor.id != 1)    //todo 指定第二个显示器为AR眼镜显示器，排除AR空间展示，可在外部指定这个id
            {
                var obj = Instantiate(this.monitorPrefab, this.transform);
                var texture = obj.GetComponent<uDesktopDuplication.Texture>();
                texture.monitorId = i;
                var monitorControl = obj.GetComponent<MonitorControl>();
                textures.Add(texture);
                controls.Add(monitorControl);
                Debug.Log(monitor.id);

                if (Config.instance.Monitors.Length > i)
                {
                    // use config
                    var config = Config.instance.Monitors[i];
                    obj.transform.position = config.Position;
                    obj.transform.rotation = Quaternion.Euler(config.Rotation);
                    obj.transform.localScale = config.Scale;
                    texture.bend = config.Bend;
                    texture.radius = config.BendRadius;
                }
                else
                {
                    obj.transform.localScale = new Vector3(monitor.width, monitor.height, 1000) / 1000;
                    // set screen position just like in the system settings
                    Vector3 deltaPosition = (this.GetCenter(monitor) - primaryCenter) / 100;
                    float rotationX = (deltaPosition.x / (Mathf.PI * 2 * radius)) * 360;
                    float rotationY = (deltaPosition.y / (Mathf.PI * 2 * radius)) * 360;
                    Vector3 newVec = Quaternion.Euler(-rotationY, rotationX, 0) * Vector3.forward;

                    Vector3 location = LocationInSphere(Vector3.zero, new Vector3(newVec.x, newVec.y, newVec.z), 1000);

                    obj.transform.localPosition = location;

                    //obj.transform.position = (this.GetCenter(monitor) - primaryCenter) / 100;
                    // look at camera
                    obj.transform.LookAt(2 * obj.transform.position - Camera.main.transform.position);
                }
            }
            else
            {
                ArMonitor = monitor;
                //var monitorControl = obj.GetComponent<MonitorControl>();
                //monitorControl.IsArMonitor = true;
            }
        }

        RawInput.OnMouseMove += (args) =>
        {
            //Win32.SetCursorPos((int)926, (int)58);
            //Debug.Log(args.Point);
            if(MonitorContainCursor(ArMonitor,args.Point,0))
            {
                Vector3 mousePos = new Vector3(args.Point.x - ArMonitor.left, ArMonitor.height - (args.Point.y - ArMonitor.top), 1000);
                Vector3 toPos = Camera.main.ScreenToWorldPoint(mousePos);

                for (var i = 0; i < textures.Count; i++)
                {
                    //鼠标移入屏幕面板时自动跳转至扩展屏真实屏幕坐标
                    var result = textures[i].RayCast(Camera.main.transform.position, toPos - Camera.main.transform.position);
                    if (result.hit)// && result.coords.x == result.coords.x && result.coords.y == result.coords.y)//排除出现NAN数据的情况，出现原因待查
                    {
                        if (MonitorContainCursor(textures[i].monitor, result.desktopCoord, 10))
                        {
                            Debug.Log("鼠标命中面板:" + result.desktopCoord);
                            RawInput.LockMouse = true;
                            //CursorInArMonitor = false;
                            //eb.Invoke("tip", "into pos:" + result.desktopCoord);
                            //Win32.SetCursorPos((int)result.desktopCoord.x, (int)result.desktopCoord.y);
                            //MouseMove((int)result.desktopCoord.x, (int)result.desktopCoord.y);
                            //StartCoroutine(MouseMoveToVirtualMonitor(i, result.desktopCoord));
                            destPoint = result.desktopCoord;
                            destTextureID = i;
                            WaitCursor = true;
                            break;
                        }
                    }
                }
            }
            else
            {
                Debug.Log("not contain");
            }
        };

        this.OnUpdate.AddListener(() =>
        {
            if (Config.instance.CursorSpaceMove)
            {
                if (!WaitCursor)
                {
                    POINT curCursor = new POINT();
                    Win32.GetCursorPos(out curCursor);
                    Debug.Log("鼠标真实位置:" + curCursor);
            //        if (CursorInArMonitor)
            //        {
            //            //if (curCursor.X >= ArMonitor.left && curCursor.X < ArMonitor.right && curCursor.Y >= ArMonitor.top && curCursor.Y < ArMonitor.bottom)//鼠标处于AR显示器中
            //            if (MonitorContainCursor(ArMonitor ,new Vector2(curCursor.X, curCursor.Y),0))//鼠标处于AR显示器中
            //            {
            //                Vector3 mousePos = new Vector3(curCursor.X - ArMonitor.left, ArMonitor.height - (curCursor.Y - ArMonitor.top), 1000);
            //                Debug.Log("鼠标在AR屏鼠标位置:" + mousePos);
            //                Vector3 toPos = Camera.main.ScreenToWorldPoint(mousePos);

            //                for (var i = 0; i < textures.Count; i++)
            //                {
            //                    //鼠标移入屏幕面板时自动跳转至扩展屏真实屏幕坐标
            //                    var result = textures[i].RayCast(Camera.main.transform.position, toPos - Camera.main.transform.position);
            //                    if (result.hit)// && result.coords.x == result.coords.x && result.coords.y == result.coords.y)//排除出现NAN数据的情况，出现原因待查
            //                    {
            //                        if (MonitorContainCursor(textures[i].monitor, result.desktopCoord, 20))
            //                        {
            //                            Debug.Log("鼠标命中面板:" + result.desktopCoord);
            //                            CursorInArMonitor = false;
            //                            //eb.Invoke("tip", "into pos:" + result.desktopCoord);
            //                            //Win32.SetCursorPos((int)result.desktopCoord.x, (int)result.desktopCoord.y);
            //                            //MouseMove((int)result.desktopCoord.x, (int)result.desktopCoord.y);
            //                            //StartCoroutine(MouseMoveToVirtualMonitor(i, result.desktopCoord));
            //                            destPoint = result.desktopCoord;
            //                            destTextureID = i;
            //                            WaitCursor = true;
            //                            break;
            //                        }
            //                    }
            //                }

            //            }
            //            else
            //            {
            //                //Debug.Log("鼠标超出屏幕位置:" + mousePos);
            //                //if (CursorInArMonitor)
            //                //{
            //                //    if (mousePos.x < 0)
            //                //    {
            //                //        mousePos.x = 0;
            //                //    }
            //                //    if (mousePos.x >= width-1)
            //                //    {
            //                //        mousePos.x = width - 1;
            //                //    }
            //                //    if (mousePos.y < 0)
            //                //    {
            //                //        mousePos.y = 0;
            //                //    }
            //                //    if (mousePos.y >= height-1)
            //                //    {
            //                //        mousePos.y = height - 1;
            //                //    }
            //                //    //Mouse.current.WarpCursorPosition(mousePos);

            //                //}
            //            }
            //        }
            //        else
            //        {
            //            if(!MonitorContainCursor(textures[destTextureID].monitor, new Vector2(curCursor.X, curCursor.Y),0))
            //            {
            //                Vector2 desktopCur = new Vector2(curCursor.X, curCursor.Y);
            //                if (curCursor.X < textures[destTextureID].monitor.left)
            //                {
            //                    desktopCur.x = textures[destTextureID].monitor.left;
            //                }
            //                if (curCursor.X > textures[destTextureID].monitor.right)
            //                {
            //                    desktopCur.x = textures[destTextureID].monitor.right;
            //                }
            //                if (curCursor.Y <= textures[destTextureID].monitor.top)
            //                {
            //                    desktopCur.y = textures[destTextureID].monitor.top;
            //                }
            //                if (curCursor.Y >= textures[destTextureID].monitor.bottom)
            //                {
            //                    desktopCur.y = textures[destTextureID].monitor.bottom;
            //                }

            //                //Debug.Log("鼠标移出扩展屏" + texture.monitorId + "鼠标位置:" + curCursor);
            //                Vector3 worldPos = textures[destTextureID].GetWorldPositionFromCoord(new Vector2(desktopCur.x - textures[destTextureID].monitor.left, desktopCur.y - textures[destTextureID].monitor.top));
            //                destPoint = GetScreenPosition(worldPos);
            //                debugpoint.position = destPoint;
            //                WaitCursor = true;
            //                CursorInArMonitor = true;
            //            }
            //        }
            //    }
            //    else
            //    {
            //        POINT curCursor = new POINT();
            //        Win32.GetCursorPos(out curCursor);
            //        Debug.Log("获取鼠标位置");
            //        if (!MonitorContainCursor(textures[destTextureID].monitor, new Vector2(curCursor.X, curCursor.Y), 0))
            //        {
            //            Debug.Log("鼠标不在目标位置，移动鼠标");
            //            Win32.SetCursorPos((int)destPoint.x, (int)destPoint.y);
            //        }
            //        else
            //        {
            //            WaitCursor = false;
            //        }
                }
            }
        });

        //eb.AddListener("cursor world pos", (Vector3 value) => {
        //    Vector2 position = GetScreenPosition(value);
        //    debugpoint.position = position;
        //    StartCoroutine(MouseMoveToARMonitor(position));
        //    //Debug.Log("鼠标移出屏幕invoke执行完成");
        //});

        RawInput.OnKeyDown += (key) =>
        {
            if (key == RawKey.E) 
            {
                Debug.Log("E down");
                if (RawInput.IsKeyDown(RawKey.LeftControl))
                {
                    if (RawInput.LockMouse)
                    {
                        RawInput.LockMouse = false;
                    }
                    else
                    {
                        RawInput.LockMouse = true;
                    }
                    //Win32.SetCursorPos((int)100, (int)100);
                    //if (Config.instance.CursorSpaceMove)
                    //{
                    //    Config.instance.CursorSpaceMove = false;
                    //    //StartCoroutine(MouseMoveToARMonitor(new Vector2(width/2,height/2)));
                    //}
                    //else
                    //{
                    //    Config.instance.CursorSpaceMove = true;
                    //    CursorInArMonitor = true;
                    //    for (var i = 0; i < controls.Count; i++)
                    //    {
                    //        controls[i].enableEdgeDetection = false;
                    //    }
                    //}
                }
            }
            if(key == RawKey.W)
            {
                Win32.SetCursorPos((int)926, (int)58);
            }

        };
    }

    Vector3 GetCenter(Monitor monitor)
    {
        return new Vector3(monitor.left + (monitor.right - monitor.left) / 2, -(monitor.top + (monitor.bottom - monitor.top) / 2), 0);
    }


    IEnumerator MouseMoveToVirtualMonitor(int i,Vector2 destPoint)//保证鼠标移动正确，有概率执行鼠标移动会失败
    {
        POINT curCursor = new POINT();
        Win32.GetCursorPos(out curCursor);
        Debug.Log("获取鼠标位置");
        while (!VirtualMonitorContainCursor(textures[i], new Vector2(curCursor.X, curCursor.Y), 20))
        {
            Debug.Log("鼠标不在目标位置，移动鼠标");
            Win32.SetCursorPos((int)destPoint.x, (int)destPoint.y);
            yield return null;
            Win32.GetCursorPos(out curCursor);
            Debug.Log("再次获取鼠标位置");
        }
        controls[i].enableEdgeDetection = true;
        Debug.Log("鼠标移进屏幕执行完成");
    }
    IEnumerator MouseMoveToMonitor(Monitor monitor, Vector2 curPoint, Vector2 destPoint)//保证鼠标移动正确，有概率执行鼠标移动会失败
    {
        POINT curCursor = new POINT();
        Win32.GetCursorPos(out curCursor);
        Debug.Log("获取鼠标位置");
        while (!MonitorContainCursor(monitor, curPoint, 20))
        {
            Debug.Log("鼠标不在目标位置，移动鼠标");
            Win32.SetCursorPos((int)destPoint.x, (int)destPoint.y);
            yield return null;
            Win32.GetCursorPos(out curCursor);
            Debug.Log("再次获取鼠标位置");
        }
        //controls[i].enableEdgeDetection = true;
        Debug.Log("鼠标移进屏幕执行完成");
    }

    IEnumerator MouseMoveToARMonitor(Vector2 destPoint)
    {
        POINT curCursor = new POINT();
        Win32.GetCursorPos(out curCursor);
        Debug.Log("鼠标目标位置"+destPoint);
        eb.Invoke("tip", "鼠标目标位置:" + destPoint+"cur"+ curCursor);
        //while (!ArMonitorContainCursor(new Vector2(curCursor.X, curCursor.Y)))
        while (!DestContainCursor(new Vector2(curCursor.X, curCursor.Y), destPoint))
        {
            Debug.Log("鼠标不在目标位置，移动鼠标");
            eb.Invoke("tip", "鼠标不在目标位置:");
            //Mouse.current.WarpCursorPosition(destPoint);
            Win32.SetCursorPos((int)destPoint.x, (int)destPoint.y);
            yield return null;
            Win32.GetCursorPos(out curCursor);
            Debug.Log("再次获取鼠标位置");
            eb.Invoke("tip", "鼠标目标位置:" + destPoint + "再次获取鼠标位置" + curCursor);
        }
        Debug.Log("鼠标到达目标位置");
        eb.Invoke("tip", "鼠标到达目标位置:");
        CursorInArMonitor = true;
    }

        /// <summary>
        /// 射线碰撞检测，反向检测
        /// </summary>
    private Vector3 LocationInSphere(Vector3 origin, Vector3 direction, float distances)
    {
        RaycastHit hit;
        Ray ray = new Ray(origin, direction);
        //反向射线
        Ray rayback = new Ray(
             ray.origin + ray.direction * distances,
             -ray.direction);
        if (Physics.Raycast(rayback, out hit, distances))
        {
            Debug.DrawRay(direction, Vector3.forward * hit.distance, Color.yellow);
            LineRenderer.useWorldSpace = true;
            LineRenderer.SetPosition(0, ray.origin + ray.direction * distances);
            LineRenderer.SetPosition(1, hit.point);
            Debug.Log("Did Hit+反向射线" + hit.transform.name);
            Debug.Log(hit.point);
            return hit.point;
        }
        return Vector3.zero;
    }

    //public Vector3 GetScreenPosition(GameObject target)
    //{
    //    RectTransform canvasRtm = parentCanvas.GetComponent<RectTransform>();
    //    float width = canvasRtm.sizeDelta.x;
    //    float height = canvasRtm.sizeDelta.y;
    //    Vector3 pos = Camera.main.WorldToScreenPoint(target.transform.position);
    //    pos.x *= width / Screen.width;
    //    pos.y *= height / Screen.height;
    //    pos.x -= width * 0.5f;
    //    pos.y -= height * 0.5f;
    //    return pos;
    //}

    Vector2 GetScreenPosition(Vector3 target)
    {
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(target);
        Debug.Log("viewport pos:" + viewportPos);
        Vector2 screenPos = Vector2.zero;
        screenPos.x = viewportPos.x * ArMonitor.width + ArMonitor.left; //使用SetCursorPos移动鼠标的计算
        screenPos.y = (1.0f - viewportPos.y) * ArMonitor.height+ ArMonitor.top;
        //screenPos.x = viewportPos.x * width;  // 使用inputsystem移动鼠标的计算
        //screenPos.y = viewportPos.y * height;
        return screenPos;
    }

    bool ArMonitorContainCursor(Vector2 mousePos)
    {
        //return (mousePos.x >= 0 && mousePos.x < width && mousePos.y >= 0 && mousePos.y < height);

        return (mousePos.x > ArMonitor.left && mousePos.x < ArMonitor.right - 1
                                 && mousePos.y > ArMonitor.top && mousePos.y < ArMonitor.bottom - 1);
    }

    bool VirtualMonitorContainCursor(uDesktopDuplication.Texture texture,Vector2 value,int offset)
    {
        return (value.x > texture.monitor.left + offset&& value.x < texture.monitor.right - 1-offset
                                 && value.y > texture.monitor.top+offset && value.y < texture.monitor.bottom - 1-offset);
    }

    bool MonitorContainCursor(Monitor monitor, Vector2 value, int offset)
    {
        return (value.x > monitor.left + offset && value.x < monitor.right - 1 - offset
                                 && value.y > monitor.top + offset && value.y < monitor.bottom - 1 - offset);
    }

    bool DestContainCursor(Vector2 mousePos,Vector2 destPos)
    {
        return (mousePos.x >=destPos.x-100 && mousePos.x <= destPos.x+100 && mousePos.y >= destPos.y-100 && mousePos.y <=destPos.y+100);
    }
}