/* Copyright (C) 2024 Christopher Buer */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeBert : MonoBehaviour
{
    public int maxHealth;
    public int health;
    public AudioSource hitAudio;
    public AudioSource healAudio;

    private void Awake()
    {
        health = maxHealth;
    }

    public int Damage(int amount)
    {
        health -= amount;
        hitAudio.Play();
        return health;
    }
    public int Heal (int amount)
    {
        int newHealth = health + amount;
        health = (newHealth > maxHealth) ? maxHealth : newHealth;        
        healAudio.Play();
        return health;
    }
    public int GetHealth()
    {
        return health;
    }

    public void Kill()
    {
        health = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        Game.GetInstance().HandleCubertCollision(this, other);
    }
}