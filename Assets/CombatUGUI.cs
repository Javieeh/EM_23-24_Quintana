using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CombatGUI : MonoBehaviour
{
    public static CombatGUI Instance;
    public TextMeshProUGUI muertesText;

    private int muertes = 0;

    private void Awake()
    {
        // Asegurar que sólo haya una instancia de UIManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Opcional, si deseas que el UIManager persista entre escenas
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateMuertesText();
    }

    public void IncrementarMuertes()
    {
        muertes++;
        UpdateMuertesText();
    }

    private void UpdateMuertesText()
    {
        if (muertesText != null)
        {
            muertesText.text = "Muertes: " + muertes.ToString();
        }
        else
        {
            Debug.LogError("MuertesText reference is not set in UIManager.");
        }
    }
}
