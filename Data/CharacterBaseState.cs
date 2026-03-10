using System;
using UnityEngine;

[Serializable]
public class CharacterState
{
    public float maxHp = 1;
    public int atkPower;
    public int defPower;
    public int atkSpeed;
}

[Serializable]
public class MpCharacterState : CharacterState
{
    public float maxMp = 1 ;
}