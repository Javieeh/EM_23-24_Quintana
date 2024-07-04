using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerColor : NetworkBehaviour
{
    [SerializeField] private Renderer bodyRenderer;
    [SerializeField] private Material[] colors;

    private int currentColorIndex = 0;
    public NetworkVariable<Color> playerColor = new NetworkVariable<Color>(Color.white);

    private void Start()
    {
        if (IsClient)
        {
            playerColor.OnValueChanged += OnColorChanged;
        }

        if (IsOwner)
        {
            ChangeColor(); // Cambia el color cuando se instancia el jugador
        }
        UpdateColor();
    }

    private void UpdateColor()
    {
        if (bodyRenderer != null && bodyRenderer.materials.Length > 1)
        {
            Material[] materials = bodyRenderer.materials;
            materials[1].color = playerColor.Value;
            bodyRenderer.materials = materials;
        }
    }

    private void OnDestroy()
    {
        if (IsClient)
        {
            playerColor.OnValueChanged -= OnColorChanged;
        }
    }

    private void OnColorChanged(Color oldColor, Color newColor)
    {
        if (bodyRenderer != null && bodyRenderer.materials.Length > 1)
        {
            Material[] materials = bodyRenderer.materials;
            materials[1].color = newColor;
            bodyRenderer.materials = materials;
        }
    }

    [ServerRpc]
    public void ChangeColorServerRpc(Color newColor)
    {
        playerColor.Value = newColor;
    }

    private void ChangeColor()
    {
        if (colors.Length == 0) return;

        Color newColor = colors[currentColorIndex].color; // Obtiene el color del array
        ChangeColorServerRpc(newColor);
    }

    public void NextColor()
    {
        if (!IsOwner) return; // Solo el propietario puede cambiar el color

        currentColorIndex = (currentColorIndex + 1) % colors.Length;
        ChangeColor();
    }
}