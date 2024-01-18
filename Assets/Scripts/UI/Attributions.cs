using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Attributions : MonoBehaviour
{

    public TextAsset AttributionsTextSource;

    public Text AttributionsText;

    public Button AttributionsButton;

    // Start is called before the first frame update
    void Start()
    {
        AttributionsText.text = AttributionsTextSource.text;
    }

    public void ToggleAttributions()
    {
        bool active = AttributionsText.gameObject.activeSelf;
        if (active)
        {
            AttributionsText.gameObject.SetActive(false);
        }
        else
        {
            AttributionsText.gameObject.SetActive(true);
        }
    }
}
