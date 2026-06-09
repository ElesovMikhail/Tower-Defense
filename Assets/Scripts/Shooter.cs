using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Shooter : MonoBehaviour // компонент для стрелков-лучников; как вражеских, так и союзных
{
    NavMeshAgent Agent;

    [SerializeField] float Speed; // скорость передвижения
    [SerializeField] private int Damage; // урон
    [SerializeField] private LayerMask EnemyLayer; // слой противника
    [SerializeField] private float LookRadius; // радиус видимости
    [SerializeField] private float AttackRadius; // радиус атаки
    [SerializeField] private Transform ArrowSpawner; // точка спавна стрелы
    [SerializeField] GameObject MissilePrefab; // префаб (образец) снаряда
    [HideInInspector] public bool IsAttacking = false;

    [Space]
    [SerializeField] float AttackDelay; // задержка после атаки
    private float Timer;// таймер для перезарядки атаки
    [Space]
    [Space]
    [Space]
    private AudioSource Source;
    [Space]
    [Space]
    [Space]
    [SerializeField] private AudioClip ShootClip;
    [Space]
    [Space]
    [SerializeField] public GameObject ClosestEnemy;

    private Animator Anim;
    // скрипт для лучника почти полностью скопирован со скрипта компонента Warrior, поэтому я не вижу смысла повторно подробно описывать его алгоритм
    void Awake()
    {
        ArrowSpawner = transform.GetChild(0);
        Agent = GetComponent<NavMeshAgent>();
        Agent.speed = Speed;

        Anim = GetComponent<Animator>();
        Source = GetComponent<AudioSource>();
    }


    void Update()
    {
        ClosestEnemy = FindEnemy();
        if (ClosestEnemy == null)
        {
            if (Agent.hasPath) Agent.ResetPath();
            if (Anim != null) Anim.SetBool("Moving", false);
            return;
        }
        //Debug.Log("Противник найден");
        MoveToEnemy();
    }

    GameObject FindEnemy()
    {
        Collider[] EnemiesInRadius = Physics.OverlapSphere(transform.position, LookRadius, EnemyLayer);
        if (EnemiesInRadius.Length == 0)
        {
            //Debug.Log("Противник не найден");
            return null;
        }
        float[] Distances = new float[EnemiesInRadius.Length];
        for (int i = 0; i < EnemiesInRadius.Length; i++)
        {
            Distances[i] = Vector3.Distance(transform.position, EnemiesInRadius[i].transform.position);
        }
        GameObject CloseEnemy = EnemiesInRadius[Array.IndexOf(Distances, Distances.Min())].gameObject;
        return CloseEnemy;
    }


    void MoveToEnemy()
    {
        if (ClosestEnemy != null)
        {
            LookToEnemy();
            Agent.SetDestination(ClosestEnemy.transform.position);
            //if (Vector3.Distance(transform.position, ClosestEnemy.transform.position) <= AttackRadius)
            if (Physics.CheckSphere(transform.position, AttackRadius, EnemyLayer))
            {
                Agent.ResetPath();
                Attack();
            }
            if (Anim != null) Anim.SetBool("Moving", true);
        }
        else return;
    }

    void LookToEnemy()
    {
        Vector3 Direction = (ClosestEnemy.transform.position - transform.position).normalized;
        Quaternion LookRotation = Quaternion.LookRotation(new Vector3(Direction.x, 0, Direction.z));
        transform.rotation = LookRotation;
    }

    void Attack()
    {
        if (Agent.hasPath) Agent.ResetPath();
        if (ClosestEnemy != null) // если есть аниматор, происходит анимация выстрела с методом OnAttack
        {
            if (Anim != null)
            {
                Anim.SetBool("Moving", false);
                if (IsAttacking == false)
                {
                    Anim.SetTrigger("Shoot");
                    IsAttacking = true;
                    //Debug.Log("Анимация выстрела");
                }
            }
            else // если аниматор отсутствует, выстрел (спавн стрелы на сцену, назначение ей цели и урона) работает через таймер
            {
                if (Timer <= 0)
                {
                    GameObject Missile = Instantiate(MissilePrefab, ArrowSpawner.position, transform.rotation);
                    Missile.GetComponent<Arrow>().Damage = Damage;
                    Missile.GetComponent<Arrow>().Target = ClosestEnemy;
                    Missile.GetComponent<Arrow>().TargetIsDefined = true;
                    Missile.GetComponent<Arrow>().enabled = true;
                    //Debug.Log("Вытрел");
                    Timer = AttackDelay;
                }
                else Timer -= Time.deltaTime;
            }
        }
        else
        {
            return;
        }
    }

    public void OnAttack() // метод, который срабатывает при анимации выстрела
    {
        if (ClosestEnemy == null)
        {
            IsAttacking = false;
            return;
        }
        //if (Vector3.Distance(transform.position, ClosestEnemy.transform.position) > AttackRadius)
        if (!Physics.CheckSphere(transform.position, AttackRadius, EnemyLayer))
        {
            IsAttacking = false;
            Debug.Log("Противник слишком далеко");
            return;
        }
        GameObject Missile = Instantiate(MissilePrefab, ArrowSpawner.position, transform.rotation);
        Missile.GetComponent<Arrow>().Damage = Damage;
        Missile.GetComponent<Arrow>().Target = ClosestEnemy;
        Missile.GetComponent<Arrow>().TargetIsDefined = true;
        Missile.GetComponent<Arrow>().enabled = true;
        Source.PlayOneShot(ShootClip);
        IsAttacking = false;
        Debug.Log("Анимация выстрела");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, LookRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AttackRadius);
    }
}
