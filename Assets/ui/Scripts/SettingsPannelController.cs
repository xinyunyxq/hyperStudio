using deVoid.UIFramework;
using DT.General;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using uDesktopDuplication;
using UnityEngine.SceneManagement;

public class SettingsFloatApply : ASignal<string,float> { }
public class SettingsBoolApply : ASignal<string, bool> { }

public class SettingsPannelController : APanelController
{
    private Slider SliderFov;
    private Slider SliderScreenDistances;
    private Slider SliderGyroSense;
    private Toggle ToggleFullScreen;
    private Toggle ToggleAutoFaceCamera;
    private Toggle ToggleBendScreen;
    private Button ButtonSaveLayout;
    private Button ButtonLoadLayout;
    private Button ButtonResetLayout;
    // Start is called before the first frame update
    void Start()
    {
        SliderFov = transform.Find("SliderFov").GetComponent<Slider>();
        SliderScreenDistances = transform.Find("SliderScreenDistances").GetComponent<Slider>();
        SliderGyroSense = transform.Find("SliderGyroSense").GetComponent<Slider>();

        ToggleFullScreen = transform.Find("ToggleFullScreen").GetComponent<Toggle>();
        ToggleAutoFaceCamera = transform.Find("ToggleAutoFaceCamera").GetComponent<Toggle>();
        ToggleBendScreen = transform.Find("ToggleBendScreen").GetComponent<Toggle>();

        ButtonSaveLayout = transform.Find("ButtonSaveLayout").GetComponent<Button>();
        ButtonLoadLayout = transform.Find("ButtonLoadLayout").GetComponent<Button>();
        ButtonResetLayout = transform.Find("ButtonResetLayout").GetComponent<Button>();

        SliderFov.value = Config.instance.MainCameraFov;

        SliderFov.onValueChanged.AddListener((float value) =>
        {

            Camera.main.GetComponent<Camera>().fieldOfView = value;
            Config.instance.MainCameraFov = value;

        });

        SliderScreenDistances.value = Config.instance.ScreenDistances;

        SliderScreenDistances.onValueChanged.AddListener((float value) =>
        {

            Signals.Get<SettingsFloatApply>().Dispatch("ScreenDistances", value);
            Config.instance.ScreenDistances = value;

        });

        ToggleFullScreen.onValueChanged.AddListener((bool value) =>
        {
            Screen.fullScreen = value;
        });

        ToggleAutoFaceCamera.onValueChanged.AddListener((bool value) =>
        {
            Config.instance.AutoLookAtCamera = value;
        });

        ToggleBendScreen.onValueChanged.AddListener((bool value) =>
        {
            Signals.Get<SettingsBoolApply>().Dispatch("ScreenBend", value);
        });

        ButtonSaveLayout.onClick.AddListener(() =>
        {
                // get monitor info
                var old = Config.instance.Monitors;
                Config.instance.Monitors = new MonitorConfig[Manager.monitors.Count];
                for (var i = 0; i < Manager.monitors.Count; i++)
                {
                    Config.instance.Monitors[i] = new MonitorConfig();
                    Config.instance.Monitors[i].Show = false;
                }
                Array.ForEach(GameObject.Find("MonitorManager").GetComponentsInChildren<uDesktopDuplication.Texture>(), ((texture) =>
                {
                    var config = Config.instance.Monitors[texture.monitorId];
                    config.Show = true;
                    config.Position = texture.transform.position;
                    config.Rotation = texture.transform.rotation.eulerAngles;
                    config.Scale = texture.transform.localScale;
                    config.Bend = texture.bend;
                    config.BendRadius = texture.radius;
                    var control = texture.GetComponent<MonitorControl>();
                    config.EnableViewZone = control.enableViewZone;
                    config.ViewZone = new ViewZone();
                    config.ViewZone.pitch = control.pitch;
                    config.ViewZone.pitch = control.yaw;
                }));

                Config.Save();

        });

        ButtonLoadLayout.onClick.AddListener(() =>
        {
            //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            foreach (Transform child in GameObject.Find("MonitorManager").transform)
            {
                var texture = child.GetComponent<uDesktopDuplication.Texture>();
                var id = texture.monitorId;
                if(id < Config.instance.Monitors.Length)
                {
                    var config = Config.instance.Monitors[id];
                    child.transform.position = config.Position;
                    child.transform.rotation = Quaternion.Euler(config.Rotation);
                    child.transform.localScale = config.Scale;
                    texture.bend = config.Bend;
                    texture.radius = config.BendRadius;
                }
            }

        });

        ButtonResetLayout.onClick.AddListener(() =>
        {
            Config.instance.Monitors  = new MonitorConfig[0];
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
