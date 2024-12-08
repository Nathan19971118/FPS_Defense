using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    public float lifeTime = 1f;
    public int damage;

    private void Awake()
    {
        Destroy(gameObject, lifeTime);
    }
}
