using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum Type { A, B, C, D }
    public Type enemyType;
    public int maxHealth;
    public int currentHealth;
    public int point;
    public bool isDead;

    private void Awake()
    {

    }
    private void Start()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        
    }

    public void TakeDamage(int amount, bool isGrenade)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void HitByGrenade(Vector3 explosionPosition)
    {
        currentHealth -= 100;
        Vector3 reactVec = transform.position = explosionPosition;
        TakeDamage(currentHealth, true);
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
