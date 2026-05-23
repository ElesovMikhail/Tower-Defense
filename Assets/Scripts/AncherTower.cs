using System;
using System.Linq;
using UnityEngine;

public class AncherTower : MonoBehaviour // компонент для башни с лучником
{
    [SerializeField] int Damage = 50; // урон
    [SerializeField] GameObject Ancher; // лучник на башне; по началу выключен
    [SerializeField] GameObject ArrowPrefab; // префаб стрелы
    [SerializeField] Transform ArrowSpawner; // точка спавна стрелы
    [SerializeField] LayerMask EnemyLayer; // слой противников
    [SerializeField] float LookRadius = 40; // радиус видимости
    [SerializeField] GameObject ClosestEnemy; // ближайший противник
    private Shooter AncherComp; // компонент Shooter у лучника

    private Animator Anim; // компонент Animator у лучника

    [Space]
    [SerializeField] float AttackDelay = 2; // задержка перед выстрелом
    private float Timer;

    private void Awake()
    {
        Anim = Ancher.GetComponent<Animator>();
        Anim.enabled = true;
        AncherComp = Ancher.GetComponent<Shooter>();
    }

    public void OnInstalled()
    {
        Ancher.SetActive(true);
    }

    void Update()
    {
        ClosestEnemy = FindEnemy();
        if (ClosestEnemy == null)
        {
            return;
        }
        LookToEnemy();
        Attack();
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

    void LookToEnemy()
    {
        Vector3 Direction = (ClosestEnemy.transform.position - transform.position).normalized;
        Quaternion LookRotation = Quaternion.LookRotation(new Vector3(Direction.x, 0, Direction.z));
        Ancher.transform.rotation = LookRotation;
    }

    void Attack()
    {
        if (ClosestEnemy != null)
        {
            if (Anim != null) // если у лучника есть компонент аниматор, то просто запускается анимация выстрела лучника, в которой запускается метод OnAttack
            {
                AncherComp.ClosestEnemy = ClosestEnemy; // назначение ближайшего противника лучнику
                if (AncherComp.IsAttacking == false)
                {
                    AncherComp.IsAttacking = true;
                    Anim.SetTrigger("Shoot");
                    //Debug.Log("Башня стреляет");
                }
            }
            else // если же аниматора нет, то атака работает через таймер
            {
                if (Timer <= 0)
                {
                    GameObject Missile = Instantiate(ArrowPrefab, ArrowSpawner.position, transform.rotation);
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, LookRadius);
    }
}
