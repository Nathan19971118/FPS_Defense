using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public Transform[] enemyZones;
    public GameObject[] enemies;
    public List<int> enemyList;
    private int wave;

    public GameObject tent;

    private void Awake()
    {
        enemyList = new List<int>();
    }

    // Start is called before the first frame update
    void Start()
    {
        wave = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void WaveStart()
    {
        StartCoroutine(InBattle());
    }

    IEnumerator InBattle()
    {
        for(int index = 0; index < wave ; index++)
        {
            int ran = Random.Range(0, enemies.Length);
            enemyList.Add(ran);
        }

        while (enemyList.Count > 0)
        {
            int ranZone = Random.Range(0, 8);
            GameObject instantEnemy = Instantiate(enemies[enemyList[0]], enemyZones[ranZone].position, enemyZones[ranZone].rotation);
            Enemy enemy = instantEnemy.GetComponent<Enemy>();
            enemy.target = tent.transform;
            enemyList.RemoveAt(0);
            yield return new WaitForSeconds(3f);
        }
    }
}
