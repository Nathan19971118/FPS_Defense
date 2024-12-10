using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float health;

    public void TakeDamage(float amount, bool isGrenade)
    {
        health -= amount;

        if (health <= 0)
        {
            Die();
        }
    }

    public void HitByGrenade(Vector3 explosionPosition)
    {
        health -= 100f;
        Vector3 reactVec = transform.position = explosionPosition;
        TakeDamage(health, true);
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
