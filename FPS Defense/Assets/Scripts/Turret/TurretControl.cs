using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretControl : MonoBehaviour
{
    private Transform enemy;
    private float dist;
    public int damage;
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
                    Shoot();
                }
            }
        }
        else
        {
            return;
        }
    }

    void Shoot()
    {
        RaycastHit hit;

        if (Physics.Raycast(barrelLeft.transform.position, barrelLeft.transform.forward, out hit, dist))
        {
            Debug.Log(hit.transform.name);

            Enemy enemy = hit.transform.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage, false);
            }
        }

        if (Physics.Raycast(barrelRight.transform.position, barrelRight.transform.forward, out hit, dist))
        {
            Debug.Log(hit.transform.name);

            Enemy enemy = hit.transform.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage, false);
            }
        }

        GameObject bulletLeft = Instantiate(bullet, barrelLeft.position, head.rotation);
        GameObject bulletRight = Instantiate(bullet, barrelRight.position, head.rotation);

        Rigidbody bulletRigidLeft = bulletLeft.GetComponent<Rigidbody>();
        Rigidbody bulletRigidRight = bulletRight.GetComponent<Rigidbody>();

        bulletRigidLeft.velocity = head.forward * bulletVelocity;
        bulletRigidRight.velocity = head.forward * bulletVelocity;
    }

    /*
    private void Shoot()
    {
        RaycastHit hit;

        if (Physics.Raycast(bulletSpawn.transform.position, bulletSpawn.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name);

            Enemy enemy = hit.transform.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage, true);
            }
        }

        GameObject instantBullet = Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);
        Rigidbody bulletRigid = instantBullet.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletSpawn.forward * bulletVelocity;
    }
    */
}
