using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    public float lifeTime = 1f;

    private void Awake()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            Destroy(gameObject);
        }
    }
}
