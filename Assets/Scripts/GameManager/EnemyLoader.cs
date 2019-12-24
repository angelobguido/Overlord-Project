using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnemyGenerator;

public class EnemyLoader : MonoBehaviour
{
    private EnemySO[] bestEnemies;
    private GameObject EnemyPrefab;

    void Start()
    {
        bestEnemies = (EnemySO[])Resources.LoadAll("Enemies", typeof(EnemySO));
    }

    public GameObject InstantiateEnemyWithIndex(int index, Vector3 position, Quaternion rotation)
    {
        GameObject enemy;

        enemy = Instantiate(EnemyPrefab, position, rotation);

        enemy.GetComponent<EnemyController>().LoadEnemyData(bestEnemies[index]);

        return enemy;
    }
}
