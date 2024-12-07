using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public MeshRenderer[] meshs;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>();
            //curHealth -= bullet.damage;
            //Vector3 reactVec = transform.position - other.transform.position;
            //Destroy(other.gameObject);

            StartCoroutine(OnDamage());
        }
    }

    IEnumerator OnDamage()
    {
        foreach (MeshRenderer mesh in meshs)
            mesh.material.color = Color.red;

        yield return new WaitForSeconds(0.1f);
    }
}
