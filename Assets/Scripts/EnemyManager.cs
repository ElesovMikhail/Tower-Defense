using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class EnemyManager : MonoBehaviour // компонент управления волнами противников; по началу он выключен, включается компонентом GameManager
{
    [SerializeField] GameObject[] Enemies; // префабы противников
    [SerializeField, Range(0, 20)] int[] Frequency; // список для настройки частот, с которыми спавнятся противники; кол-во элементов должно совпадать с кол-вом элементов списка Enemies;
    // частота в пределах от 0 до 20; больше, как по мне, слишком много, а отрицательной она быть не может
    private List<List<int>> Waves = new List<List<int>>(); // список волн и активностей в них
    [SerializeField] private List<GameObject> CurrentEnemiesPrefabs = new List<GameObject>(); // префабы (образцы) противников текущей волны
    [SerializeField] private List<GameObject> CurrentEnemies = new List<GameObject>(); // сами противники текущей волны
    // можно было бы наверное реализовать частоту спавна противников не через такие списки, а каким-нибудь другим более простым способом, но я, к сожалению, ничего лучше не придумал

    [SerializeField, Min(0)] int WavesCount; // кол-во волн
    [SerializeField, Min(0)] public int WaveIndex = 0; // индекс текущей волны; сделал его изменяемым в инспекторе, чтобы волны можно было перематать на любую волну


    private Vector3[] SpawnPoints = new Vector3[4]; // точки спавна; определяются в зависимости от размера игрового поля

    [SerializeField] private float SpawnOffset = 5;

    [SerializeField, Min(0)] private float Delay = 3; // время задержки между волнами

    [SerializeField] public bool ArcadeMode = false; // флажок аркадного режима

    [SerializeField] private TextMeshProUGUI WaveText; // счётчик волн (UI)


    private GameManager GM;

    private float X;
    private float Y;

    private void Awake()
    {
        GM = GetComponent<GameManager>();

        X = GM.GridSize.x / 2;
        Y = GM.GridSize.y / 2;
    }

    void CreateWaves(int WI, int WE) // метод создания волн и определения точек спавна
    {
        // определение точек спавна
        SpawnPoints[0] = new Vector3(-SpawnOffset, 0, Y); // точка спавна слева от поля
        SpawnPoints[1] = new Vector3(X, 0, GM.GridSize.y + SpawnOffset); // точка спавна впереди поля
        SpawnPoints[2] = new Vector3(GM.GridSize.x + SpawnOffset, 0, Y); // точка спавна справа от поля
        SpawnPoints[3] = new Vector3(X, 0, -SpawnOffset); // точка спавна сзади поля

        // определение частот спавнов противников для каждой волны по нарастанию
        for (int i = WI; i < WE; i++)
        {
            List<int> list = new List<int>();
            for (int j = 0; j < Enemies.Length; j++)
            {
                float WaveFactor = 1.5f + 0.3f * i;
                int k = Mathf.FloorToInt(Frequency[j] * WaveFactor);
                list.Add(k);
            }
            Waves.Add(list);
        }

        //Debug.Log("Волны созданы");

        WaveText.text = $"Волна: <color=red>{WaveIndex + 1}</color>"; // обновление счётчика
        // в счётчике к индексу прибавляется 1, так как для простого смертного отсчёт начинается с 1, а не с 0
        if (!ArcadeMode) WaveText.text += $"/{WavesCount}"; // если не включен аркадный режим, к счётчику дописывается общее кол-во волн, которое игроку нужно отбить
    }


    private void Update()
    {
        CurrentEnemies = CurrentEnemies.Where(enemy => enemy != null).ToList(); // чистка списка противников текущей волны от пустых полей; только ради этого скрипт надо было включить
    }


    IEnumerator StartWave() // корутина, запускающая волну противников
    {
        if (enabled) // если компонент включен, то стартует запуск волн, иначе корутина прерывается
        {
            if (WaveIndex >= WavesCount) // проверка, стоит ли вообще запускать волну
            {
                GM.Victory();
                enabled = false;
                yield break;
            }

            // заполнение списка префабов противников, которые должны быть в волне
            for (int f = 0; f < Waves[WaveIndex].Count; f++)
            {
                for (int w = 0; w < Waves[WaveIndex][f]; w++)
                {
                    CurrentEnemiesPrefabs.Add(Enemies[f]);
                }
            }

            // выпуск самих противников
            foreach (GameObject enemy in CurrentEnemiesPrefabs)
            {
                SpawnEnemy(enemy);
                yield return new WaitForSeconds(1);
            }
            CurrentEnemiesPrefabs.Clear(); // очищение списка префабов




            yield return new WaitUntil(() => CurrentEnemies.Count == 0); // ожидание, пока все противники текущей волны не исчезнут
            WaveIndex++; // обновление индекса волны

            if (WaveIndex >= WavesCount) // основная проверка, кончились ли волны
            {
                if (ArcadeMode)
                {
                    WavesCount += 10;
                    CreateWaves(WaveIndex, WavesCount);
                }
                else
                {
                    GM.Victory();
                    enabled = false;
                    yield break;
                }
            }


            WaveText.text = $"Волна: <color=red>{WaveIndex + 1}</color>"; // обновление счётчика
            if (!ArcadeMode) WaveText.text += $"/{WavesCount}";
            yield return new WaitForSeconds(Delay); // небольшая задержка перед следующей волной


            StartCoroutine(StartWave()); // запуск следующей волны с помощью рекурсии
        }
        else yield break;
    }



    void SpawnEnemy(GameObject enemy) // метод спавна врага
    {
        int PointIndex = UnityEngine.Random.Range(0, 4); // случайное определение индекса точки спавна

        // определение случайного смещения относительно спавна
        Vector3 Offset;
        float f; // число, которому будет присваиваться та или иная ширина
        if (PointIndex % 2 == 0)
        {
            Offset = new Vector3(0, 0, 1); // если точка спавна слева или справа от игрового поля (индекс точки спавна чётный), то смещение происходит по оси z
            f = Y;
        }
        else
        {
            Offset = new Vector3(1, 0, 0); // если точка спавна спереди или сзади игрового поля (индекс точки спавна нечётный) - смещение происходит по оси x
            f = X;
        }
        float k = UnityEngine.Random.Range(-f, f + 1); // насколько большим будет смещение
        Offset = Offset * k;
        //Debug.Log(Offset);

        Vector3 SpawnPoint = SpawnPoints[PointIndex] + Offset; // определение точки спавна со смещением
        GameObject CEnemy = Instantiate(enemy, SpawnPoint, transform.rotation); // спавн врага
        CurrentEnemies.Add(CEnemy); // добавление этого самого врага в список противников текущей волны
    }

    public void BeginWaves() // метод запуска корутины StartWave; вызывается компонентом GameManager
    {
        StartCoroutine(StartWave());
    }

    public void CreateStartWaves() // метод создания начальных волн; запускается компонентом GameManager в начале игры
    {
        CreateWaves(WaveIndex, WavesCount);
    }

}
