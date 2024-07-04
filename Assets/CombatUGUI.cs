using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CombatGUI : MonoBehaviour
{
    public static CombatGUI Instance;
    public TextMeshProUGUI muertesText;
    public TextMeshProUGUI destruccionesText;
    public TextMeshProUGUI cooldownText; // Añadir referencia al texto de cooldown

    private int muertes = 0;
    private int destrucciones = 0;

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
        UpdateDestruccionesText();
        if (cooldownText != null)
        {
            cooldownText.enabled = false; // Asegurarse de que el texto esté apagado inicialmente
        }
    }

    public void IncrementarMuertes()
    {
        muertes++;
        UpdateMuertesText();
    }

    public void IncrementarDestrucciones()
    {
        destrucciones++;
        UpdateDestruccionesText();
    }

    private void UpdateMuertesText()
    {
        if (muertesText != null)
        {
            muertesText.text = muertes.ToString();
        }
        else
        {
            Debug.LogError("MuertesText reference is not set in UIManager.");
        }
    }

    private void UpdateDestruccionesText()
    {
        if (destruccionesText != null)
        {
            destruccionesText.text = destrucciones.ToString();
        }
        else
        {
            Debug.LogError("DestruccionesText reference is not set in UIManager.");
        }
    }

    public void ShowCooldownText(int seconds)
    {
        if (cooldownText != null)
        {
            cooldownText.enabled = true;
            cooldownText.text = $"Respawning in {seconds} seconds...";
        }
    }

    public void HideCooldownText()
    {
        if (cooldownText != null)
        {
            cooldownText.enabled = false;
        }
    }
}
