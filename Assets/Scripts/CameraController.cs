using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    
    private readonly Vector3 _pLookOffset = new Vector3(0, 5.0F, -5.0F);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Update is called last activate
    private void LateUpdate()
    {
        transform.position = target.position + _pLookOffset;
        transform.LookAt(target);
    }
}
