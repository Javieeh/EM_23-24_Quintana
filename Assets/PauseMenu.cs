using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false; //Bool para comprobar si esta pausado o no

    public GameObject pauseMenuUI; //Canvas

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        { 
            if (GameIsPaused) //Si esta pausado resumimos
            {
                Resume();
            }
            else //pausamos
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        //Desactivamos Canvas
        pauseMenuUI.SetActive(false);
        //Ajustamos el tiempo a la escala normal
        Time.timeScale = 1f;
        //Ponemos en false el booleano
        GameIsPaused = false;
    }

    void Pause()
    {
        //Activamos Canvas
        pauseMenuUI.SetActive(true);
        //Pausamos el tiempo
        Time.timeScale = 0f;
        //Ponemos en true el booleano
        GameIsPaused = true;
    }

    public void LoadMenu() //Funcion para el boton de volver al menu
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // Asegúrate de que tienes una escena llamada "MainMenu"
    }

    
}
