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
        button_edit = GameObject.Find("Button_edit").GetComponent<Button>();//ͨ��Find�������ƻ������Ҫ��Button���
        button_edit.onClick.AddListener(OnEditButtonClick);//��������¼�

    }

    private void OnEditButtonClick()
    {
        Debug.Log("����¼�");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
