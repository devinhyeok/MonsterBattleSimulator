using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
    /// ---------------------------------------- 선언 ---------------------------------------------------- ///
    [Header("편집값")]
    public int team; // 팀 이름
    public string key;
    public UnitData unitData; // 유닛 기본 스텟
    Color enemyColor = new Color32(200, 0, 0, 255);
    Color friendColor = new Color32(0, 200, 0, 255);

    [Header("참조값")]
    protected SpriteRenderer spriteRenderer;
    protected Animator animator;
    protected RectTransform canvas;
    protected Image hpBar;
    protected Image mpBar;
    protected GameObject damageText;
    protected Stack<GameObject> damageTextPool = new Stack<GameObject>(); // 플로팅 데미지 오브젝트 풀

    [Header("디버그 설정")]
    public bool testSkill;

    [Header("읽기용")]
    // 유닛 전투 스텟 정보
    public float maxHp;
    [SerializeField]
    private float currentHp;
    public float maxMp;
    [SerializeField]
    private float currentMp;
    public float currentHealthRegen;
    public float currentManaRegen;
    public float currentAttack;
    [SerializeField]
    private float currentAttackSpeed;
    public float currentAttackDistance;
    public float currentDefense;
    public float currentAbilityPower;
    public float currentWalkSpeed;    

    // 유닛 전투 정보
    public Dictionary<BuffType, Buff> buffDictionary = new Dictionary<BuffType, Buff>(); // 버프 딕셔너리

    // 유닛 상태
    public bool isDead = false; // 사망여부       
    public AIState aiState = AIState.none;
    [SerializeField]
    protected Unit target;
    [SerializeField]
    protected Vector3 direction = new Vector2(-1, 0); // 보는 방향
    [SerializeField]
    protected bool isAction = false;
    [SerializeField]
    protected bool isRigid = false;    

    // 강제 이동
    Coroutine rigidMoveCoroution;
    Damage bumpDamage;

    /// ---------------------------------------- 프로퍼티 ---------------------------------------------------- ///
    public float CurrentHp
    {
        get
        {
            return currentHp;
        }
        set
        {
            currentHp = Mathf.Clamp(value, 0, maxHp);
            if (currentHp <= 0)
                StartCoroutine(PlayDeadAnim(1f));
        }
    }
    public float CurrentMp
    {
        get
        {
            return currentMp;
        }
        set
        {
            currentMp = Mathf.Clamp(value, 0, maxMp);
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

    /// ---------------------------------------- 레벨 디자인 이벤트 ---------------------------------------------------- ///
    // 편집때마다 실행
    private void OnValidate()
    {

    }

    /// ---------------------------------------- 유니티 이벤트 ---------------------------------------------------- ///
    private void Awake()
    {
        // 외부 스크립트  가져오기
        GetPointers();

        // 초기 설정
        Init();
        InitDamageTextPool();
    }

    private void Start()
    {
        // 팀에 따라 HpBar 색상 바꾸기
        if (AdventureModeManager.Instance)
        {
            if (team == AdventureModeManager.Instance.playerController.team)
                hpBar.color = friendColor;
            else
                hpBar.color = enemyColor;
        }
        else
        {
            if (team == 0)
                hpBar.color = friendColor;
            else
                hpBar.color = enemyColor;
        }
        if (testSkill)
        {
            CurrentMp = maxMp;
        }
    }

    private void Update()
    {
        // 디버그용 버튼
        if (Input.GetKeyDown(KeyCode.Space))
        {
            
        }

        // 디버그용 레이저
        //Debug.DrawRay(transform.position, direction, Color.green);

        // 상태바 업데이트
        hpBar.fillAmount = CurrentHp / maxHp;
        mpBar.fillAmount = CurrentMp / maxMp;

        // 방향 업데이트
        if (direction.x <= 0)
        {
            spriteRenderer.flipX = false;
            animator.SetBool("Left", true);
        }
        else
        {
            spriteRenderer.flipX = true;
            animator.SetBool("Left", false);
        }
    }

    private void FixedUpdate()
    {
        UpdateStatus();
        // AI가 켜져있는가?
        if (aiState != AIState.none)
        {
            RunBehaviorTree();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 강제 이동중인가?
        if (isRigid)
        {
            // 강제 이동중 벽과 충돌했는지 검사
            if (collision.gameObject.layer == LayerMask.NameToLayer("BattleObstacle"))
            {
                ApplyDamage(bumpDamage); // 벽궁 데미지 주기
                StartCoroutine(StopRigidMove());
                transform.position = transform.position + (Vector3)collision.contacts[0].normal * 0.1f;
            }
        }       
    }

    /// ---------------------------------------- 초기화 ---------------------------------------------------- ///
    public void GetPointers()
    {
        // 값을 참조할 컴포넌트 주소 가져오기
        spriteRenderer = transform.Find("Sprite").GetComponent<SpriteRenderer>();
        animator = transform.Find("Sprite").GetComponent<Animator>();
        canvas = transform.Find("Canvas").GetComponent<RectTransform>();
        hpBar = transform.Find("Canvas").Find("HpBorder").Find("HpBar").GetComponent<Image>();
        mpBar = transform.Find("Canvas").Find("MpBorder").Find("MpBar").GetComponent<Image>();

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
        foreach(BuffType buffType in buffTypeList)
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
        // 스텟 초기화
        maxHp = unitData.hp;
        CurrentHp = unitData.hp;
        maxMp = unitData.mp;
        CurrentMp = 0;
        currentAttack = unitData.attack;
        CurrentAttackSpeed = unitData.attackSpeed;
        currentDefense = unitData.defense;
        currentHealthRegen = unitData.healthRegen;
        currentManaRegen = unitData.manaRegen;
        currentAbilityPower = unitData.abilityPower;
        currentAttackDistance = unitData.attackDistance;
        currentWalkSpeed = unitData.walkSpeed;

        // 그래픽 초기화
        canvas.gameObject.SetActive(true);
        spriteRenderer.sortingLayerName = "GameObject";
    }

    /// ---------------------------------------- 애니메이션 이벤트 ---------------------------------------------------- ///
    // 공격 애니메이션 재생
    IEnumerator PlayAttackAnim(float cooltime)
    {
        if (target)
        {
            direction = (target.transform.position - transform.position).normalized;
        }
        animator.SetTrigger("Attack");
        isAction = true;
        yield return new WaitForSeconds(cooltime);
        isAction = false;
    }

    // 스킬 애니메이션 재생
    IEnumerator PlaySkillAnim(float cooltime)
    {        
        if (target)
        {
            direction = (target.transform.position - transform.position).normalized;
        }
        animator.SetTrigger("UseSkill");
        isAction = true;
        yield return new WaitForSeconds(cooltime);
        isAction = false;
    }
    IEnumerator PlaySkillAnim(float cooltime, bool lookAtTarget)
    {
        if (target && lookAtTarget)
        {
            direction = (target.transform.position - transform.position).normalized;
        }
        animator.SetTrigger("UseSkill");
        isAction = true;
        yield return new WaitForSeconds(cooltime);
        isAction = false;
    }

    // 사망 애니메이션 재생
    IEnumerator PlayDeadAnim(float animtime)
    {
        // 사망 처리
        isDead = true;
        aiState = AIState.none;
        animator.SetTrigger("Dead");
        canvas.gameObject.SetActive(false);

        ReleaseAggro();

        // 사망 이벤트 발생
        yield return new WaitForSeconds(animtime);
        spriteRenderer.sortingLayerName = "Decoration";
    }

    // 넉백 애니메이션 재생
    IEnumerator PlayRigidMove(Vector3 forcedVelocity)
    {
        isRigid = true;
        Vector3 targetPosition = transform.position + forcedVelocity.normalized;

        float moveSpeed = 5f;
        while ((transform.position - targetPosition).magnitude > 0.1f / moveSpeed)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);            
            yield return new WaitForEndOfFrame();
        }
        transform.position = targetPosition;
        StartCoroutine(StopRigidMove());
    }

    // 넉백 중지
    IEnumerator StopRigidMove()
    {
        bumpDamage = null;
        StopCoroutine(rigidMoveCoroution);
        yield return new WaitForSeconds(0.2f); // 벽궁에 0.2초 경직 있음
        isRigid = false;
    }

    /// ---------------------------------------- 액션 이벤트 ---------------------------------------------------- ///
    // 공격
    public virtual void Attack()
    {
        if (target)
        {
            CurrentMp += 10 * currentManaRegen / 100;
            Damage damage = new Damage();
            damage.sourceGameObject = gameObject;
            damage.normalDamage = currentAttack;
            damage.onHit = true;            
            target.ApplyDamage(damage);
        }        
    }

    // 스킬
    public virtual void UseSkill()
    {
        CurrentMp = 0;
    }

    // 사망
    public virtual void Dead()
    {
        spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 1f);
    }

    // 피해
    public virtual void ApplyDamage(Damage damage)
    {
        if (damage == null)
            return;

        // 공격자 정보가 있는가?
        if (damage.sourceGameObject != null)
        {
            // 공격자가 유닛인가?
            Unit sourceUnit = null;
            if (damage.sourceGameObject.GetComponent<Unit>() != null)
                sourceUnit = damage.sourceGameObject.GetComponent<Unit>();

            // 공격자가 실명이고 온힛 스킬이면 무효화
            if ((sourceUnit.buffDictionary[BuffType.blind].currentSecond > 0) && damage.onHit)
                return; 
        }
        
        // 피해자가 스킬 보호막을 가지고 있으면 무효화
        if ((buffDictionary[BuffType.skillShield].currentSecond > 0) && !damage.onHit)
        {
            buffDictionary[BuffType.skillShield].SetSecond(0f);
            return;
        }

        float totalNormalDamage = 0;
        float totalTrueDamage = 0;
        float totalManaDamage = 0;
        float totalIncreaseHp = 0;
        float totalIncreaseMp = 0;

        totalNormalDamage = damage.normalDamage * (100 / (100 + currentDefense));
        totalTrueDamage = damage.trueDamage;
        totalManaDamage = damage.manaDamage;
        totalIncreaseHp = damage.increaseHp;
        totalIncreaseMp = damage.increaseMp;        

        // 내가 약화 상태이면 데미지 1.5배 증가
        if (buffDictionary[BuffType.hurt].currentSecond > 0)
        {
            totalNormalDamage = totalNormalDamage * 1.5f;
            totalTrueDamage = totalTrueDamage * 1.5f;
        }

        // 내가 치유 금지 상태이면 치유 효과 0으로 만듬
        if (buffDictionary[BuffType.banHealing].currentSecond > 0)
        {
            totalIncreaseHp = 0;
        }

        // 데미지를 적용하고 적용된 데미지 정보에 따라 데미지텍스트 출력하기
        if (totalNormalDamage > 0)
        {
            CurrentHp -= totalNormalDamage;
            StartCoroutine(PrintDamageText(totalNormalDamage, DamageType.normalDamage));
        }
        if (totalTrueDamage > 0)
        {
            CurrentHp -= totalTrueDamage;
            StartCoroutine(PrintDamageText(totalTrueDamage, DamageType.trueDamage));
        }
        if (totalManaDamage > 0)
        {
            CurrentMp -= totalManaDamage;
            StartCoroutine(PrintDamageText(totalManaDamage, DamageType.manaDamage));
        }
        if (totalIncreaseHp > 0)
        {
            totalIncreaseHp *= currentHealthRegen / 100;
            CurrentHp += totalIncreaseHp;
            StartCoroutine(PrintDamageText(totalIncreaseHp, DamageType.IncreaseHp));
        }
        if (totalIncreaseMp > 0)
        {
            totalIncreaseMp *= currentManaRegen / 100;
            CurrentMp += totalIncreaseMp;
            StartCoroutine(PrintDamageText(totalIncreaseMp, DamageType.IncreaseMp));
        }

        CurrentMp += ((totalNormalDamage + totalTrueDamage) / maxHp) * (currentManaRegen / 100) * 100; // 총 받은 체력 비례 피해량에 비례해 마나 회복
        InitBuff(damage); // 데미지 정보에 따라 버프 적용
    }

    // 물리적인 공격이 포함된 데미지
    public virtual void ApplyPhysicalDamage(Vector2 velocity)
    {
        // 강제 이동값이 있으면 강제 이동 실행
        if (velocity.magnitude > 0)
        {
            rigidMoveCoroution = StartCoroutine(PlayRigidMove(velocity));            
        }
    }
    public virtual void ApplyPhysicalDamage(Vector2 velocity, Damage bumpDamage)
    {
        // 강제 이동값이 있으면 강제 이동 실행
        if (velocity.magnitude > 0)
        {
            rigidMoveCoroution = StartCoroutine(PlayRigidMove(velocity));
            this.bumpDamage = bumpDamage;
        }
    }

    /// ---------------------------------------- AI ---------------------------------------------------- ///
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

    // 가장 가까운 적 찾기
    public Unit GetCloseEnemy()
    {
        Unit tempUnit = null;
        float tempDistance = float.MaxValue;
        Unit[] units = UnityEngine.Object.FindObjectsOfType<Unit>(); // 모든 유닛 찾기
        foreach (Unit unit in units)
        {       
            if (unit == this)
                continue; // 자기 자신 제외
            if (unit.team == this.team)
                continue; // 같은 팀 제외
            if (unit.isDead)
                continue; // 이미 죽은 대상 제외
            if (unit.gameObject.layer == LayerMask.NameToLayer("BattleUnitInvincible"))
                continue; // 무적인 대상 제외

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
            if (unit == this)
                continue; // 자기 자신 제외
            if (unit.team == this.team)
                continue; // 같은 팀 제외
            if (unit.isDead)
                continue; // 이미 죽은 대상 제외
            if (unit.gameObject.layer == LayerMask.NameToLayer("BattleUnitInvincible"))
                continue; // 무적인 대상 제외

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
            if (unit == this)
                continue; // 자기 자신 제외
            if (unit.team != this.team)
                continue; // 다른 팀 제외
            if (unit.isDead)
                continue; // 이미 죽은 대상 제외
            if (unit.gameObject.layer == LayerMask.NameToLayer("BattleUnitInvincible"))
                continue; // 무적인 대상 제외

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
            if (unit == this)
                continue; // 자기 자신 제외
            if (unit.team != this.team)
                continue; // 다른 팀 제외
            if (unit.isDead)
                continue; // 이미 죽은 대상 제외
            if (unit.gameObject.layer == LayerMask.NameToLayer("BattleUnitInvincible"))
                continue; // 무적인 대상 제외

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

    // 경직 상태인지 검사
    public bool CheckRigid()
    {
        if (isRigid || buffDictionary[BuffType.stun].currentSecond > 0 || buffDictionary[BuffType.ice].currentSecond > 0)
        {
            animator.SetBool("Rigid", true);
            return false;
        }
        animator.SetBool("Rigid", false);
        return true;
    }
    
    // AI 비헤이비어 트리
    public void RunBehaviorTree()
    {
        // 경직 상태인지 검사
        if (!CheckRigid())
        {
            return;
        }
        // 타겟이 없는가?
        if (!target)
        {
            // 타겟 찾기
            aiState = AIState.idle;
            target = GetCloseEnemy();
        }
        // 타겟을 있는가?
        else
        {
            // 공격 범위 밖에 있는가?
            if (currentAttackDistance / 100 < (target.transform.position - transform.position).magnitude)
            {
                aiState = AIState.move;
                direction = (target.transform.position - transform.position).normalized;
                transform.position = transform.position + (direction * Time.deltaTime * currentWalkSpeed/100);
            }
            // 공격 범위 안에 있는가?
            else
            {
                aiState = AIState.attack;
                // 다른 행동 중이 아닌가?
                if (!isAction)
                {
                    // 마나가 부족한거나 침묵 상태인가?
                    if (currentMp < maxMp || buffDictionary[BuffType.silence].currentSecond > 0)
                    {
                        StartCoroutine(PlayAttackAnim(100 / CurrentAttackSpeed)); // 일반 공격
                    }
                    else
                    {
                        StartCoroutine(PlaySkillAnim(100 / CurrentAttackSpeed)); // 스킬 사용
                    }
                }
            }
        }
    }

    /// ---------------------------------------- 버프 시스템 ---------------------------------------------------- ///
    public void InitBuff(Damage damage)
    {
        // 버프가 없으면 무시
        if (damage.buffList == null)
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
                spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
            }
        }
    }

    public void ReleaseBuff(BuffType buffType)
    {
        if ((buffType == BuffType.invincible) || (buffType == BuffType.ice))
        {
            gameObject.layer = LayerMask.NameToLayer("BattleUnit");
            spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
        }
    }

    // 버프 정보, 아이템 스텟 관련 효과 적용
    void UpdateStatus()
    {
        // 합연산 계수
        float deltaAttack = 0;
        float deltaAttackSpeed = 0;
        float deltaAbilityPower = 0;
        float deltaHealthRegen = 0;
        float deltaManaRegen = 0;
        float deltaDefense = 0;
        float deltaWalkSpeed = 0;
        float deltaAttackDistance = 0;

        // 곱연산 계수
        float multipAttack = 1;
        float multipAttackSpeed = 1;
        float multipAbilityPower = 1;
        float multipHealthRegen = 1;
        float multipManaRegen = 1;
        float multipDefense = 1;
        float multipWalkSpeed = 1;
        float multipAttackDistance = 1;

        // 공격력 버프 영향 계산
        if ((buffDictionary[BuffType.attackDown1].currentSecond > 0) || (buffDictionary[BuffType.attackUp1].currentSecond > 0) || 
            (buffDictionary[BuffType.attackDown2].currentSecond > 0) || (buffDictionary[BuffType.attackUp2].currentSecond > 0) ||
            (buffDictionary[BuffType.attackDown3].currentSecond > 0) || (buffDictionary[BuffType.attackUp3].currentSecond > 0))
        {
            int buffLevel = 3; // 중간 레벨
            float[] buffMultip = new float[] { 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 1.75f };
            if (buffDictionary[BuffType.attackDown3].currentSecond > 0)
                buffLevel -= 3;
            else if (buffDictionary[BuffType.attackDown2].currentSecond > 0)
                buffLevel -= 2;
            else if (buffDictionary[BuffType.attackDown1].currentSecond > 0)
                buffLevel -= 1;
            if (buffDictionary[BuffType.attackUp3].currentSecond > 0)
                buffLevel += 3;
            else if (buffDictionary[BuffType.attackUp2].currentSecond > 0)
                buffLevel += 2;
            else if (buffDictionary[BuffType.attackUp1].currentSecond > 0)
                buffLevel += 1;
            multipAttack *= buffMultip[buffLevel];
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

        // 주문력 버프 영향 계산
        if ((buffDictionary[BuffType.abilityPowerDown1].currentSecond > 0) || (buffDictionary[BuffType.abilityPowerUp1].currentSecond > 0) ||
            (buffDictionary[BuffType.abilityPowerDown2].currentSecond > 0) || (buffDictionary[BuffType.abilityPowerUp2].currentSecond > 0) ||
            (buffDictionary[BuffType.abilityPowerDown3].currentSecond > 0) || (buffDictionary[BuffType.abilityPowerUp3].currentSecond > 0))
        {
            int buffLevel = 3; // 중간 레벨
            float[] buffMultip = new float[] { 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 1.75f };
            if (buffDictionary[BuffType.abilityPowerDown3].currentSecond > 0)
                buffLevel -= 3;
            else if (buffDictionary[BuffType.abilityPowerDown2].currentSecond > 0)
                buffLevel -= 2;
            else if (buffDictionary[BuffType.abilityPowerDown1].currentSecond > 0)
                buffLevel -= 1;
            if (buffDictionary[BuffType.abilityPowerUp3].currentSecond > 0)
                buffLevel += 3;
            else if (buffDictionary[BuffType.abilityPowerUp2].currentSecond > 0)
                buffLevel += 2;
            else if (buffDictionary[BuffType.abilityPowerUp1].currentSecond > 0)
                buffLevel += 1;
            multipAbilityPower *= buffMultip[buffLevel];
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

        // 방어력 버프 영향 계산
        if ((buffDictionary[BuffType.defenseDown1].currentSecond > 0) || (buffDictionary[BuffType.defenseUp1].currentSecond > 0) ||
            (buffDictionary[BuffType.defenseDown2].currentSecond > 0) || (buffDictionary[BuffType.defenseUp2].currentSecond > 0) ||
            (buffDictionary[BuffType.defenseDown3].currentSecond > 0) || (buffDictionary[BuffType.defenseUp3].currentSecond > 0))
        {
            int buffLevel = 3; // 중간 레벨
            float[] buffMultip = new float[] { 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 1.75f };
            if (buffDictionary[BuffType.defenseDown3].currentSecond > 0)
                buffLevel -= 3;
            else if (buffDictionary[BuffType.defenseDown2].currentSecond > 0)
                buffLevel -= 2;
            else if (buffDictionary[BuffType.defenseDown1].currentSecond > 0)
                buffLevel -= 1;
            if (buffDictionary[BuffType.defenseUp3].currentSecond > 0)
                buffLevel += 3;
            else if (buffDictionary[BuffType.defenseUp2].currentSecond > 0)
                buffLevel += 2;
            else if (buffDictionary[BuffType.defenseUp1].currentSecond > 0)
                buffLevel += 1; ;
            multipDefense *= buffMultip[buffLevel];
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

        if (buffDictionary[BuffType.root].currentSecond > 0)
        {
            multipWalkSpeed = 0;
        }

        // 최종 계산
        currentAttack = (unitData.attack + deltaAttack) * multipAttack;
        currentAttackSpeed = (unitData.attackSpeed + deltaAttackSpeed) * multipAttackSpeed;
        currentDefense = (unitData.defense + deltaDefense) * multipDefense;
        currentManaRegen = (unitData.manaRegen + deltaManaRegen) * multipManaRegen;
        currentAbilityPower = (unitData.abilityPower + deltaAbilityPower) * multipAbilityPower;
        currentAttackDistance = (unitData.attackDistance + deltaAttackDistance) * multipAttackDistance;
        currentWalkSpeed = (unitData.walkSpeed + deltaWalkSpeed) * multipWalkSpeed;
        currentHealthRegen = (unitData.healthRegen + deltaHealthRegen) * multipHealthRegen;
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

        // 타입에 따라 색상 설정
        switch (damageType)
        {
            case DamageType.normalDamage:
                tempDamageText.GetComponent<Text>().color = Color.red;
                break;
            case DamageType.trueDamage:
                tempDamageText.GetComponent<Text>().color = Color.magenta;
                break;
            case DamageType.manaDamage:
                tempDamageText.GetComponent<Text>().color = Color.gray;
                break;
            case DamageType.IncreaseHp:
                tempDamageText.GetComponent<Text>().color = Color.green;
                break;
            case DamageType.IncreaseMp:
                tempDamageText.GetComponent<Text>().color = Color.blue;
                break;
        }

        // 타입에 따라 텍스트 설정
        switch (damageType)
        {
            case DamageType.normalDamage:
            case DamageType.trueDamage:
            case DamageType.manaDamage:
                tempDamageText.GetComponent<Text>().text = ((int)damage).ToString();
                break;
            case DamageType.IncreaseHp:
            case DamageType.IncreaseMp:
                tempDamageText.GetComponent<Text>().text = "+" + ((int)damage).ToString();
                break;
        }        
        yield return new WaitForSeconds(2f);
        PushDamageTextPool(tempDamageText);
    }
}
