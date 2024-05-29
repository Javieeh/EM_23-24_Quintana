using UnityEngine;

public class RespawnInfo : MonoBehaviour
{
    public Transform respawnPoint;

    private void OnDrawGizmos()
    {
        if (respawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(respawnPoint.position, 0.5f);
            Gizmos.DrawLine(respawnPoint.position, respawnPoint.position + respawnPoint.forward * 2);
        }
    }
}