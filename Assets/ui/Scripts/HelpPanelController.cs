using deVoid.UIFramework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HelpPanelController : APanelController
{
    // Start is called before the first frame update
    private TMP_Text helpText;
    void Start()
    {
        helpText = transform.Find("helpText").GetComponent<TMP_Text>();
        helpText.text = @"Hold `Tab` to show this help.
Press `Enter` to toggle full screen.

Hold `Ctrl + '+'/'-'` to bend the screen.
Press `Ctrl + 0` to toggle bend.

Press `Ctrl + R` to reset viewpoint direction.
Press `Ctrl + Shift + R` to reload the scene.
Press `Ctrl + S` to save screen's location, rotation and scale to config.
Hold `Ctrl + V` to record view zone for the screen.
Press `Ctrl + Shift + V` to disable view zone for the screen.
Hold `Ctrl + A` to show all screens(not include removed).
Press `Ctrl + F` to toggle `AutoLookAtCamera`.
Press `ESC` to exit.";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
