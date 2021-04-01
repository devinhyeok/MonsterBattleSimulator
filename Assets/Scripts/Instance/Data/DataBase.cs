using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO; // 파일 읽기 쓰기 기능 제공

// 모든 열거형
public enum AIState { none, idle, move, attack }
public enum DamageType { normalDamage, trueDamage, manaDamage, IncreaseHp, IncreaseMp }
public enum Filter { unit, equip, battle, use, material, others }
public enum StackType { useStack, useHp }
public enum BuffType { stun, silence, blind, root, ice, invincible, hurt, skillShield, banHealing,
    healthRegenUp1, healthRegenUp2, healthRegenUp3, healthRegenDown1, healthRegenDown2, healthRegenDown3,
    manaRegenUp1, manaRegenUp2, manaRegenUp3, manaRegenDown1, manaRegenDown2, manaRegenDown3,
    attackPowerUp1, attackPowerUp2, attackPowerUp3, attackPowerDown1, attackPowerDown2, attackPowerDown3,
    spellPowerUp1, spellPowerUp2, spellPowerUp3, spellPowerDown1, spellPowerDown2, spellPowerDown3,
    attackArmorUp1, attackArmorUp2, attackArmorUp3, attackArmorDown1, attackArmorDown2, attackArmorDown3,
    spellArmorUp1, spellArmorUp2, spellArmorUp3, spellArmorDown1, spellArmorDown2, spellArmorDown3,
    attackSpeedUp1, attackSpeedUp2, attackSpeedUp3, attackSpeedDown1, attackSpeedDown2, attackSpeedDown3,
    walkSpeedUp1, walkSpeedUp2, walkSpeedUp3, walkSpeedDown1, walkSpeedDown2, walkSpeedDown3,    
}
public enum Shape { circle, box, sector }
public enum AdventureGameModeStat { loading, adventure, battlePlanPhase, battleWaitPhase, battleRunPhase }
public enum Direction { up, down, left, right }
public enum InvetoryCategory { Battle, Collection }

// 스폰 데이터
[System.Serializable]
public class SpawnData
{
    public GameObject spawnObject;
    public Vector3 spawnPosition;

    public SpawnData(GameObject spawnObject, Vector3 spawnPosition)
    {
        this.spawnObject = spawnObject;
        this.spawnPosition = spawnPosition;
    }
}

// 유닛 스텟 구조체
[System.Serializable]
public class UnitStatus
{
    public float health;
    public float mana;
    public float healthRegen;
    public float manaRegen;
    public float attackPower;
    public float spellPower;
    public float attackArmor;
    public float spellArmor;
    public float attackSpeed;
    public float walkSpeed;
    public float attackDistance;
}

// 버프 구조체
[System.Serializable]
public class BuffData
{
    public BuffType buffType;
    public float second;

    public BuffData(BuffType buffType, float second)
    {
        this.buffType = buffType;
        this.second = second;
    }
}

// 데미지 구조체
[System.Serializable]
public class Damage
{
    [HideInInspector]
    public GameObject sourceGameObject;
    public float normalDamage;
    public float trueDamage;
    public float manaDamage;
    public float increaseHp;
    public float increaseMp;
    public float lifeSteal;
    public bool onHit;
    public List<BuffData> buffList;

    public Damage()
    {
        buffList = new List<BuffData>();
    }    
}