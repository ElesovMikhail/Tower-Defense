using UnityEngine;

public class Barn : MonoBehaviour // компонент для хлева с лошадьми
{
    [SerializeField] GameManager GM;
    private bool Installed = false; // флажок, подтверждающий, что здание установлено
    private void Awake()
    {
        GM = FindAnyObjectByType<GameManager>();
    }

    public void OnInstalled()
    {
        Installed = true;
    }
    void Update()
    {
        GM.HasHorses = true;
    }

    private void OnDestroy()
    {
        if (Installed == true)
        {
            GM.HasHorses = false;
            Debug.Log("Хлев уничтожен");
            Destroy(gameObject);
        }
    }
}
