using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public float health;
    public Transform target;

    NavMeshAgent nav;

    private void Awake()
    {
        nav = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        nav.SetDestination(target.position);
    }

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
