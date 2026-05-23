using UnityEngine;

public class EnemyScript : MonoBehaviour
{

    [SerializeField] private int Reward = 100;

    private GameManager GM;
    private Damageble D;
    void Awake()
    {
        D = GetComponent<Damageble>();
        GM = FindAnyObjectByType<GameManager>();
    }

    private void OnDestroy()
    {
        if (D.CurrentHeath <= 0)
        {
            GM.Money += Reward;
        }
    }
}
