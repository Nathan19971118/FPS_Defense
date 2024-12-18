using System.Collections;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public GameObject meshObject;
    public GameObject effectObject;
    public Rigidbody rigid;

    private float explosionRadius = 15f;

    private void Start()
    {
        StartCoroutine(Explosion());
    }

    IEnumerator Explosion()
    {
        yield return new WaitForSeconds(3f);
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        meshObject.SetActive(false);
        effectObject.SetActive(true);

        yield return new WaitForSeconds(3f);
        Destroy(gameObject);

        RaycastHit[] ratHits = Physics.SphereCastAll(transform.position, explosionRadius, Vector3.up, 0f, LayerMask.GetMask("Enemy"));

        foreach (RaycastHit hitObject in ratHits)
        {
            hitObject.transform.GetComponent<Enemy>().HitByGrenade(transform.position);
        }
    }
}
