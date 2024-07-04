using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLookAt : MonoBehaviour
{
    // Start is called before the first frame update
    private Camera cameraToLookAt;

    private void Start()
    {
 
    }
    private void Update()
    {
        cameraToLookAt = Camera.main;
    }
    private void LateUpdate()
    {
        if (cameraToLookAt != null)
        {
            Vector3 direction = transform.position - cameraToLookAt.transform.position;
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = rotation;
        }
    }
}
