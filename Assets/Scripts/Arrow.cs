using UnityEngine;

public class Arrow : MonoBehaviour // автоматически наводящаяся на противника стрела, выпускаемая лучником
{
    [HideInInspector] public bool TargetIsDefined = false; // флажок, подтверждающий, что цель определена

    [HideInInspector] public GameObject Target; // цель для стрелы
    [HideInInspector] public int Damage; // урон, который стрела наносит

    [SerializeField, Min(0)] private float Speed = 30;


    private void Update()
    {
        if (TargetIsDefined)
        {
            if (Target == null) // если цель пропадёт, стрела пропадёт вместе с ней
            {
                //Debug.Log("Цель не определена");
                Destroy(gameObject);
            }
            else
            {
                Vector3 Direction = (Target.transform.position - transform.position).normalized; // определение вектора расстояния цели
                if (Direction != Vector3.zero) // если этот вектор нулевой, то значит, что стрела уже добралась до цели, и она может нанести ей урон, а затем уничтожиться
                {
                    Quaternion LookRotation = Quaternion.LookRotation(Direction);
                    transform.rotation = LookRotation;
                }
                else
                {
                    Target.GetComponent<IDamageble>().GetDamage(Damage);
                    //Debug.Log("Попадание");
                    Destroy(gameObject);
                }
                transform.position = Vector3.MoveTowards(transform.position, Target.transform.position, Speed * Time.deltaTime); // движение в сторону цели с определённой скоростью
                RaycastHit Hit;
                if (Physics.Raycast(transform.position, transform.forward, out Hit, 0.1f) && Hit.collider.gameObject == Target) // создаётся невидимый луч; если он направлен в цель на мизерном расстоянии, то стрела наносит урон
                {
                    Target.GetComponent<IDamageble>().GetDamage(Damage);
                    //Debug.Log("Попадание");
                    Destroy(gameObject);
                }
            }
        }
    }
    
    
}
