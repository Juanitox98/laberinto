using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotador : MonoBehaviour
{
    
    void Start()
    {
        
    }

    
    void Update()
    {
       transform.Rotate (new Vector3 (45, 45, 45)* Time.deltaTime);
    }
}
