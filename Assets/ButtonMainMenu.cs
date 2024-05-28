using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ButtonMainMenu : MonoBehaviour
{
    public GameObject[] firstCanvas;
    public GameObject[] secondCanvas;
    public GameObject[] creditos;

    void Start()
    {
        
        
    }

    void Update()
    {

    }

    public void OnPointerEnter(PointerEventData eventData)
    {

    }

    public void OnPointerExit(PointerEventData eventData)
    {

    }

    public void ChangeScene(string s)
    {
        SceneManager.LoadScene(s);
    }

    public void OpenCanvas(bool change)
    {
        if(change)
        {
            for (int i = 0; i < firstCanvas.Length; i++)
            {
                firstCanvas[i].SetActive(false);
            }

            for (int i = 0; i < secondCanvas.Length; i++)
            {
                secondCanvas[i].SetActive(true);
            }
        }
        else
        {
            for (int i = 0; i < firstCanvas.Length; i++)
            {
                firstCanvas[i].SetActive(true);
            }

            for (int i = 0; i < secondCanvas.Length; i++)
            {
                secondCanvas[i].SetActive(false);
            }

            for (int i = 0; i < creditos.Length; i++)
            {
                creditos[i].SetActive(false);
            }
        }
        
    }

    public void Creditos()
    {
        for (int i = 0; i < firstCanvas.Length; i++)
        {
            firstCanvas[i].SetActive(false);
        }

        for (int i = 0; i < creditos.Length; i++)
        {
            creditos[i].SetActive(true);
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }

}
