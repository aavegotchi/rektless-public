using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTransformMatch : MonoBehaviour
{
    Transform camTransform;
    Transform CamTransform
    {
        get
        {
            if (camTransform == null)
                camTransform = Camera.main.transform;
            return camTransform;
        }
    }

    public Vector3 offset = new();


    void LateUpdate()
    {
        transform.position = CamTransform.position + offset; 
    }
}
