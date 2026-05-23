using UnityEngine;
using UnityEngine.AI;

public class UnitSpawner : MonoBehaviour // компонент для зданий, которые спавнят юнитов
{
    [SerializeField] public GameObject Unit; // префаб (образец) юнита, которого будет выпускать здание
    [HideInInspector] public int NeedFood; // нужное кол-во еды, для юнита; выводится из самого префаба юнита
    [SerializeField] Transform SpawnPoint; // точка спавна юнита
    [SerializeField] private bool NeedHorses; // нужна ли лошадь юниту (если этот юнит - конный рыцарь)
    [SerializeField] public int ButtonIndex; // индекс кнопки для компонента GameManager

    // необязательные компоненты
    [SerializeField] private BoxCollider DopCollider; // дополнительный коллайдер; нужен он только для барака с лучниками, так как он имеет Г-образную форму; другим зданиям это не надо
    [SerializeField] private NavMeshObstacle DopNav; // дополнительный компонент NavMeshObstacle

    GameManager GM;

    private void Awake()
    {
        NeedFood = Unit.GetComponent<AllyScript>().Food;
        GM = FindAnyObjectByType<GameManager>();
    }

    private void OnEnable()
    {
        // при включении компонента включаются дополнительные BoxCollider и NavMeshObstacle, если таковые имеются
        if (DopCollider != null) DopCollider.enabled = true;
        else return;
        if (DopNav != null) DopNav.enabled = true;
        else return;
    }

    public void SpawnUnit() // метод спавна юнита
    {
        // проверка условий, есть ли нужное кол-во еды, и нужны ли лошади; если условия не удовлетворяются - проигрывается звук ошибки, и метод обрывается
        if (GM.CurrentFood >= GM.MaxFood)
        {
            GM.PlaySound(GM.ErrorBuilding);
            return;
        }
        if (NeedHorses == true)
        {
            if (GM.HasHorses == false)
            {
                GM.PlaySound(GM.ErrorBuilding);
                return;
            }
        }

        // установка юнита
        GameObject unit = Instantiate(Unit, SpawnPoint.position, Quaternion.Euler(0, 180, 0));
        unit.GetComponent<AllyScript>().Released = true;
        GM.CurrentFood += NeedFood;
    }
}
