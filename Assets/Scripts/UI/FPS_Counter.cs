using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPS_Counter : MonoBehaviour
{

    public float RefreshRate = 0.5f;

    public Text Text;

    // Start is called before the first frame update
    void Start()
    {
        if(!Text)
        {
            Text = GetComponent<Text>();
        }
        StartCoroutine(UpdateFPS());
    }

    IEnumerator UpdateFPS()
    {
        while (true)
        {
            if (!Text)
                yield return null;
            Text.text = "FPS: " + (1f / Time.unscaledDeltaTime).ToString("0.0");
            yield return new WaitForSeconds(RefreshRate);
        }
    }

}
