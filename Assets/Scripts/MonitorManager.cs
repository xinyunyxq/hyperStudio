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
    private uDesktopDuplication.Monitor ArMonitor;
    private bool CursorInArMonitor;
    private bool WaitCursor;
    private Vector2 destPoint;
    private int destTextureID;
    EventBus eb;

    public RectTransform debugpoint;

    void Start()
    {
        // center of primary monitor
        this.CursorInArMonitor = true;
        WaitCursor = false;
        this.GetComponent<SphereCollider>().radius = radius;
        this.eb = this.Get<EventBus>();
        textures = new List<uDesktopDuplication.Texture>(); 
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
            }
        }

        RawInput.OnMouseMove += (args) =>
        {
            if (Config.instance.CursorSpaceMove)
            {
                if (!RawInput.LockMouse)
                {
                    if (CursorInArMonitor)
                    {
                        if (MonitorContainCursor(ArMonitor, args.Point, 0))
                        {
                            //鼠标在AR屏幕中
                            if (!Config.instance.CursorSpaceMoveEdit)
                            {
                                Vector3 mousePos = new Vector3(args.Point.x - ArMonitor.left, ArMonitor.height - (args.Point.y - ArMonitor.top), 1000);
                                Vector3 toPos = Camera.main.ScreenToWorldPoint(mousePos);

                                for (var i = 0; i < textures.Count; i++)
                                {
                                    //鼠标移入屏幕面板时自动跳转至扩展屏真实屏幕坐标
                                    var result = textures[i].RayCast(Camera.main.transform.position, toPos - Camera.main.transform.position);
                                    if (result.hit)
                                    {
                                        if (MonitorContainCursor(textures[i].monitor, result.desktopCoord, 3))//需要进一步缩小一定像素的屏幕检测空间，防止鼠标在极限边缘来回跳转
                                        {
                                            Debug.Log("鼠标命中面板:" + result.desktopCoord);
                                            RawInput.LockMouse = true; //锁住鼠标，执行鼠标跳转，实测鼠标跳转不一定成功，为了判断执行是否成功必须锁住鼠标移动，否则用户移动鼠标会干扰系统跳转的判断
                                            CursorInArMonitor = false;
                                            destPoint = result.desktopCoord;
                                            destTextureID = i;
                                            WaitCursor = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            //限制鼠标在ar屏中
                            bool limitCursor = false;
                            if (args.Point.x < ArMonitor.left)
                            {
                                args.Point.x = ArMonitor.left;
                                limitCursor = true;
                            }
                            if (args.Point.x > ArMonitor.right -1)
                            {
                                args.Point.x = ArMonitor.right - 1;
                                limitCursor = true;
                            }
                            if (args.Point.y < ArMonitor.top)
                            {
                                args.Point.y = ArMonitor.top;
                                limitCursor = true;
                            }
                            if (args.Point.y > ArMonitor.bottom -1)
                            {
                                args.Point.y = ArMonitor.bottom - 1;
                                limitCursor = true;
                            }
                            if(limitCursor)
                            {
                                RawInput.LockMouse = true;
                                destPoint = args.Point;
                                WaitCursor = true;
                            }
                        }
                    }
                    else
                    {
                        //鼠标不在AR屏幕中

                        //鼠标是否移出当前所在扩展屏
                        if (!MonitorContainCursor(textures[destTextureID].monitor, args.Point, 0))
                        {

                            if (args.Point.x < textures[destTextureID].monitor.left)
                            {
                                args.Point.x = textures[destTextureID].monitor.left;
                            }
                            if (args.Point.x > textures[destTextureID].monitor.right - 1)
                            {
                                args.Point.x = textures[destTextureID].monitor.right - 1;
                            }
                            if (args.Point.y < textures[destTextureID].monitor.top)
                            {
                                args.Point.y = textures[destTextureID].monitor.top;
                            }
                            if (args.Point.y > textures[destTextureID].monitor.bottom - 1)
                            {
                                args.Point.y = textures[destTextureID].monitor.bottom - 1;
                            }

                            Debug.Log("鼠标移出扩展屏" + textures[destTextureID].monitorId + "鼠标位置:" + args.Point);

                            
                            Vector3 worldPos = textures[destTextureID].GetWorldPositionFromCoord(new Vector2(args.Point.x - textures[destTextureID].monitor.left, args.Point.y - textures[destTextureID].monitor.top));
                            destPoint = GetScreenPosition(worldPos);
                            if (MonitorContainCursor(ArMonitor, destPoint, 3))
                            {
                                RawInput.LockMouse = true;
                                WaitCursor = true;
                                CursorInArMonitor = true;
                            }
                                
                            
                        }
                    }
                }
            }
        };

        this.OnUpdate.AddListener(() =>
        {
            if (Config.instance.CursorSpaceMove)
            {
                if (WaitCursor)
                {
                    POINT curCursor = new POINT();
                    Win32.GetCursorPos(out curCursor);
                    Debug.Log("获取鼠标位置");
                    if ((int)destPoint.x != curCursor.X || (int)destPoint.y != curCursor.Y)
                    {
                       Debug.Log("鼠标不在目标位置，移动鼠标");
                       Win32.SetCursorPos((int)destPoint.x, (int)destPoint.y);
                    }
                    else
                    {
                       WaitCursor = false;
                       RawInput.LockMouse = false;
                    }
                }
            }
        });

        //tudo
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
                }
            }
            if(key == RawKey.W)
            {
                Win32.SetCursorPos((int)926, (int)58);
            }

        };

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.W))
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
            }
        }
    }

    Vector3 GetCenter(Monitor monitor)
    {
        return new Vector3(monitor.left + (monitor.right - monitor.left) / 2, -(monitor.top + (monitor.bottom - monitor.top) / 2), 0);
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

    bool MonitorContainCursor(Monitor monitor, Vector2 value, int offset)
    {
        return (value.x > monitor.left + offset && value.x < monitor.right - 1 - offset
                                 && value.y > monitor.top + offset && value.y < monitor.bottom - 1 - offset);
    }

}