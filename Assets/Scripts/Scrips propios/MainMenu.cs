using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button findGame;

    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject lobbyMenu;

    private void Awake()
    {
        findGame.onClick.AddListener(() =>
        {

        }
        );
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
