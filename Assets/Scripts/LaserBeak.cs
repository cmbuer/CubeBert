/* Copyright (C) 2024 Christopher Buer */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeak : MonoBehaviour
{
    public float laserFireRate = 1;
    public float laserTargetRange = 10;
    public AudioSource laserAudio;
    public GameObject laserBoltPrefab;
    public Transform emitterLocation;

    private float laserCooldownTimer;

    private void Update()
    {
        laserCooldownTimer += Time.deltaTime;
        Vector3 targetPosition = Game.GetInstance().GetCubertTargetPosition();
        Vector3 targetOffset = targetPosition - transform.position;
        if (targetOffset.magnitude < laserTargetRange)
        {
            // Even if we can't shoot, always turn to face the player when in range
            Vector3 lookPosition = targetPosition;
            lookPosition.y = transform.position.y; // don't look up/down the Y axis
            transform.LookAt(lookPosition, Vector3.up);
            if (laserCooldownTimer > laserFireRate)
            {
                // Fire a laser bolt if cooldown has been reached
                GameObject laser = GameObject.Instantiate(laserBoltPrefab);
                laser.transform.position = emitterLocation.position;
                LaserBolt bolt = laser.GetComponent<LaserBolt>();
                bolt.FireAt(targetPosition);
                laserCooldownTimer = 0;
                laserAudio.Play();
            }
        }
    }
}
