using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO; // 파일 읽기 쓰기 기능 제공

// 모든 열거형
public enum AIState { none, idle, move, attack }
public enum DamageType { normalDamage, trueDamage, IncreaseHp, IncreaseMp }
public enum Filter { unit, equip, battle, use, material, others }
public enum StackType { useStack, useHp }
public enum BuffType { stun, silence, blind, root, ice, invincible, hurt, skillShield, banHealing, 
    attackUp1, attackUp2, attackUp3, attackDown1, attackDown2, attackDown3,
    attackSpeedUp1, attackSpeedUp2, attackSpeedUp3, attackSpeedDown1, attackSpeedDown2, attackSpeedDown3,
    abilityPowerUp1, abilityPowerUp2, abilityPowerUp3, abilityPowerDown1, abilityPowerDown2, abilityPowerDown3,
    manaRegenUp1, manaRegenUp2, manaRegenUp3, manaRegenDown1, manaRegenDown2, manaRegenDown3,
    defenseUp1, defenseUp2, defenseUp3, defenseDown1, defenseDown2, defenseDown3,
    walkSpeedUp1, walkSpeedUp2, walkSpeedUp3, walkSpeedDown1, walkSpeedDown2, walkSpeedDown3,
    healthRegenUp1, healthRegenUp2, healthRegenUp3, healthRegenDown1, healthRegenDown2, healthRegenDown3
}
public enum Shape { circle, box, sector }
public enum AdventureGameModeStat { loading, adventure, battlePlanPhase, battleWaitPhase, battleRunPhase }
public enum Direction { up, down, left, right }

// 데미지 클라스
public class Damage
{
    public GameObject sourceGameObject;
    public float normalDamage = 0;
    public float trueDamage = 0;
    public float increaseHp = 0;
    public float increaseMp = 0;
    public Dictionary<BuffType, float> buffSecondDictionary = new Dictionary<BuffType, float>();
    public bool onHit = true;
}

public class DataBase : MonoBehaviour
{

}
