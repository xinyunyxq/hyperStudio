using DT.General;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowSettingPanel : ASignal { }
public class SettingsButtonHandler : MonoBehaviour
{
    private Button button;
    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            Debug.Log("show panel");
            Signals.Get<ShowSettingPanel>().Dispatch();

        });//¼àÌýµã»÷ÊÂ¼þ
    }

}
