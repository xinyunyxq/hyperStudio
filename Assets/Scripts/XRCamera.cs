using DT.General;
using UnityEngine;
using UnityRawInput;

public class XRCamera : CBC
{
    [SerializeField] private LineRenderer LineRenderer;
    float camera_yaw_rotation = 0;
    float camera_pitch_rotation = 0;

    //Rect current_monitor;

    void Start()
    {
        //current_monitor = new Rect(0, 0, 1920, 1080);
        XRSDK.XRSDK_Init();
        XRSDK.Reset();
        XRSDK.SetPanelLuminance(Config.instance.PanelLuminance);

        var eb = this.Get<EventBus>();
        RawInput.WorkInBackground = true;
        RawInput.Start();
        if (Config.instance.ResetViewHotKey.Enabled)
        {
            RawInput.OnKeyDown += (key) =>
            {
                if ((uint)key == Config.instance.ResetViewHotKey.Key)
                {
                    if ((Config.instance.ResetViewHotKey.Modifier & 0x01) != 0 && !RawInput.IsKeyDown(RawKey.LeftButtonAlt)) return;
                    if ((Config.instance.ResetViewHotKey.Modifier & 0x02) != 0 && !RawInput.IsKeyDown(RawKey.LeftControl)) return;
                    if ((Config.instance.ResetViewHotKey.Modifier & 0x04) != 0 && !RawInput.IsKeyDown(RawKey.LeftShift)) return;
                    if ((Config.instance.ResetViewHotKey.Modifier & 0x08) != 0 && !RawInput.IsKeyDown(RawKey.LeftWindows)) return;
                    XRSDK.Reset();
                    eb.Invoke("tip", "Reset View");
                }
            };
        }

        this.OnUpdate.AddListener(() =>
        {
            // update the camera position by reading the sensor data
            float x, y, z, w; // range: [-1, 1]
            unsafe
            {
                long addr = XRSDK.GetArSensor();
                x = *(float*)(addr + 44);
                y = *(float*)(addr + 48);
                z = *(float*)(addr + 52);
                w = *(float*)(addr + 56);
            }
            //--tudo 锁滚动
            //float xRot = -(new Quaternion(x, y, z, w).eulerAngles.x);
            //Quaternion qDiff = new Quaternion(Mathf.Sin(xRot / 2.0f), 0, 0, Mathf.Cos(xRot / 2.0f));
            //qDiff.Normalize();
            ////Quaternion myQuatZeroX = new Quaternion(x, y, z, w) * qDiff;

            this.transform.rotation = new Quaternion(x, y, z, w);

            if (x == 0f && y == 0f && z == 0f && w == 0f)
            {
                this.transform.rotation = Quaternion.Euler(camera_pitch_rotation, camera_yaw_rotation, 0);
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    camera_yaw_rotation += Time.deltaTime * 45;

                }
                if (Input.GetKey(KeyCode.RightArrow))
                {
                    camera_yaw_rotation -= Time.deltaTime * 45;
                }

                if (Input.GetKey(KeyCode.UpArrow))
                {
                    camera_pitch_rotation += Time.deltaTime * 45;

                }
                if (Input.GetKey(KeyCode.DownArrow))
                {
                    camera_pitch_rotation -= Time.deltaTime * 45;
                }
            }
            //draw Ray line
            Vector2 input = Input.mousePosition;
            if (input.x > 0 && input.x < Screen.width && input.y > 0 && input.y < Screen.height)
            {
                LineRenderer.enabled = true;
                LineRenderer.useWorldSpace = true;
                //设置开始和结束位置0代表第一个点，1代表第二个点
                Vector3 cameraPosition = this.transform.position;
                cameraPosition.y -= 2;
                LineRenderer.SetPosition(0, cameraPosition);
                //Debug.Log(this.transform.position);
                Vector3 mousePosition = new Vector3(input.x, input.y, 10);
                //Debug.Log(mousePosition);
                Vector3 objPosition = Camera.main.ScreenToWorldPoint(mousePosition);
                //Debug.Log(objPosition);
                LineRenderer.SetPosition(1, objPosition);

            }
            else
            {
                LineRenderer.enabled = false;
            }

            // ctrl + R to reset
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
            {
                XRSDK.Reset();
                eb.Invoke("tip", "Reset View");
            }
        });
    }

    void OnApplicationQuit()
    {
        RawInput.Stop();
    }

}
