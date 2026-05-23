using UnityEngine;

public class Shield : MonoBehaviour, IDamageble // компонент для щита; в инспекторе стоит выше основного компонента IDamageble
{
    [SerializeField] private GameObject VisualShield; //моделька щита
    [SerializeField] private int MaxShieldStrength; //максимальная прочность щита
    private int CurrentStrength; //текущая прочность щита
    private Damageble DamagebleUnit; // основной компонент для получения урона
    void Start()
    {
        DamagebleUnit = GetComponent<Damageble>();
        DamagebleUnit.enabled = false; //выключение основной компонент для получения урона (на всякий случай)
        CurrentStrength = MaxShieldStrength;
    }

    public void GetDamage(int Damage)
    {
        CurrentStrength -= Damage;
        if (CurrentStrength <= 0)
        {
            Debug.Log("Щит разрушен");
            DamagebleUnit.enabled = true; //включение основной компонент для получения урона
            Destroy(VisualShield); // уничтожение модельки щита
            Destroy(this); // уничтожение самого компонента
        }
    }
}
