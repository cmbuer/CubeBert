/* Copyright (C) 2024 Christopher Buer */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBolt : MonoBehaviour
{
    private Rigidbody body;
    private bool hasImpacted;
    private float timeAlive;

    private void Awake()
    {
        body = transform.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        timeAlive += Time.deltaTime;
        if (hasImpacted || (timeAlive > 5))
        {
            GameObject.Destroy(this.gameObject);
        }
    }

    public void FireAt(Vector3 target)
    {
        transform.LookAt(target, Vector3.left);
        transform.position += (transform.forward * 1.0f);
        body.velocity = transform.forward * 20;
    }

    private void OnTriggerEnter(Collider other)
    {
        hasImpacted = true;
        // TODO: hit effect
    }
}
