using UnityEngine;

public class Building : MonoBehaviour, IDamageble // компонент дл€ всех зданий
{
    [SerializeField] public int MaxHeath; // максимальное кол-во жизней
    [SerializeField] public int CurrentHeath; // текущее кол-во жизней

    [SerializeField, Min(0)] public int Price = 0; // ценник здани€


    [SerializeField] public Vector2Int Size = Vector2Int.one; // габариты здани€; настраиваютс€ в инспекторе

    [HideInInspector] public float BeginPosX; // лева€ нижн€€ клетка по оси x
    [HideInInspector] public float BeginPosY; // лева€ нижн€€ клетка по оси y
    // это нужно дл€ расчЄта размещени€

    private Renderer BuildingRenderer; // моделька здани€

    [HideInInspector] public bool IsActive = true; // флажок активности; выключаетс€ компонентом GameManager, когда игра окончена; предназначен он по большей части дл€ главной ратуши
    // он нужен дл€ того, чтобы здание после подтверждени€ победы перестало быть у€звимым



    [SerializeField] Color ValidPosColor; // цвет (оттенок зелЄного - #0B9D00), обозначающий приемлимую позицию дл€ установки здани€
    [SerializeField] Color NotValidPosColor; // цвет (оттенок красного - #C31E10), обозначающий Ќ≈приемлимую позицию дл€ установки здани€
    // просто красный и просто зелЄный мне не нрав€тс€

    [Space]
    [Space]
    [Space]
    [Space]
    [SerializeField] Color GridColor1;
    [SerializeField] Color GridColor2;
    [Space]
    [Space]
    [SerializeField] public AudioClip BuildingInstalled;
    [SerializeField] public AudioClip BuildingDestroyed;


    void Awake()
    {
        // вычисление позиции левой нижней клетки
        BeginPosX = (float)Size.x / 2 + 0.5f - Size.x;
        BeginPosY = (float)Size.y / 2 + 0.5f - Size.y;

        BuildingRenderer = transform.GetChild(0).GetComponent<Renderer>();
        CurrentHeath = MaxHeath;
    }


    public void SetTransparent(bool ValidPos) // метод окрашивани€ ещЄ не установленного здани€
    {
        if (ValidPos) BuildingRenderer.material.color = ValidPosColor;
        else BuildingRenderer.material.color = NotValidPosColor;
    }

    public void SetNormalMaterial() // метод окрашивани€ здани€ в его естественный цвет
    {
        BuildingRenderer.material.color = Color.white;
    }


    public void GetDamage(int Damage)
    {
        CurrentHeath -= Damage;
        if (CurrentHeath <= 0)
        {
            GameManager GM = FindAnyObjectByType<GameManager>();
            GM.PlaySound(BuildingDestroyed);
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        //Debug.Log($"{(float)Size.x / 2 + 0.5f - Size.x}, {(float)Size.y / 2 + 0.5f - Size.y}");
        for (float x = (float)Size.x / 2 + 0.5f - Size.x; x < Size.x / 2 + 0.5f; x ++)
        {
            for (float y = (float)Size.y / 2 + 0.5f - Size.y; y < Size.y / 2 + 0.5f; y ++)
            {
                if ((int)((x + y) % 2) == 0) Gizmos.color = GridColor1;
                else Gizmos.color = GridColor2;
                Gizmos.DrawCube(transform.position + new Vector3(x, 0, y), new Vector3(1, 0.1f, 1));
            }
        }
    }
}
