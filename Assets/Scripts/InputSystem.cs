using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystem : MonoBehaviour
{
    public static InputSystem Instance { get; private set; }
    [SerializeField] private Player player;

    public Player Player
    {
        get => player;
        set
        {
            player = value;
            SetPlayer(player);
        }
    }

    public InputAction Move;
    public InputAction Brake;
    public InputAction Attack;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (player)
        {
            SetPlayer(player);
        }
    }

    private void SetPlayer(Player player)
    {
        InputController input = player.GetComponent<InputController>();
        if (input == null)
        {
            Debug.LogError("InputController no encontrado en el Player.");
            return;
        }

        Move.performed += input.OnMove;
        Move.Enable();

        Brake.performed += input.OnBrake;
        Brake.Enable();

        Attack.performed += input.OnAttack; // Corregido para asignar OnAttack
        Attack.Enable();
    }
}
