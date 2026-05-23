using Unity.VisualScripting;
using UnityEngine;

public class MainTownHall : MonoBehaviour // компонент для главной ратуши
{

    private GameManager GM; // ссылка на GameManager
    Building BuildComp; // компонент Building


    void Awake()
    {
        GM = FindAnyObjectByType<GameManager>();
        BuildComp = GetComponent<Building>();
    }

    private void OnDestroy() // метод, срабатывающий, когда ратуша уничтожается
    {
        if (BuildComp.CurrentHeath <= 0 && BuildComp.IsActive == true) // если уничтожение связано с разрушением (нету жизней), ратуша вызывает у компонента GameManager метод GameOver(), и игра на этом прекращается
        {
            GM.GameOver();
        }
    }
}
