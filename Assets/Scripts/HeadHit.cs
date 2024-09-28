using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadHit : MonoBehaviour
{
    public AudioSource hitSound;
    public AudioSource dieSound;
    int health;

    GameObject parent;

    // Start is called before the first frame update
    void Start()
    {
        health = 100;
    }
   
    void FixedUpdate()
    {
        if (health <= 0)
        {
            dieSound.Play();            
            GameObject.Destroy(this.gameObject.transform.parent.gameObject);
        }
        
    }
   
    private void OnTriggerEnter(Collider other)
    {        
        health -= 25;        
        hitSound.Play();
        if (health <= 0)
            dieSound.Play();
    }
}
