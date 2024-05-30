using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LapTimeController : MonoBehaviour
{
    public TextMeshProUGUI lapTimeText; // Texto tiempo de la vuelta actual
    public TextMeshProUGUI totalTimeText; // Texto tiempo total
    private float lapStartTime; // Tiempo de inicio de la vuelta
    private float totalStartTime; // Tiempo de inicio total

    private void Start()
    {
        lapStartTime = Time.time;
        totalStartTime = Time.time;
    }
    private void Update()
    {
        // Calculo tiempo vuelta actual y total
        float lapTime = Time.time - lapStartTime;
        float totalTime = Time.time - totalStartTime;
        //formateamos los tiempos para representarlos en minutos y seg
        string lampTimeForm = FormatTime(lapTime);
        string totalTimeForm = FormatTime(totalTime);
        // Una vez formateados, actualizamos el valor en la UI
        lapTimeText.text = "Lap Time: " + lampTimeForm;
        totalTimeText.text = "Total Time: " + totalTimeForm;
    }
    // Método para reiniciar el tiempo de vuelta (llamar cuando empieza una nueva vuelta)
    public void StartNewLap()
    {
        lapStartTime = Time.time;
    }
    private string FormatTime(float time) // Funcion auxiliar para formatear el tiempo
    {
        int mins = Mathf.FloorToInt(time / 60f);
        int secs = Mathf.FloorToInt(time % 60f);
        int miliSecs = Mathf.FloorToInt((time * 100f) % 100f);
        return $"{mins}:{secs}:{miliSecs}";
    }
}
