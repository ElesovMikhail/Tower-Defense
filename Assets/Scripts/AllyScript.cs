using UnityEngine;

public class AllyScript : MonoBehaviour // компонент для союзных юнитов, которые выпускаются зданиями
{
    [SerializeField] public int Food; // еда, нужная юниту
    private GameManager GM;
    private Damageble D;
    [HideInInspector] public bool Released = false; // флажок, подтверждающий, что здание установлено
    void Awake()
    {
        D = GetComponent<Damageble>();
        GM = FindAnyObjectByType<GameManager>();
    }

    private void OnDestroy()
    {
        if (D.CurrentHeath <= 0 && Released == true)
        {
            GM.CurrentFood -= Food;
        }
    }
}