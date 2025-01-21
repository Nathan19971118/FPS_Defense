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

    public Transform target;
    Rigidbody rb;
    BoxCollider boxCollider;
    Material mat;
    NavMeshAgent nav;
    Animator anim;

    public bool isChase;
    public bool isDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        mat = GetComponentInChildren<MeshRenderer>().material;
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        Invoke("ChaseStart", 2);
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    private void ChaseStart()
    {
        isChase = true;
        anim.SetBool("isWalk", true);
    }

    private void Update()
    {
        if (isChase)
            nav.SetDestination(target.position);
    }

    private void FixedUpdate()
    {
        FreezeRotation();
    }

    private void FreezeRotation()
    {
        if (isChase)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void TakeDamage(int amount, bool isGrenade)
    {
        currentHealth -= amount;

        StartCoroutine("OnDamage");
    }

    IEnumerator OnDamage()
    {
        mat.color = Color.red;
        yield return new WaitForSeconds(0.1f);

        if (currentHealth > 0)
        {
            mat.color= Color.white;
        }
        else
        {
            mat.color= Color.red;
            anim.SetTrigger("doDie");
            isChase = false;
            nav.enabled = false;
            Destroy(gameObject, 4);
        }
    }

    public void HitByGrenade(Vector3 explosionPosition)
    {
        currentHealth -= 100;
        Vector3 reactVec = transform.position = explosionPosition;
        TakeDamage(currentHealth, true);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Target")
        {
            Destroy(gameObject);
        }
    }
}
