using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Network_Player : MonoBehaviour
{
    
    void Start()
    {
        //creamos un string que recibira la variable de playerName de nuestro GameManager
        string playerName = GameManager.Instance.playerName;
        //Buscamos nuestro hijo con el nombre del jugador (label)
        Transform childName = transform.Find("PlayerName");
        // creamos nuestro texto
        Text label = childName.GetComponentInChildren<Text>();
        //Establecemos el nombre
        label.text = playerName;  
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
