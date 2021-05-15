using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Adam McWilliams
//contact adammcw01@gmail.com

public class DisplayXY : MonoBehaviour
{

    private Text lbl;

    public Slider slider;

    // Start is called before the first frame update
    void Start()
    {
        lbl = GetComponent<Text>();
        ShowSliderValue();
    }

    public void ShowSliderValue()
    {
        string sliderValue = slider.value.ToString();
        lbl.text = sliderValue;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
