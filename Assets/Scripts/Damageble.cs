using UnityEngine;


public class Damageble : MonoBehaviour, IDamageble // скрипт для контроля здоровья юнитов; как вражеских, так и наших
{
    [SerializeField] private int MaxHeath; // максимальное кол-во жизней
    public int CurrentHeath; // текущее кол-во жизней

    void Awake()
    {
        CurrentHeath = MaxHeath;
    }

    public void GetDamage(int Damage) // метод получения урона
    {
        CurrentHeath -= Damage;
        if (CurrentHeath <= 0)
            Destroy(gameObject);
    }
}