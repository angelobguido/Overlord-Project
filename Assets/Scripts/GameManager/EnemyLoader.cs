using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnemyGenerator;
using System.Linq;

public class EnemyLoader : MonoBehaviour
{
    [SerializeField]
    public EnemySO[] bestEnemies;
    public GameObject enemyPrefab;

    void Start()
    {
    }

    public void LoadEnemies()
    {
        bestEnemies = Resources.LoadAll("Enemies", typeof(EnemySO)).Cast<EnemySO>().ToArray();
        ApplyDelegates();
    }

    public GameObject InstantiateEnemyWithIndex(int index, Vector3 position, Quaternion rotation)
    {
        Debug.Log("Begin instantiating");
        GameObject enemy;

        enemy = Instantiate(enemyPrefab, position, rotation);

        enemy.GetComponent<EnemyController>().LoadEnemyData(bestEnemies[index], index);

        return enemy;
    }
    private void ApplyDelegates()
    {
        foreach (EnemySO enemy in bestEnemies)
        {
            enemy.movement.movementType = GetMovementType(enemy.movement.enemyMovementIndex);
        }
    }
    public MovementType GetMovementType(MovementEnum moveTypeEnum)
    {
        switch (moveTypeEnum)
        {
            case MovementEnum.None:
                return EnemyMovement.NoMovement;
            case MovementEnum.Random:
                return EnemyMovement.MoveRandomly;
            case MovementEnum.Flee:
                return EnemyMovement.FleeFromPlayer;
            case MovementEnum.Follow:
                return EnemyMovement.FollowPlayer;
            default:
                Debug.Log("No Movement Attached to Enemy");
                return null;
        }
    }
}
