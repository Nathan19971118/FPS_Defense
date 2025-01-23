using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public enum SpawnState { SPAWNING, WAITING, COUNTING};

    [System.Serializable]
    public class Wave
    {
        public string name;
        public Transform[] enemies;
        public int count;
        public float rate;
    }

    public Wave[] waves;
    private int nextWave = 0;

    public float timeBetweenWaves = 5f;
    public float waveCountdown;

    private SpawnState state = SpawnState.COUNTING;

    public Transform[] enemyZones;
    public GameObject[] enemies;
    public List<int> enemyList;
    private int wave;

    public GameObject tent;

    private void Awake()
    {
        //enemyList = new List<int>();
    }

    // Start is called before the first frame update
    void Start()
    {
        waveCountdown = timeBetweenWaves;

        enemyZones = new Transform[enemyZones.Length];

        for(int i = 0;i < enemyZones.Length; i++)
        {
            enemyZones[i] = gameObject.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (waveCountdown <= 0)
        {
            if (state != SpawnState.SPAWNING)
            {
                StartCoroutine(SpawnWave(waves[nextWave]));
            }
        }
        else
        {
            waveCountdown -= Time.deltaTime;
        }
    }

    IEnumerator SpawnWave(Wave _wave)
    {
        state = SpawnState.SPAWNING;

        // Spawn

        state = SpawnState.WAITING;

        yield break;
    }


    private void SpawnEnemy()
    {
        // Selecting a random enemy
        int randomEnemy=Random.Range(0,enemies.Length);
        Transform spawnPoints = enemyZones[Random.Range(0,enemyZones.Length)];
        Instantiate(enemies[randomEnemy], spawnPoints.position, spawnPoints.rotation);
    }

    

    /*
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
    */
}
