using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Warrior : MonoBehaviour // компонент для юнитов ближнего боя - пехотинцев и конных рыцарей; как вражеских, так и союзных
{
    NavMeshAgent Agent;

    [SerializeField] float Speed; // скорость передвижения
    [SerializeField] private int Damage; // урон
    [SerializeField] private LayerMask EnemyLayer; // слой противника
    [SerializeField] private float LookRadius; // радиус видимости
    [SerializeField] private float AttackRadius; // радиус атаки
    private bool IsAttacking = false;

    [Space]
    [SerializeField] float AttackDelay; // задержка после атаки
    private float Timer;// таймер для перезарядки атаки
    [Space]
    [Space]
    [Space]
    private AudioSource Source;
    [SerializeField] private AudioClip HitClip;
    [Space]
    [Space]

    [SerializeField] private GameObject ClosestEnemy; // ближайший противник

    private Animator Anim;

    void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Agent.speed = Speed;

        Anim = GetComponent<Animator>();
        Source = GetComponent<AudioSource>();
    }

    void Update()
    {
        ClosestEnemy = FindEnemy(); // поиск ближайшего противника
        if (ClosestEnemy == null) // если противник не находится, то метод обрывается, и юнит стоит на месте
        {
            if (Agent.hasPath) Agent.ResetPath();
            if (Anim != null) Anim.SetBool("Moving", false);
            return;
        }
        //Debug.Log("Противник найден");
        MoveToEnemy(); // если противник находится, то юнит двигается в его сторону
    }

    GameObject FindEnemy() // метод нахождения ближайшего противника
    {
        Collider[] EnemiesInRadius = Physics.OverlapSphere(transform.position, LookRadius, EnemyLayer); // создание списка коллизий, которые попадают в радиус видимости, и которые имеют соответствующий слой
        if (EnemiesInRadius.Length == 0) // если этот список пустой, метод возвращает значение null и обрывается
        {
            //Debug.Log("Противник не найден");
            return null;
        }
        float[] Distances = new float[EnemiesInRadius.Length]; // создание списка дистанций
        for (int i = 0; i < EnemiesInRadius.Length; i++)
        {
            Distances[i] = Vector3.Distance(transform.position, EnemiesInRadius[i].transform.position);
        }
        GameObject CloseEnemy = EnemiesInRadius[Array.IndexOf(Distances, Distances.Min())].gameObject; // определение ближайшего противника, по индексу минимальной дистанции
        return CloseEnemy;
    }


    void MoveToEnemy()
    {
        if (ClosestEnemy != null)
        {
            LookToEnemy(); // поворот в сторону противника
            Agent.SetDestination(ClosestEnemy.transform.position); // сокращение дистанции спротивником
            if (Physics.CheckSphere(transform.position, AttackRadius, EnemyLayer)) // когда дистанция сократилась до радиуса атаки, юнит останавливается и атакует
            {
                //Debug.Log("Дошёл");
                Agent.ResetPath();
                Attack();
            }
            if (Anim != null) Anim.SetBool("Moving", true);
        }
        else return;
    }

    void LookToEnemy() // метод поворота юнита к противнику
    {
        Vector3 Direction = (ClosestEnemy.transform.position - transform.position).normalized;
        Quaternion LookRotation = Quaternion.LookRotation(new Vector3(Direction.x, 0, Direction.z));
        transform.rotation = LookRotation;
    }

    void Attack() // метод атаки
    {
        if (Agent.hasPath) Agent.ResetPath();
        if (ClosestEnemy != null) // если у война есть компонент аниматор, то просто запускается анимация атаки, в которой запускается метод OnAttack
        {
            if (Anim != null)
            {
                Anim.SetBool("Moving", false);
                if (IsAttacking == false)
                {
                    Anim.SetTrigger("Attack");
                    Debug.Log("Анимация удара");
                    IsAttacking = true;
                }
            }
            else // если же аниматора нет (есть просто тестовые префабы юнитов, которые не имею ни моделек, ни анимаций), то атака работает через таймер
            {
                if (Timer <= 0)
                {
                    ClosestEnemy.GetComponent<IDamageble>().GetDamage(Damage);
                    //Debug.Log("Удар");
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

    public void OnAttack() // метод, который срабатывает при анимации атаки
    {
        if (ClosestEnemy == null)
        {
            IsAttacking = false;
            return;
        }
        ClosestEnemy.GetComponent<IDamageble>().GetDamage(Damage);
        Source.PlayOneShot(HitClip);
        IsAttacking = false;
        Debug.Log("Удар");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, LookRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AttackRadius);
    }
}
