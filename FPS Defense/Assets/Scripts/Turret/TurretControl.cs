using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretControl : MonoBehaviour
{
    private Transform enemy;
    private float dist;
    public int turretDamage;
    public float fireDist;
    public float bulletVelocity;
    public float fireRate, nextFire;
    public Transform head;
    public Transform barrelLeft;
    public Transform barrelRight;
    public GameObject bullet;
    

    // Start is called before the first frame update
    void Start()
    {
        // Will automatically target the enemy
        enemy = GameObject.FindGameObjectWithTag("Enemy").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (enemy != null)
        {
            // Calculate distance between the enemy and the turret
            dist = Vector3.Distance(enemy.position, transform.position);
            if (dist <= fireDist)
            {
                head.LookAt(enemy);
                if (Time.time >= nextFire)
                {
                    nextFire = Time.time + 1f / fireRate;
                    Shoot(barrelLeft);
                    Shoot(barrelRight);
                }
            }
        }
        else
        {
            return;
        }
    }

    void Shoot(Transform barrel)
    {
        RaycastHit hit;

        if (Physics.Raycast(barrel.transform.position, barrel.transform.forward, out hit, fireDist))
        {
            Debug.Log(hit.transform.name);

            Enemy enemy = hit.transform.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(turretDamage, false);
            }
        }
        
        GameObject bulletInstance = Instantiate(bullet, barrel.position, head.rotation);
        Rigidbody bulletRigid = bulletInstance.GetComponent<Rigidbody>();
        bulletRigid.velocity = head.forward * bulletVelocity;

        Destroy(bulletInstance, 3f);
    }
}
