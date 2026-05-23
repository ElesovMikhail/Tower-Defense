using UnityEngine;

public class CameraZoom : MonoBehaviour // компонент для управления камерой
{
    private Camera mainCamera;

    [Header("Параметры перемещения")]
    [SerializeField] private float PanSpeed = 200f; // скорость движения камеры
    [SerializeField] private float MapHeight = 40f; // длина игрового поля
    [SerializeField] private float MapWidth = 40f; // ширина игрового поля
    [SerializeField] public bool Inverted = false; // флажок инвертирования управления
    private float Movement; // коэфициент движения; зависит от управления - инвертировано оно или нет

    // ограничения по передвижению камеры
    float MinX;
    float MaxX;
    float MinY;
    float MaxY;

    [Header("Параметры зума")]
    [SerializeField] private float ZoomSpeed = 5f; // скорость изменения зума
    [SerializeField] private float MinZoom = 2f; // минимальная кратность
    [SerializeField] private float MaxZoom = 15f; // максимальная кратность



    private void Awake()
    {
        Inverted = MainMenu.CameraInverted;

        MinX = 0;
        MaxX = MapWidth;
        MinY = -MapHeight;
        MaxY = MapHeight;


        mainCamera = GetComponent<Camera>();
    }

    void Update()
    {
        HandlePan();
        HandleZoom();
    }

    void HandlePan() // метод перемещения камеры
    {
        if (Input.GetMouseButton(1)) // если правая кнопка мыши нажата
        {
            // считывание движения мышки
            float MX = -Input.GetAxis("Mouse X");
            float MY = -Input.GetAxis("Mouse Y");

            if (Inverted == true) Movement = -1;
            else Movement = 1;

            // Определение вектора перемещения
            Vector3 Move = new Vector3(MX, 0, MY);
            Vector3 desiredPosition = transform.position + Move * PanSpeed * Movement * Time.deltaTime;

            
            // добавление ограничений по передвижению, чтобы камера не уходила далеко
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, MinX, MaxX);
            desiredPosition.z = Mathf.Clamp(desiredPosition.z, MinY, MaxY);

            
            transform.position = desiredPosition; // перемещение самой камеры

        }
    }

    void HandleZoom() // метод изменения зума камеры
    {
        float Scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Scroll != 0)
        {
            float NewSize = mainCamera.orthographicSize - Scroll * ZoomSpeed;
            mainCamera.orthographicSize = Mathf.Clamp(NewSize, MinZoom, MaxZoom);
        }
    }
}