using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class Unit : MonoBehaviour
{
    /// ---------------------------------------- 선언 ---------------------------------------------------- ///
    [Header("편집값")]
    public string key;
    public int team; // 팀 이름
    [SerializeField]
    private int level = 1; // 현재 레벨값
    public int Level
    {
        get { return level; }
        set
        {
            level = value;
            Init();
        }
    }
    public UnitData unitData;
    public bool noSkill;
    Color enemyColor = new Color32(200, 0, 0, 255);
    Color friendColor = new Color32(0, 200, 0, 255);

    [Header("참조값")]
    [HideInInspector]
    public List<GameObject> enemyInAttackDistance = new List<GameObject>(); // 공격 사거리에 있는 오브젝트 리스트
    [HideInInspector]
    public new Rigidbody2D rigidbody;
    protected SpriteRenderer spriteRenderer;
    protected Animator animator;
    protected RectTransform canvas;
    protected Image healthBar;
    protected Image mpBar;
    protected GameObject damageText;
    protected Stack<GameObject> damageTextPool = new Stack<GameObject>(); // 플로팅 데미지 오브젝트 풀


    [Header("디버그 설정")]
    public bool testSkill;

    [Header("현재 상태")]
    // 유닛 상태
    public bool isDead = false; // 사망여부    
    public AIState aiState = AIState.none;
    [SerializeField]
    protected Unit target;
    [SerializeField]
    protected Vector3 movePoint;
    [SerializeField]
    protected Vector3 direction = new Vector2(-1, 0); // 보는 방향
    [SerializeField]
    protected bool isAction = false;
    [SerializeField]
    protected bool isRigid = false;

    [Header("기본 스텟")]
    // 유닛 기본 스텟
    public float baseHealth;
    public float baseMana;
    public float baseHealthRegan;
    public float baseManaRegan;
    public float baseAttackPower;
    public float baseSpellPower;
    public float baseAttackArmor;
    public float baseSpellArmor;
    public float baseAttackSpeed;
    public float baseWalkSpeed;
    public float baseAttackDistance;

    [Header("현재 스텟")]
    // 유닛 전투 스텟 정보    
    public float maxHealth;
    [SerializeField]
    private float currentHealth;
    public float maxMana;
    [SerializeField]
    private float currentMana;
    public float currentHealthRegen;
    public float currentManaRegen;
    public float currentAttackPower;
    public float currentSpellPower;
    public float currentAttackArmor;
    public float currentSpellArmor;
    [SerializeField]
    private float currentAttackSpeed;
    public float currentWalkSpeed;
    public float currentAttackDistance;
        
    // 버프
    public Dictionary<BuffType, Buff> buffDictionary = new Dictionary<BuffType, Buff>(); // 버프 딕셔너리
    Damage bumpDamage;

    /// ---------------------------------------- 프로퍼티 ---------------------------------------------------- ///
    public float CurrentHealth
    {
        get
        {
            return currentHealth;
        }
        set
        {
            currentHealth = Mathf.Clamp(value, 0, maxHealth);
            if (currentHealth <= 0)
                StartCoroutine(PlayDeadAnim());
        }
    }
    public float CurrentMana
    {
        get
        {
            return currentMana;
        }
        set
        {
            if (noSkill)
                currentMana = 0;
            else
                currentMana = Mathf.Clamp(value, 0, maxMana);
        }
    }
    public float CurrentAttackSpeed
    {
        get
        {
            return currentAttackSpeed;
        }
        set
        {
            currentAttackSpeed = Mathf.Clamp(value, 0, float.MaxValue);
            if (currentAttackSpeed > 100)
            {
                animator.SetFloat("AttackSpeed", currentAttackSpeed / 100);
            }
            else
            {
                animator.SetFloat("AttackSpeed", 1);
            }
        }
    }

    /// ---------------------------------------- 유니티 이벤트 ---------------------------------------------------- ///
    public virtual void Awake()
    {
        // 외부 스크립트  가져오기
        GetPointers();

        // 초기 설정
        Init();
        InitDamageTextPool();
    }

    public virtual void Start()
    {
        // 팀에 따라 HpBar 색상 바꾸기
        if (AdventureModeManager.Instance)
        {
            if (team == AdventureModeManager.Instance.playerController.team)
                healthBar.color = friendColor;
            else
                healthBar.color = enemyColor;
        }
        else
        {
            if (team == 0)
                healthBar.color = friendColor;
            else
                healthBar.color = enemyColor;
        }
        if (testSkill)
        {
            CurrentMana = maxMana;
        }
    }

    public virtual void Update()
    {
        // 상태바 업데이트
        healthBar.fillAmount = CurrentHealth / maxHealth;
        mpBar.fillAmount = CurrentMana / maxMana;

        // 방향에 따라 스프라이트 방향 맞추기
        if (direction.x <= 0)
        {
            spriteRenderer.flipX = false;
        }
        else
        {
            spriteRenderer.flipX = true;
        }

        // 이동상태면 이동 애니메이션 재생
        if (aiState == AIState.move)
        {
            animator.SetBool("Walk", true);
        }
        else
        {
            animator.SetBool("Walk", false);
        }

        CheckRigid();

    }

    public virtual void FixedUpdate()
    {
        // 사망시 검사하지 않음
        if (isDead)
            return;

        // 사정거리 안에 유닛이 존재하는지 체크
        CheckEnenyInAttackDistance();

        // 스테이터스 업데이트
        UpdateStatus();

        // 버프 검사
        CheckBuff();

        // AI가 켜져있는가?
        if (aiState != AIState.none)
        {
            RunBehaviorTree();
        }
    }

    public virtual void OnCollisionEnter2D(Collision2D collision)
    {
        // 사망시 검사하지 않음
        if (isDead)
            return;
        CheckBump(collision);
    }

    /// ---------------------------------------- 초기화 ---------------------------------------------------- ///
    public void GetPointers()
    {
        // 값을 참조할 컴포넌트 주소 가져오기
        spriteRenderer = transform.Find("Sprite").GetComponent<SpriteRenderer>();
        animator = transform.Find("Sprite").GetComponent<Animator>();
        canvas = transform.Find("Canvas").GetComponent<RectTransform>();
        healthBar = transform.Find("Canvas").Find("HpBorder").Find("HpBar").GetComponent<Image>();
        mpBar = transform.Find("Canvas").Find("MpBorder").Find("MpBar").GetComponent<Image>();
        rigidbody = GetComponent<Rigidbody2D>();

        // 폴더에서 프리팹 가져오기
        damageText = (GameObject)Resources.Load("Prefabs/UI/DamageTextUI");

        // <버프 타입, 버프 게임 오브젝트> 리스트 만들기: BuffType(첫글자 소문자)와 프리팹(첫글자 대문자) 이름 같게 지을것!  
        List<BuffType> buffTypeList = new List<BuffType>();
        List<GameObject> gameObjectList = new List<GameObject>();

        foreach (BuffType buffType in Enum.GetValues(typeof(BuffType)))
        {
            buffTypeList.Add(buffType);
            gameObjectList.Add(buffType.ToString().GetBuffPrefab());
        }

        // 버프 게임 오브젝트 생성후 주소 저장하기 
        int index = 0;
        foreach (BuffType buffType in buffTypeList)
        {
            GameObject buffGameObject = Instantiate(gameObjectList[index], transform);
            buffGameObject.GetComponent<Buff>().parentGameObject = gameObject;
            buffGameObject.SetActive(false);
            buffDictionary.Add(buffTypeList[index], buffGameObject.GetComponent<Buff>());
            index++;
        }
    }

    public void Init()
    {
        // 기본 스텟 설정
        baseHealth = unitData.statusList[0].health * Mathf.Pow(1.1f, Level - 1);
        baseMana = unitData.statusList[0].mana;
        baseHealthRegan = unitData.statusList[0].healthRegen;
        baseManaRegan = unitData.statusList[0].manaRegen;
        baseAttackPower = unitData.statusList[0].attackPower * Mathf.Pow(1.1f, Level - 1);
        baseSpellPower = unitData.statusList[0].spellPower * Mathf.Pow(1.1f, Level - 1);
        baseAttackArmor = unitData.statusList[0].attackArmor;
        baseSpellArmor = unitData.statusList[0].spellArmor;
        baseAttackSpeed = unitData.statusList[0].attackSpeed;
        baseWalkSpeed = unitData.statusList[0].walkSpeed;
        baseAttackDistance = unitData.statusList[0].attackDistance;

        // 스텟 초기화       
        maxHealth = baseHealth;
        CurrentHealth = maxHealth;
        maxMana = baseMana;
        CurrentMana = 0;
        currentHealthRegen = baseHealthRegan;
        currentManaRegen = baseManaRegan;
        currentAttackPower = baseAttackArmor;
        currentSpellPower = baseSpellPower;
        currentAttackArmor = baseAttackArmor;
        currentSpellArmor = baseSpellArmor;
        CurrentAttackSpeed = baseAttackSpeed;
        currentWalkSpeed = baseWalkSpeed;
        currentAttackDistance = baseAttackDistance;

        // 그래픽 초기화
        canvas.gameObject.SetActive(true);
        spriteRenderer.sortingLayerName = "GameObject";
    }

    /// ---------------------------------------- 애니메이션 이벤트 ---------------------------------------------------- ///
    // 공격 모션 재생
    public virtual IEnumerator PlayAttackAnim(float cooltime)
    {
        // 타겟이 있으면 모션 재생
        if (target)
        {
            direction = (target.transform.position - transform.position).normalized;
        }
        animator.SetTrigger("Attack");
        isAction = true;
        yield return new WaitForSeconds(cooltime);
        isAction = false;
    }

    // 스킬 모션 재생
    public virtual IEnumerator PlaySkillAnim(float cooltime)
    {
        // 스킬이 없으면 대신 공격 애니메이션 실행
        if (noSkill)
        {
            StartCoroutine(PlayAttackAnim(cooltime));
            yield return null;
        }

        // 타겟이 있으면 스킬 모션 재생
        if (target)
        {
            direction = (target.transform.position - transform.position).normalized;
        }
        animator.SetTrigger("UseSkill");
        isAction = true;
        yield return new WaitForSeconds(cooltime);
        isAction = false;
    }

    // 사망 애니메이션 재생
    IEnumerator PlayDeadAnim()
    {
        // 초기화
        buffDictionary = new Dictionary<BuffType, Buff>();
        isDead = true; // 사망여부       
        aiState = AIState.none;
        target = null;
        movePoint = Vector2.zero;
        isAction = false;
        bumpDamage = null;
        isRigid = false;
        rigidbody.velocity = Vector2.zero;

        // 사망 처리
        animator.SetBool("Dead", true);
        canvas.gameObject.SetActive(false);
        ReleaseAggro();

        // 사망 이벤트 발생
        float animTime = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animTime);
        yield return new WaitForSeconds(0.2f);
        while (spriteRenderer.color.a > 0)
        {
            spriteRenderer.color = new Color(1f, 1f, 1f, spriteRenderer.color.a - 0.01f);
            yield return new WaitForEndOfFrame();
        }
        gameObject.SetActive(false);
    }

    /// ---------------------------------------- 액션 이벤트 ---------------------------------------------------- ///
    // 공격
    public virtual void Attack()
    {
        if (target)
        {
            CurrentMana += 10 * currentManaRegen / 100;
            Damage damage = new Damage();
            damage.sourceGameObject = gameObject;
            damage.normalDamage = currentAttackPower;
            damage.onHit = true;
            target.ApplyDamage(damage);
        }
    }

    // 스킬
    public virtual void UseSkill()
    {
        CurrentMana = 0;
    }

    // 사망
    public virtual void Dead()
    {
        GetComponent<CircleCollider2D>().enabled = false;
    }

    /// ---------------------------------------- 데미지 처리 ---------------------------------------------------- ///
    // 피해
    public virtual void ApplyDamage(Damage damage)
    {
        if (damage == null)
            return;

        if (isDead)
            return;

        // 공격자 정보가 있는가?
        if (damage.sourceGameObject)
        {
            // 공격자가 유닛인가?
            Unit sourceUnit = null;

            if (damage.sourceGameObject.GetComponent<Unit>() != null)
                sourceUnit = damage.sourceGameObject.GetComponent<Unit>();

            // 공격자가 실명이고 온힛 스킬이면 무효화
            if (sourceUnit.buffDictionary != null)
            {                
                if ((sourceUnit.buffDictionary[BuffType.blind].currentSecond > 0) && damage.onHit)
                    return;
            }                            
        }

        // 피해자가 스킬 보호막을 가지고 있으면 무효화
        if ((buffDictionary[BuffType.skillShield].currentSecond > 0) && !damage.onHit)
        {
            buffDictionary[BuffType.skillShield].SetSecond(0f);
            return;
        }

        float totalNormalDamage = 0;
        float totalMagicDamage = 0;
        float totalTrueDamage = 0;
        float totalIncreaseHp = 0;
        float totalIncreaseMp = 0;
        float totalDecreaseMp = 0;

        totalNormalDamage = damage.normalDamage * (100 / (100 + currentAttackArmor));
        totalMagicDamage = damage.magicDamage * (100 / (100 + currentSpellArmor));
        totalTrueDamage = damage.trueDamage;
        totalDecreaseMp = damage.decreaseMp;
        totalIncreaseHp = damage.increaseHp;
        totalIncreaseMp = damage.increaseMp;

        // 내가 약화 상태이면 데미지 1.5배 증가
        if (buffDictionary[BuffType.hurt].currentSecond > 0)
        {
            totalNormalDamage *= 1.5f;
            totalMagicDamage *= 1.5f;
            totalTrueDamage *= 1.5f;
        }

        // 내가 치유 금지 상태이면 치유 효과 0으로 만듬
        if (buffDictionary[BuffType.banHealing].currentSecond > 0)
        {
            totalIncreaseHp = 0;
        }

        // 데미지를 적용하고 적용된 데미지 정보에 따라 데미지텍스트 출력하기
        if (totalNormalDamage > 0)
        {
            CurrentHealth -= totalNormalDamage;
            StartCoroutine(PrintDamageText(totalNormalDamage, DamageType.normalDamage));
        }
        if (totalMagicDamage > 0)
        {
            CurrentHealth -= totalMagicDamage;
            StartCoroutine(PrintDamageText(totalMagicDamage, DamageType.magicDamage));
        }
        if (totalTrueDamage > 0)
        {
            CurrentHealth -= totalTrueDamage;
            StartCoroutine(PrintDamageText(totalTrueDamage, DamageType.trueDamage));
        }
        if (totalIncreaseHp > 0)
        {
            totalIncreaseHp *= currentHealthRegen / 100;
            CurrentHealth += totalIncreaseHp;
            StartCoroutine(PrintDamageText(totalIncreaseHp, DamageType.increaseHp));
        }
        if (totalIncreaseMp > 0)
        {
            totalIncreaseMp *= currentManaRegen / 100;
            CurrentMana += totalIncreaseMp;
            StartCoroutine(PrintDamageText(totalIncreaseMp, DamageType.increaseMp));
        }
        if (totalDecreaseMp > 0)
        {
            CurrentMana -= totalDecreaseMp;
            StartCoroutine(PrintDamageText(totalDecreaseMp, DamageType.decreaseMp));
        }

        CurrentMana += ((totalNormalDamage + totalMagicDamage + totalTrueDamage) / maxHealth) * (currentManaRegen / 100) * 100; // 총 받은 체력 비례 피해량에 비례해 마나 회복
        InitBuff(damage); // 데미지 정보에 따라 버프 적용        

        if (damage.sourceGameObject)
        {
            Unit sourceUnit = damage.sourceGameObject.GetComponent<Unit>();
            Damage tempDamage = damage;
            tempDamage.normalDamage = totalNormalDamage;
            tempDamage.magicDamage = totalMagicDamage;
            tempDamage.trueDamage = totalTrueDamage;
            tempDamage.increaseHp = totalIncreaseHp;
            tempDamage.increaseMp = totalIncreaseMp;
            tempDamage.decreaseMp = totalDecreaseMp;
            sourceUnit.SucessAttack(damage);
        }
    }
    
    // 공격 피드백 정보
    public virtual void SucessAttack(Damage damage)
    {
        if (damage == null)
            return;

        // 생명력 흡수가 달려있으면 데미지의 일부를 흡혈
        if (damage.sourceGameObject && damage.lifeSteal > 0)
        {
            Unit sourceUnit = damage.sourceGameObject.GetComponent<Unit>();
            Damage tempDamage = new Damage();
            tempDamage.increaseHp = (damage.normalDamage + damage.magicDamage + damage.trueDamage) * damage.lifeSteal;
            sourceUnit.ApplyDamage(tempDamage);
        }
    }

    /// ---------------------------------------- 강제 이동 처리 ---------------------------------------------------- ///
    // 물리이동 + 피해
    public virtual void ApplyPhysicalDamage(Vector2 velocity, Damage bumpDamage)
    {
        // 강제 이동값이 있으면 강제 이동 실행
        if (velocity.magnitude > 0)
        {
            this.bumpDamage = bumpDamage;
            rigidbody.velocity = velocity;
            isRigid = true;
        }
    }

    // 충돌했는지 검사
    public void CheckBump(Collision2D collision)
    {
        // 강제 이동중인가?
        if (isRigid)
        {
            // 강제 이동중 벽과 충돌했는지 검사
            if (collision.gameObject.layer == LayerMask.NameToLayer("BattleObstacle"))
            {
                ApplyDamage(bumpDamage); // 벽궁 데미지 주기
                rigidbody.velocity = collision.contacts[0].normal * 1f;
                bumpDamage = null;
            }
        }
    }

    // 경직 상태인지 검사
    public void CheckRigid()
    {
        // 이동이 정지됬으면 강제 이동 해제
        if (isRigid && (rigidbody.velocity.magnitude > 4f))
        {
            gameObject.layer = LayerMask.NameToLayer("UsingMovementSkill");
            animator.SetBool("Rigid", true);
            aiState = AIState.stun;
        }
        else if (isRigid && (rigidbody.velocity.magnitude > 0.1f))
        {
            gameObject.layer = LayerMask.NameToLayer("UsingMovementSkill");
            animator.SetBool("Rigid", false);
            animator.SetBool("Walk", false);
            aiState = AIState.stun;
        }
        else if (isRigid && rigidbody.velocity.magnitude <= 0.1f)
        {
            isRigid = false;
            gameObject.layer = LayerMask.NameToLayer("UsingMovementSkill");
            rigidbody.velocity = Vector2.zero;
            aiState = AIState.stun;
        }
        else if (!isRigid)
        {
            animator.SetBool("Rigid", false);
            if (gameObject.layer == LayerMask.NameToLayer("UsingMovementSkill"))
                gameObject.layer = LayerMask.NameToLayer("BattleUnit");

            if(aiState == AIState.stun)            
                aiState = AIState.idle;
            
        }
    }

    /// ---------------------------------------- 버프 시스템 ---------------------------------------------------- ///
    public void InitBuff(Damage damage)
    {
        // 버프가 없으면 무시
        if (damage.buffList == null)
            return;
        if (isDead)
            return;

        // 실제 버프 적용
        foreach (BuffData buffData in damage.buffList)
        {
            buffDictionary[buffData.buffType].SetSecond(buffData.second);

            // 무적상태가 포함된 경우
            if ((buffDictionary[BuffType.invincible].currentSecond > 0) || (buffDictionary[BuffType.ice].currentSecond > 0))
            {
                ReleaseAggro();
                gameObject.layer = LayerMask.NameToLayer("BattleUnitInvincible");
            }
        }
    }

    public void ReleaseBuff(BuffType buffType)
    {
        if ((buffType == BuffType.invincible) || (buffType == BuffType.ice))
        {
            gameObject.layer = LayerMask.NameToLayer("BattleUnit");
        }
    }

    // 버프 정보, 아이템 스텟 관련 효과 적용
    void UpdateStatus()
    {
        // 합연산 계수
        float deltaHealthRegen = 0;
        float deltaManaRegen = 0;
        float deltaAttackPower = 0;
        float deltaSpellPower = 0;
        float deltaAttackArmor = 0;
        float deltaSpellArmor = 0;
        float deltaAttackSpeed = 0;
        float deltaWalkSpeed = 0;
        float deltaAttackDistance = 0;


        // 곱연산 계수
        float multipHealthRegen = 1;
        float multipManaRegen = 1;
        float multipAttackPower = 1;
        float multipAbilityPower = 1;
        float multipAttackArmor = 1;
        float multipSpellArmor = 1;
        float multipAttackSpeed = 1;
        float multipWalkSpeed = 1;
        float multipAttackDistance = 1;

        // 체력재생력 버프 영향 계산
        if ((buffDictionary[BuffType.healthRegenDown1].currentSecond > 0) || (buffDictionary[BuffType.healthRegenUp1].currentSecond > 0) ||
            (buffDictionary[BuffType.healthRegenDown2].currentSecond > 0) || (buffDictionary[BuffType.healthRegenUp2].currentSecond > 0) ||
            (buffDictionary[BuffType.healthRegenDown3].currentSecond > 0) || (buffDictionary[BuffType.healthRegenUp3].currentSecond > 0))
        {
            int buffLevel = 3; // 중간 레벨
            float[] buffMultip = new float[] { 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 1.75f };
            if (buffDictionary[BuffType.healthRegenDown3].currentSecond > 0)
                buffLevel -= 3;
            else if (buffDictionary[BuffType.healthRegenDown2].currentSecond > 0)
                buffLevel -= 2;
            else if (buffDictionary[BuffType.healthRegenDown1].currentSecond > 0)
                buffLevel -= 1;
            if (buffDictionary[BuffType.healthRegenUp3].currentSecond > 0)
                buffLevel += 3;
            else if (buffDictionary[BuffType.healthRegenUp2].currentSecond > 0)
                buffLevel += 2;
            else if (buffDictionary[BuffType.healthRegenUp1].currentSecond > 0)
                buffLevel += 1; ;
            multipHealthRegen *= buffMultip[buffLevel];
        }

        // 마나재생력 버프 영향 계산
        if ((buffDictionary[BuffType.manaRegenDown1].currentSecond > 0) || (buffDictionary[BuffType.manaRegenUp1].currentSecond > 0) ||
            (buffDictionary[BuffType.manaRegenDown2].currentSecond > 0) || (buffDictionary[BuffType.manaRegenUp2].currentSecond > 0) ||
            (buffDictionary[BuffType.manaRegenDown3].currentSecond > 0) || (buffDictionary[BuffType.manaRegenUp3].currentSecond > 0))
        {
            int buffLevel = 3; // 중간 레벨
            float[] buffMultip = new float[] { 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 1.75f };
            if (buffDictionary[BuffType.manaRegenDown3].currentSecond > 0)
                buffLevel -= 3;
            else if (buffDictionary[BuffType.manaRegenDown2].currentSecond > 0)
                buffLevel -= 2;
            else if (buffDictionary[BuffType.manaRegenDown1].currentSecond > 0)
                buffLevel -= 1;
            if (buffDictionary[BuffType.manaRegenUp3].currentSecond > 0)
                buffLevel += 3;
            else if (buffDictionary[BuffType.manaRegenUp2].currentSecond > 0)
                buffLevel += 2;
            else if (buffDictionary[BuffType.manaRegenUp1].currentSecond > 0)
                buffLevel += 1;
            multipManaRegen *= buffMultip[buffLevel];
        }

        // 공격력 버프 영향 계산
        if ((buffDictionary[BuffType.attackPowerDown1].currentSecond > 0) || (buffDictionary[BuffType.attackPowerUp1].currentSecond > 0) ||
            (buffDictionary[BuffType.attackPowerDown2].currentSecond > 0) || (buffDictionary[BuffType.attackPowerUp2].currentSecond > 0) ||
            (buffDictionary[BuffType.attackPowerDown3].currentSecond > 0) || (buffDictionary[BuffType.attackPowerUp3].currentSecond > 0))
        {
            int buffLevel = 3; // 중간 레벨
            float[] buffMultip = new float[] { 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 1.75f };
            if (buffDictionary[BuffType.attackPowerDown3].currentSecond > 0)
                buffLevel -= 3;
            else if (buffDictionary[BuffType.attackPowerDown2].currentSecond > 0)
                buffLevel -= 2;
            else if (buffDictionary[BuffType.attackPowerDown1].currentSecond > 0)
                buffLevel -= 1;
            if (buffDictionary[BuffType.attackPowerUp3].currentSecond > 0)
                buffLevel += 3;
            else if (buffDictionary[BuffType.attackPowerUp2].currentSecond > 0)
                buffLevel += 2;
            else if (buffDictionary[BuffType.attackPowerUp1].currentSecond > 0)
                buffLevel += 1;
            multipAttackPower *= buffMultip[buffLevel];
        }

        // 주문력 버프 영향 계산
        if ((buffDictionary[BuffType.spellPowerDown1].currentSecond > 0) || (buffDictionary[BuffType.spellPowerUp1].currentSecond > 0) ||
            (buffDictionary[BuffType.spellPowerDown2].currentSecond > 0) || (buffDictionary[BuffType.spellPowerUp2].currentSecond > 0) ||
            (buffDictionary[BuffType.spellPowerDown3].currentSecond > 0) || (buffDictionary[BuffType.spellPowerUp3].currentSecond > 0))
        {
            int buffLevel = 3; // 중간 레벨
            float[] buffMultip = new float[] { 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 1.75f };
            if (buffDictionary[BuffType.spellPowerDown3].currentSecond > 0)
                buffLevel -= 3;
            else if (buffDictionary[BuffType.spellPowerDown2].currentSecond > 0)
                buffLevel -= 2;
            else if (buffDictionary[BuffType.spellPowerDown1].currentSecond > 0)
                buffLevel -= 1;
            if (buffDictionary[BuffType.spellPowerUp3].currentSecond > 0)
                buffLevel += 3;
            else if (buffDictionary[BuffType.spellPowerUp2].currentSecond > 0)
                buffLevel += 2;
            else if (buffDictionary[BuffType.spellPowerUp1].currentSecond > 0)
                buffLevel += 1;
            multipAbilityPower *= buffMultip[buffLevel];
        }

        // 방어력 버프 영향 계산
        if ((buffDictionary[BuffType.attackArmorDown1].currentSecond > 0) || (buffDictionary[BuffType.attackArmorUp1].currentSecond > 0) ||
            (buffDictionary[BuffType.attackArmorDown2].currentSecond > 0) || (buffDictionary[BuffType.attackArmorUp2].currentSecond > 0) ||
            (buffDictionary[BuffType.attackArmorDown3].currentSecond > 0) || (buffDictionary[BuffType.attackArmorUp3].currentSecond > 0))
        {
            int buffLevel = 3; // 중간 레벨
            float[] buffMultip = new float[] { 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 1.75f };
            if (buffDictionary[BuffType.attackArmorDown3].currentSecond > 0)
                buffLevel -= 3;
            else if (buffDictionary[BuffType.attackArmorDown2].currentSecond > 0)
                buffLevel -= 2;
            else if (buffDictionary[BuffType.attackArmorDown1].currentSecond > 0)
                buffLevel -= 1;
            if (buffDictionary[BuffType.attackArmorUp3].currentSecond > 0)
                buffLevel += 3;
            else if (buffDictionary[BuffType.attackArmorUp2].currentSecond > 0)
                buffLevel += 2;
            else if (buffDictionary[BuffType.attackArmorUp1].currentSecond > 0)
                buffLevel += 1; ;
            multipAttackArmor *= buffMultip[buffLevel];
        }

        // 주문 방어력 버프 영향 계산
        if ((buffDictionary[BuffType.spellArmorDown1].currentSecond > 0) || (buffDictionary[BuffType.spellArmorUp1].currentSecond > 0) ||
            (buffDictionary[BuffType.spellArmorDown2].currentSecond > 0) || (buffDictionary[BuffType.spellArmorUp2].currentSecond > 0) ||
            (buffDictionary[BuffType.spellArmorDown3].currentSecond > 0) || (buffDictionary[BuffType.spellArmorUp3].currentSecond > 0))
        {
            int buffLevel = 3; // 중간 레벨
            float[] buffMultip = new float[] { 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 1.75f };
            if (buffDictionary[BuffType.spellArmorDown3].currentSecond > 0)
                buffLevel -= 3;
            else if (buffDictionary[BuffType.spellArmorDown2].currentSecond > 0)
                buffLevel -= 2;
            else if (buffDictionary[BuffType.spellArmorDown1].currentSecond > 0)
                buffLevel -= 1;
            if (buffDictionary[BuffType.spellArmorUp3].currentSecond > 0)
                buffLevel += 3;
            else if (buffDictionary[BuffType.spellArmorUp2].currentSecond > 0)
                buffLevel += 2;
            else if (buffDictionary[BuffType.spellArmorUp1].currentSecond > 0)
                buffLevel += 1; ;
            multipSpellArmor *= buffMultip[buffLevel];
        }

        // 공격속도 버프 영향 계산
        if ((buffDictionary[BuffType.attackSpeedDown1].currentSecond > 0) || (buffDictionary[BuffType.attackSpeedUp1].currentSecond > 0) ||
            (buffDictionary[BuffType.attackSpeedDown2].currentSecond > 0) || (buffDictionary[BuffType.attackSpeedUp2].currentSecond > 0) ||
            (buffDictionary[BuffType.attackSpeedDown3].currentSecond > 0) || (buffDictionary[BuffType.attackSpeedUp3].currentSecond > 0))
        {
            int buffLevel = 3; // 중간 레벨
            float[] buffMultip = new float[] { 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 1.75f };
            if (buffDictionary[BuffType.attackSpeedDown3].currentSecond > 0)
                buffLevel -= 3;
            else if (buffDictionary[BuffType.attackSpeedDown2].currentSecond > 0)
                buffLevel -= 2;
            else if (buffDictionary[BuffType.attackSpeedDown1].currentSecond > 0)
                buffLevel -= 1;
            if (buffDictionary[BuffType.attackSpeedUp3].currentSecond > 0)
                buffLevel += 3;
            else if (buffDictionary[BuffType.attackSpeedUp2].currentSecond > 0)
                buffLevel += 2;
            else if (buffDictionary[BuffType.attackSpeedUp1].currentSecond > 0)
                buffLevel += 1;
            multipAttackSpeed *= buffMultip[buffLevel];
        }

        // 이동속도 버프 영향 계산
        if ((buffDictionary[BuffType.walkSpeedDown1].currentSecond > 0) || (buffDictionary[BuffType.walkSpeedUp1].currentSecond > 0) ||
            (buffDictionary[BuffType.walkSpeedDown2].currentSecond > 0) || (buffDictionary[BuffType.walkSpeedUp2].currentSecond > 0) ||
            (buffDictionary[BuffType.walkSpeedDown3].currentSecond > 0) || (buffDictionary[BuffType.walkSpeedUp3].currentSecond > 0))
        {
            int buffLevel = 3; // 중간 레벨
            float[] buffMultip = new float[] { 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 1.75f };
            if (buffDictionary[BuffType.walkSpeedDown3].currentSecond > 0)
                buffLevel -= 3;
            else if (buffDictionary[BuffType.walkSpeedDown2].currentSecond > 0)
                buffLevel -= 2;
            else if (buffDictionary[BuffType.walkSpeedDown1].currentSecond > 0)
                buffLevel -= 1;
            if (buffDictionary[BuffType.walkSpeedUp3].currentSecond > 0)
                buffLevel += 3;
            else if (buffDictionary[BuffType.walkSpeedUp2].currentSecond > 0)
                buffLevel += 2;
            else if (buffDictionary[BuffType.walkSpeedUp1].currentSecond > 0)
                buffLevel += 1; ;
            multipWalkSpeed *= buffMultip[buffLevel];
        }

        if (buffDictionary[BuffType.root].currentSecond > 0)
        {
            multipWalkSpeed = 0;
        }

        // 최종 계산
        currentHealthRegen = (baseHealthRegan + deltaHealthRegen) * multipHealthRegen;
        currentManaRegen = (baseManaRegan + deltaManaRegen) * multipManaRegen;
        currentAttackPower = (baseAttackPower + deltaAttackPower) * multipAttackPower;
        currentSpellPower = (baseSpellPower + deltaSpellPower) * multipAbilityPower;
        currentAttackArmor = (baseAttackArmor + deltaAttackArmor) * multipAttackArmor;
        currentSpellArmor = (baseSpellArmor + deltaSpellArmor) * multipSpellArmor;
        currentAttackSpeed = (baseAttackSpeed + deltaAttackSpeed) * multipAttackSpeed;
        currentWalkSpeed = (baseWalkSpeed + deltaWalkSpeed) * multipWalkSpeed;
        currentAttackDistance = (baseAttackDistance + deltaAttackDistance) * multipAttackDistance;
    }

    // 버프에 따른 상태 업데이트
    void CheckBuff()
    {
        if (buffDictionary[BuffType.ice].currentSecond > 0 || buffDictionary[BuffType.stun].currentSecond > 0)
        {
            if (aiState != AIState.stun)
            {
                animator.SetBool("Rigid", false);
                animator.SetBool("Walk", false);
                aiState = AIState.stun;
            }            
        }
        else
        {
            if (aiState == AIState.stun)
            {
                aiState = AIState.idle;
            }            
        }
    }

    /// ---------------------------------------- AI ---------------------------------------------------- ///
    // 공격 사정거리 안에 있는 유닛 체크
    void CheckEnenyInAttackDistance()
    {
        int layerMask = 1 << LayerMask.NameToLayer("BattleUnit");
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, currentAttackDistance / 100 - 0.25f, layerMask);
        List<GameObject> tempGameObjects = new List<GameObject>();
        foreach (Collider2D collider in hits)
        {
            Unit unit = collider.gameObject.GetComponent<Unit>();
            if (unit.team == team)
                continue; // 아군 제외
            if (unit.isDead)
                continue; // 죽은 대상 제외
            if (unit.gameObject.layer != LayerMask.NameToLayer("BattleUnit"))
                continue; // 때릴수 없는 상태인 대상 제외
            tempGameObjects.Add(collider.gameObject);
        }
        enemyInAttackDistance = tempGameObjects;
    }

    // 나의 어그로 해제
    public void ReleaseAggro()
    {
        // 나를 타겟으로 하는 유닛 제거하기
        Unit[] Units = UnityEngine.Object.FindObjectsOfType<Unit>(); // 모든 유닛 찾기
        foreach (Unit unit in Units)
        {
            if (unit.target == this)
                unit.target = null;
        }
    }

    bool CanTarget(Unit unit)
    {
        if (unit == this)
            return false; // 자기 자신 제외
        if (unit.isDead)
            return false; // 이미 죽은 대상 제외        
        if (unit.gameObject.layer != LayerMask.NameToLayer("BattleUnit") && unit.gameObject.layer != LayerMask.NameToLayer("UsingMovementSkill"))
            return false; // 때릴수 없는 상태인 대상 제외
        return true;
    }

    // 가장 가까운 적 찾기
    public Unit GetCloseEnemy()
    {
        Unit tempUnit = null;
        float tempDistance = float.MaxValue;
        Unit[] units = UnityEngine.Object.FindObjectsOfType<Unit>(); // 모든 유닛 찾기
        foreach (Unit unit in units)
        {
            if (!CanTarget(unit))
                continue; // 타겟팅 할 수 없는 오브젝트 제외
            if (unit.team == this.team)
                continue; // 같은 팀 제외

            if (!tempUnit)
            {
                tempUnit = unit;
            }
            else
            {
                float distance = (unit.transform.position - transform.position).magnitude;
                tempDistance = (tempUnit.transform.position - transform.position).magnitude;
                if (distance > tempDistance)
                    continue;
                tempUnit = unit;
            }
        }
        return tempUnit;
    }

    // 가장 멀리있는 적 찾기
    public Unit GetFarEnemy()
    {
        Unit tempUnit = null;
        float tempDistance = 0;
        Unit[] units = UnityEngine.Object.FindObjectsOfType<Unit>(); // 모든 유닛 찾기
        foreach (Unit unit in units)
        {
            if (!CanTarget(unit))
                continue; // 타겟팅 할 수 없는 오브젝트 제외
            if (unit.team == this.team)
                continue; // 같은 팀 제외

            if (!tempUnit)
            {
                tempUnit = unit;
            }
            else
            {
                float distance = (unit.transform.position - transform.position).magnitude;
                tempDistance = (tempUnit.transform.position - transform.position).magnitude;
                if (distance < tempDistance)
                    continue;
                tempUnit = unit;
            }
        }
        return tempUnit;
    }

    // 가장 가까운 아군 찾기
    public Unit GetCloseFriend()
    {
        Unit tempUnit = null;
        float tempDistance = float.MaxValue;
        Unit[] units = UnityEngine.Object.FindObjectsOfType<Unit>(); // 모든 유닛 찾기
        foreach (Unit unit in units)
        {
            if (!CanTarget(unit))
                continue; // 타겟팅 할 수 없는 오브젝트 제외
            if (unit.team != this.team)
                continue; // 다른 팀 제외

            if (!tempUnit)
            {
                tempUnit = unit;
            }
            else
            {
                float distance = (unit.transform.position - transform.position).magnitude;
                tempDistance = (tempUnit.transform.position - transform.position).magnitude;
                if (distance > tempDistance)
                    continue;
                tempUnit = unit;
            }
        }
        return tempUnit;
    }

    // 가장 멀리있는 아군 찾기
    public Unit GetFarFriend()
    {
        Unit tempUnit = null;
        float tempDistance = 0;
        Unit[] units = UnityEngine.Object.FindObjectsOfType<Unit>(); // 모든 유닛 찾기
        foreach (Unit unit in units)
        {
            if (!CanTarget(unit))
                continue; // 타겟팅 할 수 없는 오브젝트 제외
            if (unit.team != this.team)
                continue; // 다른 팀 제외            

            if (!tempUnit)
            {
                tempUnit = unit;
            }
            else
            {
                float distance = (unit.transform.position - transform.position).magnitude;
                tempDistance = (tempUnit.transform.position - transform.position).magnitude;
                if (distance < tempDistance)
                    continue;
                tempUnit = unit;
            }
        }
        return tempUnit;
    }

    public Unit GetWeakEnemy()
    {
        Unit tempUnit = null;
        float tempHp = 0;
        Unit[] units = UnityEngine.Object.FindObjectsOfType<Unit>(); // 모든 유닛 찾기
        foreach (Unit unit in units)
        {
            if (!CanTarget(unit))
                continue; // 타겟팅 할 수 없는 오브젝트 제외
            if (unit.team == this.team)
                continue; // 같은 팀 제외

            if (!tempUnit)
            {
                tempUnit = unit;
            }
            else
            {
                float hp = unit.currentHealth;
                tempHp = tempUnit.currentHealth;
                if (hp > tempHp)
                    continue;
                tempUnit = unit;
            }
        }
        return tempUnit;
    }

    public Unit GetStrongEnemy()
    {
        Unit tempUnit = null;
        float tempHp = 0;
        Unit[] units = UnityEngine.Object.FindObjectsOfType<Unit>(); // 모든 유닛 찾기
        foreach (Unit unit in units)
        {
            if (!CanTarget(unit))
                continue; // 타겟팅 할 수 없는 오브젝트 제외
            if (unit.team == this.team)
                continue; // 같은 팀 제외

            if (!tempUnit)
            {
                tempUnit = unit;
            }
            else
            {
                float hp = unit.currentHealth;
                tempHp = tempUnit.currentHealth;
                if (hp < tempHp)
                    continue;
                tempUnit = unit;
            }
        }
        return tempUnit;
    }

    // 무브 포인트로 이동
    public virtual void MoveToMovePoint()
    {
        int sensorWidth = 18;
        float sensorDistnace = 0.5f;

        // 움직일 방향 앞에 장애물이 있는지 검사
        int layerMask = (1 << LayerMask.NameToLayer("BattleUnit") | (1 << LayerMask.NameToLayer("BattleUnitInvincible")));
        List<Vector3> canMoveDirectionList = new List<Vector3>();

        for (int i = 0; i < sensorWidth; i++)
        {
            Vector3 raycastDirection = Vector3.right.Rotate(i * 360 / sensorWidth);
            RaycastHit2D[] raycastHits = Physics2D.CircleCastAll(transform.position + raycastDirection * 0.1f, GetComponent<CircleCollider2D>().radius, raycastDirection, sensorDistnace, layerMask);
            GameObject hitObject = null;
            Vector3 hitPoint = new Vector3();
            foreach (RaycastHit2D raycastHit in raycastHits)
            {
                if (raycastHit.collider.gameObject == gameObject)
                    continue;
                if (raycastHit.collider)
                {
                    hitPoint = raycastHit.point;
                    hitObject = raycastHit.collider.gameObject;
                    break;
                }
            }
            // 충돌 여부에 따라 이동 방향 선택
            if (hitObject)
            {
                float tempDistance = (hitPoint - transform.position).magnitude;
                Debug.DrawRay(transform.position, raycastDirection * tempDistance, Color.red);
            }
            else
            {
                canMoveDirectionList.Add(raycastDirection);
                Debug.DrawRay(transform.position, raycastDirection * sensorDistnace, Color.green);
            }
        }

        // 이동 가능한 벡터 모두 가져오기
        //Debug.Log(canMoveDirectionList.Count);
        if (canMoveDirectionList.Count == 0)
        {
            rigidbody.velocity = Vector2.zero;
        }
        else
        {
            // 각도 구하기
            Vector2 tempDirection = (target.transform.position - transform.position).normalized;
            Dictionary<Vector2, float> angles = new Dictionary<Vector2, float>();
            foreach (Vector2 canMoveDirection in canMoveDirectionList)
            {
                angles.Add(canMoveDirection, Vector2.Angle(tempDirection, canMoveDirection));
            }

            // 가장 가까운 벡터 네개 가져오기
            List<Vector2> closeVectors = new List<Vector2>();
            var angleDictionary = angles.OrderBy(x => x.Value);
            for (int i = 0; i < 4; i++)
            {
                if (angleDictionary.Count() - 1 < i)
                    continue;
                closeVectors.Add(angleDictionary.ElementAt(i).Key);
            }

            // 현재 이동 방향과 가장 가까운 벡터 선택
            float tempAngle = 180;
            foreach (Vector2 closeVector in closeVectors)
            {
                if (closeVector == Vector2.zero)
                    return;
                if (Vector2.Angle(rigidbody.velocity, closeVector) < tempAngle)
                {
                    tempAngle = Vector2.Angle(rigidbody.velocity, closeVector);
                    tempDirection = closeVector;
                }
            }
            direction = tempDirection;
            rigidbody.velocity = direction * currentWalkSpeed / 100;
        }
        Debug.DrawRay(transform.position, movePoint - transform.position, new Color32(255, 255, 255, 255));
    }

    // AI 비헤이비어 트리
    public void RunBehaviorTree()
    {
        // 경직 상태이면 비활성화
        if (isRigid)
            return;

        // 스턴 상태이면 비활성화
        if (aiState == AIState.stun)
            return;

        // 타겟이 없으면 타겟 탐색
        if (!target)
            aiState = AIState.idle;

        // 아이들 상태인가?
        if (aiState == AIState.idle)
        {
            // 타겟 찾기            
            target = GetCloseEnemy();
            if (target)
            {
                // 이동 좌표 설정후 이동
                movePoint = target.transform.position;
                aiState = AIState.move;
            }
        }
        // 공격 상태인가?
        else if (aiState == AIState.attack)
        {
            // 타겟이 공격 범위 안에 있는가?
            if ((target.transform.position - transform.position).magnitude <= currentAttackDistance / 100 + 0.001f)
            {
                // 다른 행동 중이 아닌가?
                if (!isAction)
                {
                    // 마나가 부족한거나 침묵 상태인가?
                    if (currentMana < maxMana || buffDictionary[BuffType.silence].currentSecond > 0)
                    {
                        StartCoroutine(PlayAttackAnim(100 / CurrentAttackSpeed)); // 일반 공격
                    }
                    else
                    {
                        StartCoroutine(PlaySkillAnim(100 / CurrentAttackSpeed)); // 스킬 사용
                    }
                }
            }
            // 타겟이 공격 범위 밖에 있는가?
            else
            {
                // 타겟 재탐색
                target = null;
                aiState = AIState.idle;
            }
        }
        // 이동 상태인가?
        else if (aiState == AIState.move)
        {
            // 이동중 다른 타겟과 맞주쳤는가?
            if (enemyInAttackDistance.Count > 0)
            {
                // 타겟을 바꾸고 공격
                target = GetCloseEnemy();
                rigidbody.velocity = Vector2.zero;
                aiState = AIState.attack;
            }
            // 타겟이 공격 범위 안에 있는가?
            else if ((target.transform.position - transform.position).magnitude <= currentAttackDistance / 100 + 0.001)
            {
                rigidbody.velocity = Vector2.zero;
                aiState = AIState.attack;
            }
            else
            {
                MoveToMovePoint(); // 좌표를 향해 계속 이동
            }
        }
    }

    /// ---------------------------------------- 데미지 출력 ---------------------------------------------------- ///    
    // 데미지 출력 오브젝트 풀 관리
    public void InitDamageTextPool()
    {
        for (int i = 0; i < 50; i++)
        {
            GameObject tempDamageText = Instantiate(damageText, canvas);
            tempDamageText.SetActive(false);
            damageTextPool.Push(tempDamageText);
        }
    }
    public GameObject PopDamageTextPool()
    {
        GameObject tempDamageText = damageTextPool.Pop();
        tempDamageText.SetActive(true);
        tempDamageText.GetComponent<Animator>().SetTrigger("Floating");
        return tempDamageText;
    }
    public void PushDamageTextPool(GameObject tempDamageText)
    {
        damageTextPool.Push(tempDamageText);
        tempDamageText.SetActive(false);
    }
    IEnumerator PrintDamageText(float damage, DamageType damageType)
    {
        GameObject tempDamageText = PopDamageTextPool();
        tempDamageText.transform.SetAsLastSibling();

        // 타입에 따라 색상 설정
        switch (damageType)
        {
            case DamageType.normalDamage:
                tempDamageText.GetComponent<TextMeshProUGUI>().color = Color.red;
                break;
            case DamageType.magicDamage:
                tempDamageText.GetComponent<TextMeshProUGUI>().color = Color.magenta;
                break;
            case DamageType.trueDamage:
                tempDamageText.GetComponent<TextMeshProUGUI>().color = Color.white;
                break;
            case DamageType.increaseHp:
                tempDamageText.GetComponent<TextMeshProUGUI>().color = Color.green;
                break;
            case DamageType.increaseMp:
                tempDamageText.GetComponent<TextMeshProUGUI>().color = Color.blue;
                break;
            case DamageType.decreaseMp:
                tempDamageText.GetComponent<TextMeshProUGUI>().color = Color.gray;
                break;            
        }

        // 타입에 따라 텍스트 설정
        switch (damageType)
        {
            case DamageType.normalDamage:
            case DamageType.magicDamage:
            case DamageType.trueDamage:
            case DamageType.decreaseMp:
                tempDamageText.GetComponent<TextMeshProUGUI>().text = ((int)damage).ToString();
                break;
            case DamageType.increaseHp:
            case DamageType.increaseMp:
                tempDamageText.GetComponent<TextMeshProUGUI>().text = "+" + ((int)damage).ToString();
                break;
        }        
        yield return new WaitForSeconds(2f);
        PushDamageTextPool(tempDamageText);
    }

    public static Unit GetData(string key)
    {
        Unit unit = Resources.Load<Unit>(string.Format("Prefabs/Unit/{0}", key));

        if (unit == null)
        {
            Debug.LogWarning(string.Format("{0}키를 가진 유닛 데이터를 찾을 수 없습니다.", key));
            return null;
        }
        else
        {
            return unit;
        }
    }
}
