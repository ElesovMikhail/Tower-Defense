using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour // компонент, отвечающий за игровую логику
{
    private Camera Camera;
    private CameraZoom Zoom;

    private bool FPS; // флажок, разрешающий отображать текст для FPS

    private enum CameraMode { ViewMode = 1, InstallingBuildingsMode = 2, NeutralMode = 3 }; // режимы работы камеры и их индексы
    // ViewMode - режим перемещения и выделения юнитов и зданий; InstallingBuildingsMode - режим строительства; NeutralMode - нейтральный режим

    [Header("Основные игровые параметры")] // атрибут Header создаёт заголовки для компонента в окне инспектора; очень помогает, когда настраиваемых полей довольно много
    [SerializeField] private CameraMode Mode = CameraMode.ViewMode; // переключатель режима камеры
    private int LevelIndex;
    [SerializeField, Min(0)] public int Money = 500; // начальное кол-во монет; атрибут Min задаёт минимальное значение, меньше него число быть не может
    [SerializeField, Min(0)] public int MaxFood = 0; // начальное кол-во еды
    [SerializeField, Min(0)] public int CurrentFood = 0; // текущее кол-во еды
    // все эти ограничения я устанавливаю для себя, чтобы я случайно не поставил неприемлемое значение
    [SerializeField] public bool HasHorses = false; // флажок, показывающий, есть ли лошади; по началу хлевов с лошадьми нет, поэтому он равен значению false
    [SerializeField] private float BeginDelay = 4; // задержка перед началом запуска волн, чтобы дать игроку возможность подготовиться
    [Space]
    [Space]

    private EnemyManager EM; // компонент EnemyManager; поначалу он выключен
    [Space]
    [Space]
    [SerializeField] private GameObject FlyingUnit; //выбранный юнит для переноса
    [SerializeField] private GameObject SelectedBuilding; // выделенное здание
    [Space]
    [Space]

    [Header("Компонеты для строительной сетки")]
    [SerializeField] public Vector2Int GridSize = new Vector2Int(10, 10); // размеры строительной сетки; размеры можно менять в инспекторе
    private GameObject[,] Grid; // сама сторительная сетка; двумерный массив
    [SerializeField] public GameObject[] BuildingPrefab; // список префабов (образцов) зданий
    [Space]
    [Space]
    [SerializeField] private GameObject FlyingBuilding; // выбранное здание для установки; сделал на всякий случай отображаемым
    [Space]
    [SerializeField] private GameObject MainTownHall; // главная ратуша
    [Space]
    [Space]
    [Space]
    [SerializeField] private bool TurnOnEM = true; // флажок для включения компонента EnemyManager; по умолчанию он равен true, но иногда полезно его выключать
    [Space]
    [Space]
    [Space]
    [SerializeField] Color GridColor1;
    [SerializeField] Color GridColor2;


    [Header("Элементы UI")]
    [SerializeField] GameObject ViewPanel; // основная панель для работы с ViewMode
    [SerializeField] GameObject BuildingPanel; // панель для строительства
    [Space]
    [SerializeField] GameObject HintsControlText; // подсказки для управления
    [SerializeField] GameObject HintsFoodText; // подсказка для еды
    [SerializeField] GameObject HintsBuildingText; // подсказка для зданий
    [SerializeField] GameObject CloseButton; // кнопка, чтобы скрыть кнопку спавна юнита
    [SerializeField] GameObject[] UnitButtons; // кнопки спавна юнитов;
    private GameObject UnitButton; // кнопка выбранного здания, которое спавнит юнитов; выявляется из списка UnitButtons по индексу из компонента UnitSpawner у выбранного здания

    [Space]
    [SerializeField] private TextMeshProUGUI MoneyText; // счётчик монет
    [SerializeField] private TextMeshProUGUI FoodText; // счётчик еды
    [SerializeField] private TextMeshProUGUI FPSText; // счётчик кадров
    [Space]
    [SerializeField] GameObject GamePanel; // основная игровая панель
    [SerializeField] GameObject PausePanel; // панель для паузы
    [SerializeField] GameObject EndPanel; // конечная панель
    [Space]
    [SerializeField] GameObject NextLevelButton; // кнопка для перехода на следующий уровень
    [Space]
    [Space]
    [SerializeField] GameObject MessageBox;
    [Space]
    [Space]
    // UI виджеты для настроек
    [SerializeField] private Slider MusicSlider;
    [SerializeField] private Slider SoundSlider;
    [SerializeField] private Toggle FullScreenToogle;
    [SerializeField] private Toggle FPSToogle;
    [SerializeField] private Toggle CameraInvertedToogle;

    [Header("Аудио")]
    [SerializeField] private AudioMixer Audio; // аудиомиксер
    [SerializeField] public AudioClip ErrorBuilding;
    [Space]
    [SerializeField] public AudioClip BuildingIsInstalled;
    [SerializeField] public AudioClip BuildingIsDestroyed;
    [SerializeField] public AudioClip VictoryClip;
    [Space]
    [SerializeField] public AudioSource SoundSource;
    [SerializeField] public AudioSource MusicSource;

    void Start()
    {
        LevelIndex = SceneManager.GetActiveScene().buildIndex;
        FPS = MainMenu.FPS;
        FPSText.gameObject.SetActive(FPS);

        Grid = new GameObject[GridSize.x, GridSize.y]; // инициализация строительной сетки

        Camera = Camera.main;
        Zoom = Camera.GetComponent<CameraZoom>();

        LoadSettings(); // загрузка настроек


        FlyingBuilding = MainTownHall;
        InstallBuilding(GridSize.x / 2, GridSize.y / 2, MainTownHall);

        EM = GetComponent<EnemyManager>();
        EM.CreateStartWaves(); // создание волн
        // да, представте себе, можно вызывать публичные методы с выключенных компонентов, и они будут работать
        Invoke("EnableEM", BeginDelay); // запуск волн с задердкой
    }

    private void EnableEM()
    {
        EM.enabled = TurnOnEM; // включение компонета EnemyManager
        EM.BeginWaves(); // запуск волн
    }

    void Update()
    {
        switch(Mode) // те или иные функции включаются в зависимости от режима камеры
        {
            case CameraMode.ViewMode: // в режиме выделения включается контроллер камеры и вызывается метод ViewMode
                Zoom.enabled = true;
                ViewPanel.SetActive(true);
                BuildingPanel.SetActive(false);
                ViewMode();
                break;
            case CameraMode.InstallingBuildingsMode: // в режиме выделения контроллер камеры выключается и вызывается метод InstallingBuildingMode
                Zoom.enabled = false;
                ViewPanel.SetActive(false);
                BuildingPanel.SetActive(true);
                InstallingBuildingMode();
                break;
            default: // в нейтральном режиме контроллер камеры выключается; в этот момент от игрока уже ничего не требуется
                Zoom.enabled = false;
                break;
        }

        // обновления счётчиков
        ShowFPS();
        UpdateCounters();
    }


    // методы, отвечающие за механики
    public void Victory() // метод подтверждающий победу в игре; вызывается компонетом EnemyManager, когда противники последней волны заканчиваются, и главная ратуша уцелела
    {
        MainTownHall.GetComponent<Building>().IsActive = false;
        Debug.Log("Противники кончились. Победа!");
        MusicSource.PlayOneShot(VictoryClip);
        GamePanel.SetActive(false);
        BuildingPanel.SetActive(false);
        SwitchCameraMode(3);
        Debug.Log($"уровней: {PlayerPrefs.GetInt("AvailableLevels")}, уровень: {LevelIndex}");
        // обновление счётчика доступных уровней
        if (!PlayerPrefs.HasKey("AvailableLevels"))
        {
            PlayerPrefs.SetInt("AvailableLevels", LevelIndex + 1);
        }
        else
        {
            int AL = PlayerPrefs.GetInt("AvailableLevels");
            if (AL == LevelIndex)
            {
                PlayerPrefs.SetInt("AvailableLevels", LevelIndex + 1);
                Debug.Log("уровень записывается");
            }
        }



        Invoke("APV", 4);
    }

    public void GameOver() // метод подтверждающий поражение в игре; вызывается главной ратушей, когда та разрушается
    {
        SwitchCameraMode(3);
        GamePanel.SetActive(false);
        BuildingPanel.SetActive(false);

        if (EM.ArcadeMode) // запись кол-ва отражённых волн, если включен аркадный режим
        {
            int WI = EM.WaveIndex;
            if (PlayerPrefs.HasKey("WavesInArcade"))
            {
                if (EM.WaveIndex > PlayerPrefs.GetInt("WavesInArcade")) PlayerPrefs.SetInt("WavesInArcade", WI);
            }
            else
            {
                PlayerPrefs.SetInt("WavesInArcade", WI);
            }

            GameObject WaveText = EndPanel.transform.GetChild(2).gameObject;
            WaveText.SetActive(true);
            WaveText.GetComponent<TextMeshProUGUI>().text = $"отражено волн: <color=#CD3C19>{WI}</color>";
        }
        EM.StopWaves();
        Debug.Log("Игра окончена. Поражение.");
        Invoke("APGO", 4);
    }

    void APV()
    {
        ActiveEndPanel("Победа");
        if (NextLevelButton != null) NextLevelButton.SetActive(true);
    }

    void APGO()
    {
        ActiveEndPanel("<color=red>Поражение</color>");
        if (NextLevelButton != null) NextLevelButton.SetActive(false);
    }

    void ActiveEndPanel(string Value) // появление конечной панели
    {
        EndPanel.SetActive(true);
        TextMeshProUGUI T = EndPanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        T.text = Value;
    }



    public void SwitchCameraMode(int CurrentMode) // метод переключения режима работы камеры; вызывается по нажатию кнопки
    {
        ReleaseUnit();
        SelectedBuilding = null;
        Destroy(FlyingBuilding);
        Mode = (CameraMode)CurrentMode;

        HintsBuildingText.SetActive(false);
    }


    public void ViewMode() // метод управления и выделения
    {
        UnitCursor();
        if (Input.GetMouseButtonDown(0))
        {
            Ray CameraRay = Camera.ScreenPointToRay(Input.mousePosition); // создание невидимого луча, который направлен из камеры на место, в которое смотрит курсор
            RaycastHit Hit;
            
            if (Physics.Raycast(CameraRay, out Hit) && !EventSystem.current.IsPointerOverGameObject()) // если луч упирается в какое-то место, и это не какой-либо UI элемент, то происходит проверка, в кого попали
            {
                GameObject SelectedObject = Hit.collider.gameObject;
                
                if (FlyingUnit != null) // если за курсором уже летает юнит, то он сбрасывается
                {
                    ReleaseUnit();
                }
                else
                {
                    if (SelectedObject.TryGetComponent<Damageble>(out Damageble damageble) && SelectedObject.layer == 7) // если луч попал в юнита, и этот юнит - наш союзник, то его можно перетаскивать
                    // слой №7 - "Ally" (Союзник); вражеские юниты перетаскиваться не должны
                    {
                        TakeUnit(SelectedObject);
                    }
                }

                if (SelectedObject.TryGetComponent<Building>(out Building build)) // если луч попал в здание, которое может выпускать юнитов, то из его компонента UnitSpawner извлекается число ButtonIndex, которое, как ни сложно догадаться, служит индексом для включение кнопки из списка UnitButtons
                {
                    if (SelectedObject.TryGetComponent<UnitSpawner>(out UnitSpawner Spawner) == false) return;
                    if (SelectedBuilding != null)
                    {
                        CloseSpawnButton();
                    }
                    SelectedBuilding = SelectedObject;
                    ActiveSpawnButton();
                }
            }
        }
        if (SelectedBuilding == null) CloseSpawnButton();
    }

    private void TakeUnit(GameObject unit) // взять юнита
    {
        if (FlyingUnit != null) return;
        // запись юнита в поле FlyingUnit и выключение всех компонентов у юнита, чтоб не мешался, когда летал за курсором
        FlyingUnit = unit;
        FlyingUnit.GetComponent<Damageble>().enabled = false;
        FlyingUnit.GetComponent<CapsuleCollider>().enabled = false;
        FlyingUnit.GetComponent<NavMeshAgent>().enabled = false;
        if (FlyingUnit.TryGetComponent<Warrior>(out Warrior W)) W.enabled = false;
        if (FlyingUnit.TryGetComponent<Shooter>(out Shooter S)) S.enabled = false;
        FlyingUnit.GetComponent<Animator>().SetBool("Moving", false);
        //Debug.Log("Юнит взялся");
    }

    private void ReleaseUnit() // отпустить юнита
    {
        if (FlyingUnit == null) return;
        // включение всех компонентов у юнита, чтоб вернуть его на поле
        FlyingUnit.GetComponent<Damageble>().enabled = true;
        FlyingUnit.GetComponent<CapsuleCollider>().enabled = true;
        FlyingUnit.GetComponent<NavMeshAgent>().enabled = true;
        if (FlyingUnit.TryGetComponent<Warrior>(out Warrior W)) W.enabled = true;
        if (FlyingUnit.TryGetComponent<Shooter>(out Shooter S)) S.enabled = true;

        // определение точки приземления юнита
        Vector3 Point = FlyingUnit.transform.position;
        Point.y = 0;
        FlyingUnit.transform.position = Point;

        // сброс
        FlyingUnit = null;
        //Debug.Log("Юнита отпустили");
    }

    void UnitCursor() // метод перемещения юнита за курсором
    {
        if (FlyingUnit != null)
        {
            Ray CameraRay = Camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit Hit;
            if (Physics.Raycast(CameraRay, out Hit) && !EventSystem.current.IsPointerOverGameObject())
            {
                Vector3 Point = Hit.point;
                Point.y = 1;
                FlyingUnit.transform.position = Point;
                FlyingUnit.transform.rotation = Quaternion.Euler(0, 180, 0);

            }
            else FlyingUnit.transform.position = new Vector3(0, 1, 0);
        }
        else
        {
            return;
        }
    }


    public void ActiveSpawnButton() // включение кнопки спавна
    {
        CloseButton.SetActive(true);
        if (UnitButton != null)
        {
            UnitButton.SetActive(false);
            UnitButton = null;
        }
        UnitSpawner US = SelectedBuilding.GetComponent<UnitSpawner>();
        TextMeshProUGUI FT = HintsFoodText.GetComponent<TextMeshProUGUI>();

        UnitButton = UnitButtons[US.ButtonIndex];
        UnitButton.SetActive(true);
        HintsControlText.SetActive(false);
        string[] names = { "пехотинец", "лучник", "рыцарь" };
        FT.text = $"<color=#FFE000>{US.NeedFood}</color> - {names[US.ButtonIndex]}";
        HintsFoodText.SetActive(true);
    }

    public void CloseSpawnButton() // выключение кнопки спавна
    {
        CloseButton.SetActive(false);
        if (UnitButton != null)
        {
            UnitButton.SetActive(false);
            UnitButton = null;
        }
        if (SelectedBuilding != null) SelectedBuilding = null;
        HintsControlText.SetActive(true);
        HintsFoodText.SetActive(false);
    }

    public void SpawnUnit() // спавн юнита из выбранного здания
    {
        SelectedBuilding.GetComponent<UnitSpawner>().SpawnUnit();
    }


    // строительство зданий
    public void InstallingBuildingMode() // метод размещения зданий
    {
        if (FlyingBuilding != null)
        {
            Ray CameraRay = Camera.ScreenPointToRay(Input.mousePosition); // создание невидимого луча, который направлен из камеры на место, в которое смотрит курсор
            RaycastHit Hit;
            if (Physics.Raycast(CameraRay, out Hit) && !EventSystem.current.IsPointerOverGameObject()) // если луч упирается в какое-то место, и это место не перекрывает какой-либо UI элемент, то начинается работа с выбранным зданием
            {
                Building BuildComp = FlyingBuilding.GetComponent<Building>();

                Vector3 Position = Hit.point;
                
                float x = Mathf.RoundToInt(Position.x);
                if (BuildComp.Size.x % 2 == 0) x -= 0.5f;
                // определение положения здания по оси x; если габариты у здания по оси x чётные, то происходит небольшое смещение, чтобы здание грамотно вписалось в строительную сетку
                // аналогичные действия происходят с положением здания по оси y, которая на самом деле ось z
                // в Unity ось y - это вверх-вниз, а ось z - это вперёд-назад

                float y = Mathf.RoundToInt(Position.z);
                if (BuildComp.Size.y % 2 == 0) y -= 0.5f;

                bool ValidPos = true; // флажок, разрешающий разместить здание; по умолчанию он равен true
                if (x < 0 || x > GridSize.x - BuildComp.Size.x / 2 + 1) ValidPos = false;
                if (y < 0 || y > GridSize.y - BuildComp.Size.y / 2 + 1) ValidPos = false;
                // если габариты выбранного здания находятся за пределами сетки, то выбранная позиция неподходящая
                if (ValidPos && PlaceIsTaken(x, y, FlyingBuilding) || Hit.collider.gameObject.layer != 6) ValidPos = false; // если позиция размещения здания уже чем-то занято или она вообще не на земле, то она также неподходящая
                // Слой №6 - слой "Ground" (земля); здания должны ставиться только на землю и никуда больше

                FlyingBuilding.transform.position = new Vector3(x, 0, y); // размещение здания в позицию; пока размещение, не установка

                BuildComp.SetTransparent(ValidPos); // окрас здания в красный или зелёный цвет в зависимости от позиции - подходящая она или нет

                if (Input.GetMouseButtonDown(0))
                {
                    if (ValidPos)
                    {
                        InstallBuilding(x, y, FlyingBuilding); // а вот это уже сама установка здания, если конечно, позиция подходящая
                    }
                    else
                    {
                        //Debug.Log("Здесь здание поставить нельзя");
                        SoundSource.PlayOneShot(ErrorBuilding);
                        // если позиция не подходящая, то проиграется звук ошибки, и здание не установится
                    }
                }
            }
            else FlyingBuilding.transform.position = new Vector3(-100,0,-100);
        }
        else return;
    }

    public void SpawnBuilding(int BuildingIndex) // метод появления ещё не установленного на поле здания выбранного по индексу из списка BuildingPrefab; вызывается по нажатию кнопки
    {
        int BuildingPrice = BuildingPrefab[BuildingIndex].GetComponent<Building>().Price;

        if (FlyingBuilding != null) Destroy(FlyingBuilding); // если уже имеется какое-то выбранное здание, оно уничтожится, и вместо него появится то, которое игрок выбрал только что

        string[] names = { "барак пехотинцев", "барак лучников", "казарма рыцарей", "ферма", "хлев", "башня с лучником" };
        TextMeshProUGUI T = HintsBuildingText.GetComponent<TextMeshProUGUI>();
        T.text = $"<color=#FFE000>{BuildingPrice}</color> - {names[BuildingIndex]}";
        HintsBuildingText.SetActive(true);

        if (Money >= BuildingPrice) // если есть нужное кол-во монет, то здание появится; если же нет - ничего не произойдёт, просто проиграется звук ошибки, и метод завершится
        {
            FlyingBuilding = Instantiate(BuildingPrefab[BuildingIndex]);
            FlyingBuilding.transform.position = new Vector3(0,0,0); // изначально выбранное здание имеет координаты (0, 0, 0); потом оно будет следовать за курсором, пока игрок его не установит
        }
        else
        {
            OpenMB("Недостаточно монет для установки здания.");
            SoundSource.PlayOneShot(ErrorBuilding);
            return;
        }
    }

    bool PlaceIsTaken(float PosX, float PosY, GameObject Build) // метод проверяющий занято ли место для установки здания
    {
        Building BuildComp = Build.GetComponent<Building>();
        // проверка осуществляется с помощью цикла, который проверяет клетки сетки (ячейки массива Grid), на которых размещается здание
        // если хотя бы одна клетка чем-то занята, то метод обрывается и возвращается значение true
        // если все клетки пустые, то сама позиция тоже не занята, и метод возвращает значение false
        for (int x = (int)BuildComp.BeginPosX; x < BuildComp.Size.x / 2 + 1; x++)
        {
            for (int y = (int)BuildComp.BeginPosY; y < BuildComp.Size.y / 2 + 1; y++)
            {
                int GridX = (int)PosX + x;
                int GridY = (int)PosY + y;

                if (GridX < 0 || GridX >= GridSize.x || GridY < 0 || GridY >= GridSize.y) return true;

                if (Grid[GridX, GridY] != null) return true;
            }
        }
        return false;
    }

    void InstallBuilding(float PosX, float PosY, GameObject Build) // метод установки здания на игровое поле
    {
        Building BuildComp = Build.GetComponent<Building>();

        // цикл заполняющий клетки
        for (int x = (int)BuildComp.BeginPosX; x < BuildComp.Size.x / 2 + 1; x++)
        {
            for (int y = (int)BuildComp.BeginPosY; y < BuildComp.Size.y / 2 + 1; y++)
            {
                Grid[(int)PosX + x, (int)PosY + y] = Build;
                //Debug.Log($"ячейка x : {(int)PosX} + {x} = {(int)(PosX + x)}; ячейка y : {(int)PosY} + {y} = {(int)(PosY + y)}"); // проверка того, что при установке здания заполняются нужные клетки; решил оставить и закомментировать
            }
        }
        Build.transform.position = new Vector3(PosX, 0, PosY);

        //Debug.Log($"Здание установлено. x: {(int)PosX}, y: {(int)PosY}");

        FlyingBuilding.GetComponent<NavMeshObstacle>().enabled = true; // включаение компонента NavMeshObstacle; этот компонет создаёт препядствие в навигационной сетке, по которой ходят юниты
        FlyingBuilding.GetComponent<BoxCollider>().enabled = true; // включаение коллайдера, чтобы здание было видно для противников

        BuildComp.SetNormalMaterial(); //окрашивание здания в его обычный цвет
        //SoundSource.PlayOneShot(BuildingIsInstalled);
        PlaySound(BuildComp.BuildingInstalled);

        Money -= BuildComp.Price; // плата за установку здания
        // лишний раз проверять, есть ли нужное кол-во монет, нет смысла, так как это проверяется при появлении здания в методе SpawnBuilding

        InitializeBuilding(); // инициализация здания

        FlyingBuilding = null; // сброс
    }

    public void InitializeBuilding() // метод инициализации здания; проще говоря, если установленное здание - это какая-нибудь ферма или хлев, оно должно начать выполнять свою функцию
    {
        if (FlyingBuilding.TryGetComponent(out Farm farm))
        {
            Debug.Log("Ферма");
            farm.enabled = true;
            farm.OnInstalled();
            return;
        }

        if (FlyingBuilding.TryGetComponent(out Barn barn))
        {
            Debug.Log("Хлев");
            barn.enabled = true;
            barn.OnInstalled();
            return;
        }

        if (FlyingBuilding.TryGetComponent(out UnitSpawner spawner))
        {
            spawner.enabled = true;
            return;
        }

        if (FlyingBuilding.TryGetComponent(out AncherTower Tower))
        {
            Tower.enabled = true;
            Tower.OnInstalled();
            return;
        }
    }


    public void PlaySound(AudioClip clip) // воспроизведение звука; ничего особенного
    {
        SoundSource.PlayOneShot(clip);
    }



    public void OpenMB(string Message)
    {
        MessageBox.GetComponent<TextMeshProUGUI>().text = Message;
        MessageBox.SetActive(true);
        Invoke("CloseMb", 3);
    }

    public void CloseMb()
    {
        MessageBox.SetActive(false);
    }
    
    void UpdateCounters() // обновление игровых счётчиков
    {
        MoneyText.text = $"{Money}";
        if (CurrentFood < MaxFood || CurrentFood == 0) FoodText.text = $"{CurrentFood}/{MaxFood}";
        else FoodText.text = $"<color=red>{CurrentFood}</color>/{MaxFood}";
    }

    void ShowFPS() // отображение кол-ва кадров в секунду
    {
        float fps = 1 / Time.deltaTime;
        FPSText.text = $"FPS: {(int)fps}";
    }

    public void Back() // вернуться в главное меню
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }


    public void Pause() // пауза
    {
        Time.timeScale = 0f;
        GamePanel.SetActive(false);
        PausePanel.SetActive(true);
        Zoom.enabled = false;
    }

    public void Resume() // продолжить
    {
        Time.timeScale = 1f;
        GamePanel.SetActive(true);
        PausePanel.SetActive(false);
        Zoom.enabled = true;
    }

    public void NextLevel() // переход на следующий уровень
    {
        SceneManager.LoadScene(LevelIndex + 1);
    }

    public void RestartLelvel() // перезапуск уровня
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(LevelIndex);
    }

    public void CorrectMusic(float Value) // метод для регулировки громкости музыки
    {
        Audio.SetFloat("Music", Value);
        Debug.Log("Регулировка");
    }

    public void CorrectSound(float Value) // метод для регулировки громкости простых звуков
    {
        Audio.SetFloat("Sound", Value);
        Debug.Log("Регулировка");
    }

    public void FullScreen(bool IsFullScreen) // метод для смены режима экрана
    {
        Screen.fullScreen = IsFullScreen;
        Debug.Log("Переключение");
    }

    public void ShowFPS(bool showFPS) // метод для смены режима экрана
    {
        FPSText.gameObject.SetActive(showFPS);
        FPS = showFPS;
        Debug.Log("Переключение");
    }

    public void InvertCamera(bool Invert) // метод для смены режима экрана
    {
        Zoom.Inverted = Invert;
        Debug.Log("Переключение");
    }

    public void SaveSettings() // сохранение настроек
    {
        PlayerPrefs.SetFloat("MusicVolume", MusicSlider.value);
        PlayerPrefs.SetFloat("SoundVolume", SoundSlider.value);
        PlayerPrefs.SetInt("FullScreen", Convert.ToInt32(FullScreenToogle.isOn));
        PlayerPrefs.SetInt("ShowFPS", Convert.ToInt32(FPS));
        PlayerPrefs.SetInt("CameraInverted", Convert.ToInt32(Zoom.Inverted));
    }

    public void LoadSettings() // загркзка имеющихся настроек
    {
        if (PlayerPrefs.HasKey("MusicVolume")) MusicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        else MusicSlider.value = -25;
        Audio.SetFloat("Music", MusicSlider.value);

        if (PlayerPrefs.HasKey("SoundVolume")) SoundSlider.value = PlayerPrefs.GetFloat("SoundVolume");
        else SoundSlider.value = -16;
        Audio.SetFloat("Sound", SoundSlider.value);

        if (PlayerPrefs.HasKey("FullScreen")) FullScreenToogle.isOn = Convert.ToBoolean(PlayerPrefs.GetInt("FullScreen"));
        else Screen.fullScreen = true;

        if (PlayerPrefs.HasKey("ShowFPS")) FPSToogle.isOn = Convert.ToBoolean(PlayerPrefs.GetInt("ShowFPS"));
        else FPS = false;
        FPSText.gameObject.SetActive(FPS);

        if (PlayerPrefs.HasKey("CameraInverted")) CameraInvertedToogle.isOn = Convert.ToBoolean(PlayerPrefs.GetInt("CameraInverted"));
        else Zoom.Inverted = false;
    }

    private void OnDrawGizmos()
    {
        for (int x = 0; x < GridSize.x; x++)
        {
            for (int y = 0; y < GridSize.y; y++)
            {
                if ((x + y) % 2 == 0) Gizmos.color = GridColor1;
                else Gizmos.color = GridColor2;
                Gizmos.DrawCube(transform.position + new Vector3(x, 0, y), new Vector3(1, 0.1f, 1));
            }
        }
    }
}
