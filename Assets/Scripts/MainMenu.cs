using System;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour // компонент для всего главного меню и его элементов
{

    [SerializeField] private AudioMixer Audio; // аудиомиксер
    private AudioSource SoundSource;
    [SerializeField] private AudioClip PressClip;

    [SerializeField] private Slider MusicSlider;
    [SerializeField] private Slider SoundSlider;
    [SerializeField] private Toggle FullScreenToogle;
    [SerializeField] private Toggle FPSToogle;
    [SerializeField] private Toggle CameraInvertedToogle;

    [SerializeField] private GameObject LoadingPanel; // панель загрузки
    [Space]
    [SerializeField] private TextMeshProUGUI LevelText; // текст, отображающий выбранный уровень
    [SerializeField] private GameObject LevelButton; // кнопка запуска уровня; запускается, когда уровень выбран
    [Space]
    [Space]
    [Space]
    [SerializeField] GameObject LevelCamera; // камера, смотрящая в сторону меток уровней
    [Space]
    [SerializeField] GameObject[] LevelMarks; // метки уровней; метка аркадного уровня последняя в списке
    [Space]
    [Space]
    [HideInInspector] public static bool FPS = false;
    [HideInInspector] public static bool CameraInverted = false;


    private int LevelIndex;

    public static int AvailableLevels;

    [SerializeField] private int FPSCount = 100;

    private void Start()
    {
        Application.targetFrameRate = FPSCount;

        LoadSettings();
        LoadGameData();

        SoundSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && LevelCamera.activeInHierarchy)
        {
            Ray CameraRay = LevelCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit Hit;
            if (Physics.Raycast(CameraRay, out Hit) && !EventSystem.current.IsPointerOverGameObject())
            {
                SoundSource.PlayOneShot(PressClip);
                LevelIndex = Array.IndexOf(LevelMarks, Hit.collider.gameObject) + 1;
                LevelText.text = $"уровень {LevelIndex}";
                if (LevelIndex == LevelMarks.Length)
                {
                    LevelMarks[^1].transform.GetChild(0).gameObject.SetActive(false);
                    LevelText.text = $"аркадный режим";
                    if (PlayerPrefs.HasKey("WavesInArcade") && PlayerPrefs.GetInt("WavesInArcade") > 0) LevelText.text += $"\n<color=#3C1C09>Рекорд:</color> <color=red>{PlayerPrefs.GetInt("WavesInArcade")}</color>";
                }
                else LevelMarks[^1].transform.GetChild(0).gameObject.SetActive(true);
                LevelText.gameObject.SetActive(true);
                LevelButton.SetActive(true);

                Debug.Log(LevelIndex);
            }
        }
    }

    public void StartGame()
    {
        LoadingPanel.SetActive(true);
        SceneManager.LoadScene(LevelIndex);
    }

    public void ExitGame() // выход из преложения
    {
        Debug.Log("Выход");
        Application.Quit();
    }

    public void OpenBrowser(string url) // открытие ссылок
    {
        Debug.Log("Ссылка");
        Application.OpenURL(url);
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
        FPS = showFPS;
        Debug.Log("Переключение");
    }

    public void InvertCamera(bool Invert) // метод для смены режима экрана
    {
        CameraInverted = Invert;
        Debug.Log("Переключение");
    }


    public void SaveSettings() // сохранение настроек
    {
        PlayerPrefs.SetFloat("MusicVolume", MusicSlider.value);
        PlayerPrefs.SetFloat("SoundVolume", SoundSlider.value);
        PlayerPrefs.SetInt("FullScreen", Convert.ToInt32(FullScreenToogle.isOn));
        PlayerPrefs.SetInt("ShowFPS", Convert.ToInt32(FPS));
        PlayerPrefs.SetInt("CameraInverted", Convert.ToInt32(CameraInverted));
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

        if (PlayerPrefs.HasKey("CameraInverted")) CameraInvertedToogle.isOn = Convert.ToBoolean(PlayerPrefs.GetInt("CameraInverted"));
        else CameraInverted = false;
    }

    public void LoadGameData() // загрузка данных, связанных с игрой
    {
        if (PlayerPrefs.HasKey("AvailableLevels")) AvailableLevels = PlayerPrefs.GetInt("AvailableLevels");
        else PlayerPrefs.SetInt("AvailableLevels", 1);
        AvailableLevels = PlayerPrefs.GetInt("AvailableLevels");
        Debug.Log(AvailableLevels);

        for (int i = 0; i < AvailableLevels; i++) // включение меток доступных уровней
        {
            if (i >= LevelMarks.Length) break;
            LevelMarks[i].SetActive(true);
        }
        LevelMarks[^1].SetActive(true); // метка уровня аркадного режима всегда включается
    }

    public void DropAllData() // удаление всех данных, включая настроек
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("Удалено");
    }

    public void DropGameData() // сброс игровых данных
    {
        PlayerPrefs.DeleteKey("AvailableLevels");
        PlayerPrefs.DeleteKey("WavesInArcade");
    }

    public void RestartMenu() // перезапуск сцены
    {
        SceneManager.LoadScene(0);
    }
}
