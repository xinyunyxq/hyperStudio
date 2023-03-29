using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InterfaceHandle : MonoBehaviour
{
    // Start is called before the first frame update
    private Button button_edit;
    void Start()
    {
        button_edit = GameObject.Find("Button_edit").GetComponent<Button>();//通过Find查找名称获得我们要的Button组件
        button_edit.onClick.AddListener(OnEditButtonClick);//监听点击事件

    }

    private void OnEditButtonClick()
    {
        Debug.Log("点击事件");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
