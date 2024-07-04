using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CombatGUI : MonoBehaviour
{
    private TextMeshProUGUI numMuertes;
    private TextMeshProUGUI numDestruidos;

    private int numMuertesTotal;
    private int numDestruidosTotal;

    public Player player;


    void Start()
    {
        numMuertes = this.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        numDestruidos = this.transform.GetChild(3).GetComponent<TextMeshProUGUI>();

        /*Player[] carControllers = FindObjectsOfType<Player>();
        foreach (Player carController in carControllers)
        {
            if (carController.gameObject.GetComponent<NetworkObject>().IsOwner)
            {
                ownPlayer = carController;
            }
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        //numDestruidos.text = player.points.ToString();
        //numMuertes.text = player.numeroMuertes.ToString();
    }
}
