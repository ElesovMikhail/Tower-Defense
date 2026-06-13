using UnityEngine;

[ExecuteAlways]
public class TreeDrawer : MonoBehaviour // компонент дл€ отрисовки моделек деревьев
{
    [SerializeField] Mesh Mesh; // мэш дерева
    [SerializeField] Material Material; // материал дерева

    private Quaternion Rot = Quaternion.identity;
    private Vector3 Scale = Vector3.one;

    GameManager GM;

    float X;
    float Y;

    private void Awake()
    {
        GM = GetComponent<GameManager>();

        X = GM.GridSize.x;
        Y = GM.GridSize.y;
    }
    private void Update()
    {
        // отрисовка деревьев слева от пол€
        for (float i = -1; i <= Y + 1; i += 2)
        {
            Vector3 Pos = new Vector3(-2, 0, i);
            Matrix4x4 Matrix = Matrix4x4.TRS(Pos, Rot, Scale);

            Graphics.DrawMesh(Mesh, Matrix, Material, 0);
        }

        // отрисовка деревьев спереди пол€
        for (float i = -2; i <= X + 1; i += 2)
        {
            Vector3 Pos = new Vector3(i, 0, Y + 1);
            Matrix4x4 Matrix = Matrix4x4.TRS(Pos, Rot, Scale);

            Graphics.DrawMesh(Mesh, Matrix, Material, 0);
        }

        // отрисовка деревьев справа от пол€
        for (float i = -1; i <= Y + 1; i += 2)
        {
            Vector3 Pos = new Vector3(X + 1, 0, i);
            Matrix4x4 Matrix = Matrix4x4.TRS(Pos, Rot, Scale);

            Graphics.DrawMesh(Mesh, Matrix, Material, 0);
        }

        // отрисовка деревьев сзади пол€
        for (float i = -2; i <= X + 1; i += 2)
        {
            Vector3 Pos = new Vector3(i, 0, -2);
            Matrix4x4 Matrix = Matrix4x4.TRS(Pos, Rot, Scale);

            Graphics.DrawMesh(Mesh, Matrix, Material, 0);
        }
    }
}
