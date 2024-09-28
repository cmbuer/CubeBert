using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthCube : MonoBehaviour
{

    public bool pickedUp;    
    public MeshRenderer renderer;
    

    void Start()
    {    
    }

    private void FixedUpdate()
    {
        if (pickedUp)
        {
            GameObject.Destroy(this.gameObject);
        }
        renderer.material.color = Game.GetHealthStatus();
    }
    private void OnTriggerEnter(Collider other)
    {
        pickedUp = true;
        
    }
    
}
