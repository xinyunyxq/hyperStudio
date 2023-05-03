using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResetViewButtonHandle : MonoBehaviour
{
    private Button button;
    bool resetView = false;
    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            resetView = true;
            StartCoroutine(EndResetView());
        });
    }

    // Update is called once per frame
    void Update()
    {
        if(resetView)
        {
            Debug.Log("reset");
            XRSDK.Reset();
        }
    }

    IEnumerator EndResetView()
    {
        Debug.Log("3√Î∫Ûπÿ±’");
        yield return new WaitForSeconds(3);
        resetView = false;
    }
}
