using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CountdownText : MonoBehaviour
{
    private int iteration;
    private TextMeshProUGUI textCD;
    private GameObject startUI;

    private void Start()
    {
        startUI = GetComponentInParent<GameObject>();
        iteration = 5;
        textCD = GetComponent<TextMeshProUGUI>();
        textCD.text = iteration.ToString();
    }
    public bool TryDecrement()
    {
        if (iteration == 1){
            startUI.SetActive(false);
            return false;
        }
        iteration--;
        textCD.text = iteration.ToString();
        return true;
    }
}
