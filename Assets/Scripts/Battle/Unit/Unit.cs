using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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

[System.Serializable]
public class UnitData
{
    public List<UnitStatus> statusList;
    
    [TextArea]
    public string englishTooltip;
    [TextArea]
    public string koreanTooltip;

    public static UnitData GetData(string key)
    {
        Unit unit = Resources.Load<Unit>(string.Format("Prefabs/Unit/{0}", key));

        if (unit == null)
        {
            Debug.LogWarning(string.Format("{0}키를 가진 유닛 데이터를 찾을 수 없습니다.", key));
            return null;
        }
        else
        {
            return unit.unitData;
        }                    
    }
}

public class Unit : MonoBehaviour
{
    /// ---------------------------------------- 선언 ---------------------------------------------------- ///
    [Header("편집값")]        
    public string key;
    public int team; // 팀 이름
    public int level; // 현재 레벨값
    public UnitData unitData; // 유닛 기본 스텟
    Color enemyColor = new Color32(200, 0, 0, 255);
    Color friendColor = new Color32(0, 200, 0, 255);

    [Header("참조값")]
    [HideInInspector]
    public new Rigidbody2D rigidbody;
    protected SpriteRenderer spriteRenderer;
    protected Animator animator;
    protected RectTransform canvas;
    protected Image healthBar;
    protected Image mpBar;
    protected GameObject damageText;
    protected Stack<GameObject> damageTextPool = new Stack<GameObject>(); // 플로팅 데미지 오브젝트 풀
    public List<GameObject> enemyInAttackDistance = new List<GameObject>(); // 공격 사거리에 있는 오브젝트 리스트

    [Header("디버그 설정")]
    public bool testSkill;    
    [Header("읽기용")]
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
    
    // 유닛 전투 정보
    public Dictionary<BuffType, Buff> buffDictionary = new Dictionary<BuffType, Buff>(); // 버프 딕셔너리

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
    

    // 강제 이동
    Coroutine rigidMoveCoroution;
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
                StartCoroutine(PlayDeadAnim(1f));
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

    private void Update()
    {
        // 디버그용 버튼
        if (Input.GetKeyDown(KeyCode.Space))
        {
            
        }

        // 디버그용 레이저
        //Debug.DrawRay(transform.position, direction, Color.green);

        // 상태바 업데이트
        healthBar.fillAmount = CurrentHealth / maxHealth;
        mpBar.fillAmount = CurrentMana / maxMana;

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
        // 사정거리 안에 유닛이 존재하는지 체크
        CheckEnenyInAttackDistance();

        // 스테이터스 업데이트
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
        maxHealth = unitData.statusList[level-1].health;
        CurrentHealth = unitData.statusList[level-1].health;
        maxMana = unitData.statusList[level-1].mana;
        CurrentMana = 0;
        currentHealthRegen = unitData.statusList[level-1].healthRegen;
        currentManaRegen = unitData.statusList[level-1].manaRegen;
        currentAttackPower = unitData.statusList[level-1].attackPower;
        currentSpellPower = unitData.statusList[level-1].spellPower;        
        currentAttackArmor = unitData.statusList[level-1].attackArmor;
        currentSpellArmor = unitData.statusList[level-1].spellArmor;
        CurrentAttackSpeed = unitData.statusList[level-1].attackSpeed;
        currentWalkSpeed = unitData.statusList[level-1].walkSpeed;
        currentAttackDistance = unitData.statusList[level-1].attackDistance;        

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
    public virtual IEnumerator PlaySkillAnim(float cooltime)
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
        // 설정
        gameObject.layer = LayerMask.NameToLayer("UsingMovementSkill");
        isRigid = true;
        rigidbody.velocity = Vector2.zero;

        // 해당 좌표로 유닛 넉백 시키기
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
        // 설정
        gameObject.layer = LayerMask.NameToLayer("BattleUnit");
        bumpDamage = null;

        // 강제이동 코루틴 정지
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

        if (damage.onHit)
            totalNormalDamage = damage.normalDamage * (100 / (100 + currentAttackArmor));
        else
            totalNormalDamage = damage.normalDamage * (100 / (100 + currentSpellArmor));

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
            CurrentHealth -= totalNormalDamage;
            StartCoroutine(PrintDamageText(totalNormalDamage, DamageType.normalDamage));
        }
        if (totalTrueDamage > 0)
        {
            CurrentHealth -= totalTrueDamage;
            StartCoroutine(PrintDamageText(totalTrueDamage, DamageType.trueDamage));
        }
        if (totalManaDamage > 0)
        {
            CurrentMana -= totalManaDamage;
            StartCoroutine(PrintDamageText(totalManaDamage, DamageType.manaDamage));
        }
        if (totalIncreaseHp > 0)
        {
            totalIncreaseHp *= currentHealthRegen / 100;
            CurrentHealth += totalIncreaseHp;
            StartCoroutine(PrintDamageText(totalIncreaseHp, DamageType.IncreaseHp));
        }
        if (totalIncreaseMp > 0)
        {
            totalIncreaseMp *= currentManaRegen / 100;
            CurrentMana += totalIncreaseMp;
            StartCoroutine(PrintDamageText(totalIncreaseMp, DamageType.IncreaseMp));
        }

        CurrentMana += ((totalNormalDamage + totalTrueDamage) / maxHealth) * (currentManaRegen / 100) * 100; // 총 받은 체력 비례 피해량에 비례해 마나 회복
        InitBuff(damage); // 데미지 정보에 따라 버프 적용

        // 생명력 흡수가 달려있으면 데미지의 일부를 흡혈
        if (damage.sourceGameObject && damage.lifeSteal > 0)
        {
            Unit sourceUnit = damage.sourceGameObject.GetComponent<Unit>();
            Damage tempDamage = new Damage();
            tempDamage.increaseHp = (totalNormalDamage + totalTrueDamage) * damage.lifeSteal;
            sourceUnit.ApplyDamage(tempDamage);
        }
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
    // 공격 사정거리 안에 있는 유닛 체크
    void CheckEnenyInAttackDistance()
    {
        int layerMask = 1 << LayerMask.NameToLayer("BattleUnit");
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, currentAttackDistance / 100 - 0.25f, layerMask);
        List<GameObject> tempGameObjects = new List<GameObject>();
        foreach(Collider2D collider in hits)
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
            if (unit.gameObject.layer != LayerMask.NameToLayer("BattleUnit"))
                continue; // 때릴수 없는 상태인 대상 제외

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
            if (unit.gameObject.layer != LayerMask.NameToLayer("BattleUnit"))
                continue; // 때릴수 없는 상태인 대상 제외

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
            if (unit.gameObject.layer != LayerMask.NameToLayer("BattleUnit"))
                continue; // 때릴수 없는 상태인 대상 제외

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
            if (unit.gameObject.layer != LayerMask.NameToLayer("BattleUnit"))
                continue; // 때릴수 없는 상태인 대상 제외

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
    
    // 이동할 좌표 찾기
    void FindMovePoint()
    {
        movePoint = target.transform.position;
    }

    // 무브 포인트로 이동
    void MoveToMovePoint()
    {
        int sensorWidth = 18;
        float sensorDistnace = 0.5f;

        // 움직일 방향 앞에 장애물이 있는지 검사
        int layerMask = 1 << LayerMask.NameToLayer("BattleUnit");
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

            // 가장 가까운 벡터 두개 가져오기
            Vector2 closeVector1 = new Vector2();
            Vector2 closeVector2 = new Vector2();
            var angleDictionary = angles.OrderBy(x => x.Value);
            int i = 0;
            foreach(var dictionary in angleDictionary)
            {
                i++;
                if (i == 1)
                {
                    closeVector1 = dictionary.Key;
                }
                else if (i == 2)
                {
                    closeVector2 = dictionary.Key;
                }
                else
                {
                    break;
                }                
            }

            // 가장 가까운 두 벡터 중에서 현재 이동방향과 비슷한 방향으로 이동하기
            //Debug.Log(string.Format("벡터1:{0}, 벡터2{1}", closeVector1, closeVector2));
            if (closeVector1 != Vector2.zero && closeVector2 == Vector2.zero)
            {
                tempDirection = closeVector1;
            }
            else if (closeVector1 == Vector2.zero && closeVector2 != Vector2.zero)
            {
                tempDirection = closeVector2;
            }
            else if(closeVector1 != Vector2.zero && closeVector2 != Vector2.zero)
            {
                if (Vector2.Angle(rigidbody.velocity, closeVector1) < Vector2.Angle(rigidbody.velocity, closeVector2))
                {
                    tempDirection = closeVector1;
                }
                else
                {
                    tempDirection = closeVector2;
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
        // 경직 상태인지 검사
        if (!CheckRigid())        
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
                FindMovePoint();
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
        currentHealthRegen = (unitData.statusList[level-1].healthRegen + deltaHealthRegen) * multipHealthRegen;
        currentManaRegen = (unitData.statusList[level-1].manaRegen + deltaManaRegen) * multipManaRegen;
        currentAttackPower = (unitData.statusList[level-1].attackPower + deltaAttackPower) * multipAttackPower;
        currentSpellPower = (unitData.statusList[level-1].spellPower + deltaSpellPower) * multipAbilityPower;        
        currentAttackArmor = (unitData.statusList[level-1].attackArmor + deltaAttackArmor) * multipAttackArmor;
        currentSpellArmor= (unitData.statusList[level-1].spellArmor+deltaSpellArmor)*multipSpellArmor;
        currentAttackSpeed = (unitData.statusList[level-1].attackSpeed + deltaAttackSpeed) * multipAttackSpeed;
        currentWalkSpeed = (unitData.statusList[level-1].walkSpeed + deltaWalkSpeed) * multipWalkSpeed;
        currentAttackDistance = (unitData.statusList[level-1].attackDistance + deltaAttackDistance) * multipAttackDistance;
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
