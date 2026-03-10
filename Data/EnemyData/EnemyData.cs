using System;
using UnityEngine;


[Serializable]
public class EnemySpawnData
{
    public int pathIndex;
    public int enemyDataID;
    public int enemyLevel;
    public float spawnTime;
}

[Serializable]
public class EnemyData : CSVData
{
    public int id;
    public string controllObjectKey;
    public int enemyLevel;
    public CharacterState characterState;
    public string enemyName;
    //public EnemyController TestObject;

    public override int GetID() => id;
}

public class EnemyDataList : CSVDataList<EnemyData>
{
    
}