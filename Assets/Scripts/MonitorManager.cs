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
    public float radius; //��Ļ�ֲ�������뾶
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
            if (monitor.id != 1)    //todo ָ���ڶ�����ʾ��ΪAR�۾���ʾ�����ų�AR�ռ�չʾ�������ⲿָ�����id
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
                    //���������Ļ���ʱ�Զ���ת����չ����ʵ��Ļ����
                    var result = textures[i].RayCast(Camera.main.transform.position, toPos - Camera.main.transform.position);
                    if (result.hit)// && result.coords.x == result.coords.x && result.coords.y == result.coords.y)//�ų�����NAN���ݵ����������ԭ�����
                    {
                        if (MonitorContainCursor(textures[i].monitor, result.desktopCoord, 10))
                        {
                            Debug.Log("����������:" + result.desktopCoord);
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
                    Debug.Log("�����ʵλ��:" + curCursor);
            //        if (CursorInArMonitor)
            //        {
            //            //if (curCursor.X >= ArMonitor.left && curCursor.X < ArMonitor.right && curCursor.Y >= ArMonitor.top && curCursor.Y < ArMonitor.bottom)//��괦��AR��ʾ����
            //            if (MonitorContainCursor(ArMonitor ,new Vector2(curCursor.X, curCursor.Y),0))//��괦��AR��ʾ����
            //            {
            //                Vector3 mousePos = new Vector3(curCursor.X - ArMonitor.left, ArMonitor.height - (curCursor.Y - ArMonitor.top), 1000);
            //                Debug.Log("�����AR�����λ��:" + mousePos);
            //                Vector3 toPos = Camera.main.ScreenToWorldPoint(mousePos);

            //                for (var i = 0; i < textures.Count; i++)
            //                {
            //                    //���������Ļ���ʱ�Զ���ת����չ����ʵ��Ļ����
            //                    var result = textures[i].RayCast(Camera.main.transform.position, toPos - Camera.main.transform.position);
            //                    if (result.hit)// && result.coords.x == result.coords.x && result.coords.y == result.coords.y)//�ų�����NAN���ݵ����������ԭ�����
            //                    {
            //                        if (MonitorContainCursor(textures[i].monitor, result.desktopCoord, 20))
            //                        {
            //                            Debug.Log("����������:" + result.desktopCoord);
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
            //                //Debug.Log("��곬����Ļλ��:" + mousePos);
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

            //                //Debug.Log("����Ƴ���չ��" + texture.monitorId + "���λ��:" + curCursor);
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
            //        Debug.Log("��ȡ���λ��");
            //        if (!MonitorContainCursor(textures[destTextureID].monitor, new Vector2(curCursor.X, curCursor.Y), 0))
            //        {
            //            Debug.Log("��겻��Ŀ��λ�ã��ƶ����");
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
        //    //Debug.Log("����Ƴ���Ļinvokeִ�����");
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


    IEnumerator MouseMoveToVirtualMonitor(int i,Vector2 destPoint)//��֤����ƶ���ȷ���и���ִ������ƶ���ʧ��
    {
        POINT curCursor = new POINT();
        Win32.GetCursorPos(out curCursor);
        Debug.Log("��ȡ���λ��");
        while (!VirtualMonitorContainCursor(textures[i], new Vector2(curCursor.X, curCursor.Y), 20))
        {
            Debug.Log("��겻��Ŀ��λ�ã��ƶ����");
            Win32.SetCursorPos((int)destPoint.x, (int)destPoint.y);
            yield return null;
            Win32.GetCursorPos(out curCursor);
            Debug.Log("�ٴλ�ȡ���λ��");
        }
        controls[i].enableEdgeDetection = true;
        Debug.Log("����ƽ���Ļִ�����");
    }
    IEnumerator MouseMoveToMonitor(Monitor monitor, Vector2 curPoint, Vector2 destPoint)//��֤����ƶ���ȷ���и���ִ������ƶ���ʧ��
    {
        POINT curCursor = new POINT();
        Win32.GetCursorPos(out curCursor);
        Debug.Log("��ȡ���λ��");
        while (!MonitorContainCursor(monitor, curPoint, 20))
        {
            Debug.Log("��겻��Ŀ��λ�ã��ƶ����");
            Win32.SetCursorPos((int)destPoint.x, (int)destPoint.y);
            yield return null;
            Win32.GetCursorPos(out curCursor);
            Debug.Log("�ٴλ�ȡ���λ��");
        }
        //controls[i].enableEdgeDetection = true;
        Debug.Log("����ƽ���Ļִ�����");
    }

    IEnumerator MouseMoveToARMonitor(Vector2 destPoint)
    {
        POINT curCursor = new POINT();
        Win32.GetCursorPos(out curCursor);
        Debug.Log("���Ŀ��λ��"+destPoint);
        eb.Invoke("tip", "���Ŀ��λ��:" + destPoint+"cur"+ curCursor);
        //while (!ArMonitorContainCursor(new Vector2(curCursor.X, curCursor.Y)))
        while (!DestContainCursor(new Vector2(curCursor.X, curCursor.Y), destPoint))
        {
            Debug.Log("��겻��Ŀ��λ�ã��ƶ����");
            eb.Invoke("tip", "��겻��Ŀ��λ��:");
            //Mouse.current.WarpCursorPosition(destPoint);
            Win32.SetCursorPos((int)destPoint.x, (int)destPoint.y);
            yield return null;
            Win32.GetCursorPos(out curCursor);
            Debug.Log("�ٴλ�ȡ���λ��");
            eb.Invoke("tip", "���Ŀ��λ��:" + destPoint + "�ٴλ�ȡ���λ��" + curCursor);
        }
        Debug.Log("��굽��Ŀ��λ��");
        eb.Invoke("tip", "��굽��Ŀ��λ��:");
        CursorInArMonitor = true;
    }

        /// <summary>
        /// ������ײ��⣬������
        /// </summary>
    private Vector3 LocationInSphere(Vector3 origin, Vector3 direction, float distances)
    {
        RaycastHit hit;
        Ray ray = new Ray(origin, direction);
        //��������
        Ray rayback = new Ray(
             ray.origin + ray.direction * distances,
             -ray.direction);
        if (Physics.Raycast(rayback, out hit, distances))
        {
            Debug.DrawRay(direction, Vector3.forward * hit.distance, Color.yellow);
            LineRenderer.useWorldSpace = true;
            LineRenderer.SetPosition(0, ray.origin + ray.direction * distances);
            LineRenderer.SetPosition(1, hit.point);
            Debug.Log("Did Hit+��������" + hit.transform.name);
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
        screenPos.x = viewportPos.x * ArMonitor.width + ArMonitor.left; //ʹ��SetCursorPos�ƶ����ļ���
        screenPos.y = (1.0f - viewportPos.y) * ArMonitor.height+ ArMonitor.top;
        //screenPos.x = viewportPos.x * width;  // ʹ��inputsystem�ƶ����ļ���
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