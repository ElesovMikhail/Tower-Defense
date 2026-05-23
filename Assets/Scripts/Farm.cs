using UnityEngine;

public class Farm : MonoBehaviour // компонент для фермы
{
    [SerializeField] int Food; // еда, которую даёт ферма
    [SerializeField] GameManager GM;
    private bool Installed = false; // флажок, подтверждающий, что здание установлено

    [SerializeField] GameObject Ogorod; // моделька огорода

    private void Awake()
    {
        GM = FindAnyObjectByType<GameManager>();
    }

    public void OnInstalled()
    {
        GM.MaxFood += Food; // прибавление еды
        Installed = true; // подтверждение, что здание установлено
        Ogorod.SetActive(true); // появление модельки огорода
    }

    private void OnDestroy()
    {
        if (Installed == true)
        {
            GM.MaxFood -= Food;
            //Debug.Log("Ферма уничтожена");
        }
    }
}
