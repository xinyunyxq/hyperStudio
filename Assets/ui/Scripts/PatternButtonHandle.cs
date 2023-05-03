using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RuntimeGizmos;

public class PatternButtonHandle : MonoBehaviour
{
    [SerializeField] private Sprite VRPattern;
    [SerializeField] private Sprite PCPattern;
    private Button button;
    private Image image;
    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            Debug.Log("点击事件");
            Config.instance.CursorSpaceMoveEdit = !Config.instance.CursorSpaceMoveEdit;
            if (Config.instance.CursorSpaceMoveEdit)
            {
                image.sprite = VRPattern;
                TransformGizmo gizmo = Camera.main.GetComponent<TransformGizmo>();
                gizmo.enabled = true;
            }
            else
            {
                image.sprite = PCPattern;
                TransformGizmo gizmo = Camera.main.GetComponent<TransformGizmo>();
                gizmo.enabled = false;
            }
        })
            ;//监听点击事件
        image = transform.Find("Image").GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
