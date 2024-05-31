using UnityEngine;
using System.Collections.Generic;

public class CarProperties : MonoBehaviour
{
    public string carName;
    public Color carColor;

    private static List<CarProperties> carInstances = new List<CarProperties>();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        carInstances.Add(this);
    }

    private void OnDestroy()
    {
        carInstances.Remove(this);
    }

    public static void SaveProperties()
    {
        foreach (var car in carInstances)
        {
            PlayerPrefs.SetString(car.name + "_Name", car.carName);
            PlayerPrefs.SetString(car.name + "_Color", ColorUtility.ToHtmlStringRGBA(car.carColor));
        }
    }

    public static void LoadProperties()
    {
        foreach (var car in carInstances)
        {
            if (PlayerPrefs.HasKey(car.name + "_Name"))
            {
                car.carName = PlayerPrefs.GetString(car.name + "_Name");
                Color color;
                if (ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString(car.name + "_Color"), out color))
                {
                    car.carColor = color;
                }
            }
        }
    }
}
