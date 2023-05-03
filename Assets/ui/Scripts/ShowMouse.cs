using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using deVoid.UIFramework;

public class ShowMouse : APanelController
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.localPosition = Input.mousePosition-new Vector3(1920/2,1080/2,0);
    }
}
