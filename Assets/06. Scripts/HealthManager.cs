using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HealthManager : MonoBehaviour
{

    public enum EnemyState
    {
        idle,
        trace,
        attack,
        die
    };
    public EnemyState enemyState = EnemyState.idle;                       // 적 현재 상태 정보를 저장할 Enum 변수

    [Header("EnemySetting")]
    public float hitPoint = 100f;                                         // 적 체력 설정
    public float traceDistance = 20.0f;                                   // 추적 사정거리
    public float attackDistance = 8.0f;                                   // 공격 사정거리
    public GameObject bullet;                                             // 총알 담는 변수

    [Header("Sound")]
    AudioSource audioSource;                                             // 사운드 설정
    public AudioClip DieSound;                                           // 죽는 소리 설정

    private Transform enemyTransform;                                        // 적 위치
    private Transform playerTransform;                                   // 적이 따라갈 타겟 설정
    private NavMeshAgent nav;                                            // NavMesh 선언
    private Animator animator;                                           // 적 애니메이터 선언
    private bool isDie = false;                                          // 적 사망 여부

    public Bullet enemyBullet;                                          // Bullet Script 호출
    [SerializeField] float coolDownTime = 4f;                                      // 공격 중 딜레이
    [SerializeField] float nextFireTime = 0f;                                      // 공격 중 딜레이

    // 절대강좌 유니티 Script 추가
    private readonly int hashFire = Animator.StringToHash("isFire");

    private float nextFire = 0.0f;                                      // 총을 발사하고 딜레이
    private readonly float fireRate = 0.1f; 
    private readonly float damping = 10.0f;                             // 회전값을 저장하기 위한 변수
    public bool isFire = false;                                         // 활성화 되있으면 총을 발사
    /*
    // Add
    public LayerMask whatIsGround, whatIsPlayer;
    // Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    // Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;

    // States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;
    */

    void Awake()
    {
        nav = this.gameObject.GetComponent<NavMeshAgent>();                                             // NavMeshAnent 컴포넌트 할당
        this.audioSource = GetComponent<AudioSource>();
    }
    void Update()
    {
        if (nav.enabled)
        {
            nav.SetDestination(playerTransform.position);                                   // 도착할 목표 위치 지정함수
            animator.SetBool("isTrace", true);
            // false? true?
            nav.isStopped = true;
        }

        // 격발 코드
        if(isFire)
        {
            // Time.time : 스크립트가 실행 됐을 때부터 흘러가는 시간.
            // nextFire에는 Time.time + delay time 이 들어감
            if(Time.time >= nextFire)
            {
                EnemyFire();
                // 랜덤한 딜레이를 추가하여 자연스럽게 만듦
                nextFire = Time.time + fireRate + Random.Range(0.0f, 0.4f);
            }
        }

        // 쿼터니언을 이용하여 플레이어를 바라보게 만든다 (만약되면 카메라 싱글톤 삭제)
        Quaternion rot = Quaternion.LookRotation(playerTransform.position - enemyTransform.position);
        /* 
         새로운 코드가 되면, 주석안 코드 삭제
        // Check for Sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        // 플레이어가 범위안에 있지만 공격 범위에 있지 않은 경우 적은 순찰
        if (!playerInSightRange && !playerInAttackRange)
        {
            Patroliing();
        }
        // 플레이어가 범위안에 있으나 공격 범위 없으면 추격
        if (playerInSightRange && !playerInAttackRange)
        {
            ChasePlayer();
        }
        // 플레이어가 범위안, 공격범위면 공격
        if (playerInSightRange && playerInAttackRange)
        {
            AttackPlayer();
        }
        */
    }

    void Start()
    {
        enemyTransform = this.gameObject.GetComponent<Transform>();
        playerTransform = GameObject.FindWithTag("Player").GetComponent<Transform>();                   // Tag에 지정해둔 "Player"를 따라가게 함
        animator = GetComponent<Animator>();                                                            // Animator 컴포넌트 할당

        // 추적 대상의 위치를 설정하면 바로 추적 시작.
        nav.destination = playerTransform.position;

        // 코루틴 선언
        StartCoroutine(this.CheckState());                                                              // 일정 간격으로 적 행동 상태를 체크하는 코루틴 함수 실행
        StartCoroutine(this.CheckStateForAction());                                                     // 적 상태에 따라 동작하는 루틴을 실행하는 코루틴 함수 실행
    }
    public void ApplyDamage(float damage)
    {
        hitPoint -= damage;

        if (hitPoint <= 0)
        {
            EnemyDie();
            // 죽었으니, Look At Camera 스크립트 끔
            gameObject.GetComponent<LookAtCamera>().enabled = false;
            //적이 죽으면 nav 멈추고 업데이트 포지션과 로테이션 끔.
            //animator.enabled = false;
            //nav.enabled = false;
            //this.nav.isStopped = false;

            // 적이 자연스럽게 죽는 것을 위해서 선언
            this.nav.updatePosition = false;
            this.nav.updateRotation = false;

        }
    }

    IEnumerator CheckState()
    {
        while (isDie == false)
        {
            // 0.2 대기 후 다음으로 넘어감
            yield return new WaitForSeconds(0.2f);

            // 적과 플레이어와의 거리 측정
            float distance = Vector3.Distance(playerTransform.position, enemyTransform.position);

            // 사정거리안에 들어오면 공격 && !FindObjectOfType<GameManager>().isGameOver
            if (distance <= attackDistance)
            {
                nav.enabled = true;                                                       // Add
                enemyState = EnemyState.attack;
            }
            // 사정거리안에 들어오면 추적
            else if (distance <= traceDistance)
            {
                enabled = true;
                enemyState = EnemyState.trace;
            }
            // 아니면 서있음
            else
            {
                nav.enabled = false;                                                       // Add
                enemyState = EnemyState.idle;
            }
        }
    }

    //
    IEnumerator CheckStateForAction()
    {

        while (isDie == false)
        {
            switch (enemyState)
            {
                // idle 상태
                case EnemyState.idle:
                    nav.enabled = true;
                    animator.SetBool("isTrace", false);
                    break;

                // 추적 상태
                case EnemyState.trace:
                    //nav.destination = playerTransform.position;                             // 추적 대상의 위치를 넘겨줌
                    nav.enabled = false;
                    animator.SetBool("isFire", false);                                      // false >> 공격하지 않음
                    animator.SetBool("isTrace", true);                                      // 변수값을 true로 설정 (따라가야 함 : Run 동작)
                    break;

                // 공격 상태
                case EnemyState.attack:
                    nav.enabled = false;
                    // nav.isStopped = true;
                    animator.SetBool("isFire", true);                                       // true로 설정해 attack state로 전이
                    break;
            }
            yield return null;
        }
    }

    // 적이 사망시 처리 루틴
    void EnemyDie()
    {
        if (isDie == true)
        {
            return;
        }

        // 적이 죽을 때 사운드 호출
        audioSource.clip = DieSound;
        audioSource.Play();

        // 모든 코루틴을 정지
        StopAllCoroutines();
        isDie = true;
        enemyState = EnemyState.idle;
        nav.enabled = true;                                                               // 사망하였으니 NavMesh 추적 끔
        animator.SetTrigger("doDie");

        // 적에게 추가된 Collider를 비활성화 (안 없애면 죽은 후에 총을 맞음)
        gameObject.GetComponentInChildren<CapsuleCollider>().enabled = false;

        //// 적 내부에 있는 박스 콜라이더도 다 끔
        //foreach (Collider coll in gameObject.GetComponentsInChildren<BoxCollider>())
        //{
        //    coll.enabled = false;
        //}
    }
    void EnemyFire()
    {
        animator.SetTrigger(hashFire);
        // 사운드 넣으려면 여기에
        //audioSource.PlayOneShot(사운드, 1.0f);
    }
    /*
     *
    void StartAttacking()
    {
        if (nextFireTime < Time.time)
        {
            Bullet instance = Instantiate(enemyBullet, transform.position, transform.rotation);
            instance.SetInstantiator(tag);
            nextFireTime = coolDownTime += Time.time;
        }
    }
    */
    
    /*
    // 적 순찰 상태
    void Patroliing()
    {
        if(!walkPointSet)
        {
            SearchWalkPoint();
        }
        if(walkPointSet)
        {
            nav.SetDestination(walkPoint);
        }

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        // Walkpoint reached. 도보 지점 거리가 1미만인 경우, 도보 지점에 도달하고 도보 지점 세트는 다시 Flase
        if(distanceToWalkPoint.magnitude < 1f)
        {
            walkPointSet = false;
        }
    }
    void SearchWalkPoint()
    {
        // Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if(Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
        {
            walkPointSet = true;
        }
    }
    //플레이어 추적
    void ChasePlayer()
    {
        nav.SetDestination(playerTransform.position);
    }

    // 플레이어 공격
    void AttackPlayer()
    {
        // Make sure enemy doesn't move
        //nav.SetDestination(transform.position);

        transform.LookAt(playerTransform);

        if(!alreadyAttacked)
        {
            // 공격 코드
            Rigidbody rigidbody = Instantiate(bullet, transform.position, Quaternion.identity.normalized).GetComponent<Rigidbody>();
            // 탄알이 나가는 방향
            rigidbody.AddForce(transform.forward * 32f, ForceMode.Impulse);
            //rigidbody.AddForce(transform.up * 8f, ForceMode.Impulse);                               // 이 부분 필요 없을 듯

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }
    // 공격 재설정
    void ResetAttack()
    {
        alreadyAttacked = false;
    }
    */
}
