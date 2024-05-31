using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilCollision : MonoBehaviour
{
    // Start is called before the first frame update

    public Material hitMaterial;
    private float timeHit;
    public int id;

    void Start()
    {
        timeHit = 0.1f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Dado");

            //cogemos el componente de la vida de nuestro player y la reducimos en 1
            other.GetComponentInParent<Player>().life--;

            //Realizamos un cambio de color muy breve para darle feeling a la partida
            // Cambiar el material del objeto del jugador
            Renderer playerRenderer = other.GetComponent<Renderer>();
            if (playerRenderer != null && hitMaterial != null)
            {
                // Almacenar el material original del jugador
                Material originalMaterial = playerRenderer.material;
                Debug.Log("Material original: " + originalMaterial.name);

                // Cambiar el material al material de impacto
                playerRenderer.material = hitMaterial;
                Debug.Log("Material hit: " + hitMaterial.name);

                // Volver al material original después de materialDuration segundos
                StartCoroutine(ResetMaterial(originalMaterial, playerRenderer, other.GetComponent<Player>().life, timeHit));
            }

            //Destruimos la bala para que no siga impactando 0.01 segundos despues para que se pueda realizar la coroutina
            
            Destroy(gameObject, timeHit + 0.01f);
        }
    }

    private System.Collections.IEnumerator ResetMaterial(Material originalMaterial, Renderer renderer, int life, float timeHit)
    {
        yield return new WaitForSeconds(timeHit);

        if (life >= 1) //Excepcion para que no salte error durante la ejecucion debido a que el jugador ya haya sido eliminado
        {
            // Restablecer el material original
            renderer.material = originalMaterial;
        }
    }
}
