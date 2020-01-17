using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConverterMesh : MonoBehaviour
{
    #region nested classes
    #region normal
    public class normal
    {
        public static bool ContainKey(Dictionary<normal, List<vertex>> PolygonTree, Vector3 newNormal)
        {
            foreach (var key in PolygonTree.Keys)
            {
                if (Vector3.Equals(newNormal, key.direction)) return true;
            }
            return false;
        }
        public static normal TakeKey(Dictionary<normal, List<vertex>> PolygonTree, Vector3 normal)
        {
            foreach (var key in PolygonTree.Keys)
            {
                if (Vector3.Equals(normal, key.direction)) return key;
            }
            return null;
        }
        public normal(Vector3 direction,int iPolygonKeys)
        { this.direction = direction; this.iPolygonKeys = iPolygonKeys; }
        public int iPolygonKeys;
        public Vector3 direction;
    }
    #endregion
    #region vertex
    public class vertex
    {
        public vertex(Vector3 position, int indexInRealList)
        { this.position = position; iReal = indexInRealList; }

        public int iReal;
        public List<int> indexes;
        public Vector3 position;
        public static vertex vertexInList(List<vertex> Vertices, Vector3 position)
        {
            foreach (vertex vertex in Vertices)
            {
                if (Vector3.Equals(vertex.position, position)) return vertex;
            }
            return null;
        }
    }
    #endregion
    #endregion
    #region Gets
    public void GetPolygonTree(GameObject obj)
    {
        GetPolygonTree(obj.GetComponent<MeshFilter>().mesh);
    }
    public void GetPolygonTree(Mesh mesh)
    {
        Dictionary<normal, List<vertex>> PolygonTree = new Dictionary<normal, List<vertex>>();
        this.mesh = mesh;
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;

        int i;
        for (i = 0; i < normals.Length; i++)
        {
            if (!normal.ContainKey(PolygonTree, normals[i]))
            {
                PolygonTree.Add(new normal(normals[i], PolygonTree.Keys.Count), new List<vertex>());
            }
        }

        RealVertices = new List<vertex>();
        for (i = 0; i < vertices.Length; i++)
        {
            vertex v = vertex.vertexInList(RealVertices, vertices[i]);
            if (v == null)
            {
                RealVertices.Add(new vertex(vertices[i], RealVertices.Count));
                RealVertices[RealVertices.Count - 1].indexes = new List<int>();
                RealVertices[RealVertices.Count - 1].indexes.Add(i);
            }
            else RealVertices[v.iReal].indexes.Add(i);
        }


        vertex vertice; normal key;
        for (i = 0; i < vertices.Length; i++)
        {
            key = normal.TakeKey(PolygonTree, normals[i]);
            if (key != null)
            {
                vertice = vertex.vertexInList(RealVertices, vertices[i]);
                PolygonTree[key].Add(_realVertices[vertice.iReal]);
            }
        }

        _polygonTree = PolygonTree;
    }
    public normal[] GetKeys()
    {
        int i = 0; normal[] Keys = new normal[PolygonTree.Keys.Count];
        foreach (var key in PolygonTree.Keys)
        {
            Keys[i] = key;
            i++;
        }
        _keys = Keys;
        return Keys;
    }
    #endregion
    #region Overwrite
    public void OverwriteMesh()
    {
        Vector3 var = Vector3.zero;
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        foreach (vertex v in RealVertices)
        {
            foreach (int i in v.indexes)
            {
                vertices[i] = v.position;
            }
        }

        List<int> indxs = new List<int>();
        for (int ik = 0; ik < Keys.Length; ik++)
        {
            foreach (vertex v in PolygonTree[Keys[ik]])
            {
                var += v.position;
                indxs = v.indexes;
            }
            var /= PolygonTree[Keys[ik]].Count;
            Keys[ik].direction = var / Vector3.Magnitude(var);
            foreach (int i in indxs)
            {
                normals[i] = Keys[ik].direction;
            }
        }
        mesh.normals = normals;
        mesh.vertices = vertices;
        if (mCollider != null) mCollider.sharedMesh = mesh;
        else Debug.LogError("В ConverterMesh, поле - Mesh collider = null. Присвойте значение в вызывающем эту функцию методе, прежде чем вызывать этот метод");
    }
    public void OverwritePolygon(normal n, GameObject objWithMesh)
    {
        Mesh mesh = objWithMesh.GetComponent<MeshFilter>().mesh;
        OverwritePolygon(n, mesh, objWithMesh.GetComponent<MeshCollider>());
    }
    public void OverwritePolygon(normal n, Mesh mesh, MeshCollider meshCollider)
    {
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;

        foreach (vertex v in PolygonTree[n])
        {
            foreach (int i in RealVertices[v.iReal].indexes)
            {
                vertices[i] = v.position;
                normals[i] = n.direction;
            }
        }
        mesh.vertices = vertices;
        mesh.normals = normals;

        meshCollider.sharedMesh = mesh;
    }
    #endregion
    #region Data
    public Mesh mesh;
    public MeshCollider mCollider;
    public List<vertex> RealVertices { get => _realVertices; set => _realVertices = value; }
    private List<vertex> _realVertices;
    public Dictionary<normal, List<vertex>> PolygonTree { get => _polygonTree; set => _polygonTree = value; }
    private Dictionary<normal, List<vertex>> _polygonTree;
    public normal[] Keys { get => _keys; set => _keys = value; }
    private normal[] _keys;
    #endregion
    #region угол между векторами
    public static float AngleBtwVectors(Vector3 vA, Vector3 vB, Vector3 Filter)
    {
        Vector3 KeyProect = new Vector3(vA.x * Filter.x, vA.y * Filter.y, vA.z * Filter.z);
        Vector3 NorProect = new Vector3(vB.x * Filter.x, vB.y * Filter.y, vB.z * Filter.z);
        return AngleBtwVectors(KeyProect, NorProect);
    }
    public static float AngleBtwVectors(Vector3 vectA, Vector3 vectB)
    {
        Vector3 N = Vector3Multiply(vectA, vectB);
        float scalMult = (Vector3.Magnitude(vectA) * Vector3.Magnitude(vectB));

        if (Vector3.Distance(vectA, -vectB) < Vector3.Distance(vectA, vectB))
        {
            if (Vector3.Magnitude(N) == 0 || Equals(scalMult, 0)) return 180;
            else return (180 - Mathf.Asin(Vector3.Magnitude(N) / scalMult) * Mathf.Rad2Deg);
        }
        else
        {
            if (Vector3.Magnitude(N) == 0 || Equals(scalMult, 0)) return 0;
            else return Mathf.Asin(Vector3.Magnitude(N) / scalMult) * Mathf.Rad2Deg;
        }
    }
    private static Vector3 Vector3Multiply(Vector3 A, Vector3 B)
    {
        return new Vector3(A.z * B.y - A.y * B.z, A.x * B.z - A.z * B.x, A.y * B.x - A.x * B.y);
    }
    #endregion

    #region NearestKeySearch
    public int NearestKeySearch(Vector3 target)
    {
        return NearestKeySearch(Keys, target);
    }
    public int NearestKeySearch(normal[] Keys, Vector3 target)
    {
        float angle, minAngle = float.MaxValue;
        int minimal = int.MaxValue;
        for (int i = 0; i < Keys.Length; i++)
        {
            angle = AngleBtwVectors(Keys[i].direction, target);
            if (Mathf.Abs(angle) < minAngle) { minAngle = angle; minimal = i; }
        }
        return minimal;
    }
    #endregion
    public void PolygonTransformation(normal key, Vector3 Translation, Vector3 Rotation, Vector3 Scale, bool noOverwrite = true)
    {
        PolygonTransformation(key, Translation, Rotation, Vector3.zero, Scale, noOverwrite);
    }
    public void PolygonTransformation(normal key, Vector3 Translation, Vector3 Rotation, Vector3 RotationCenter, Vector3 Scale, bool Overwrite = true)
    {
        List<vertex> polygon = PolygonTree[key]; Rotation *= Mathf.Deg2Rad;

        float[,] Figure = new float[polygon.Count, 4];

        for (int i = 0; i < polygon.Count; i++)
        {
            Figure[i, 0] = polygon[i].position.x - RotationCenter.x;
            Figure[i, 1] = polygon[i].position.y - RotationCenter.y;
            Figure[i, 2] = polygon[i].position.z - RotationCenter.z;
            Figure[i, 3] = 1;
        }

        if (!Vector3.Equals(Vector3.zero, Rotation))
        {
            Figure = RotateFigureX(Figure, polygon.Count, Rotation.x);
            Figure = RotateFigureY(Figure, polygon.Count, Rotation.y);
            Figure = RotateFigureZ(Figure, polygon.Count, Rotation.z);

            for (int i = 0; i < polygon.Count; i++)
            {
                Figure[i, 0] = polygon[i].position.x + RotationCenter.x;
                Figure[i, 1] = polygon[i].position.y + RotationCenter.y;
                Figure[i, 2] = polygon[i].position.z + RotationCenter.z;
                Figure[i, 3] = 1;
            }
        }
        if (!Vector3.Equals(Vector3.zero, Translation)) Figure = TranslationFigure(Figure, polygon.Count, Translation);
        if (!Vector3.Equals(Vector3.zero, Scale)) Figure = ScaleFigure(Figure, polygon.Count, Scale);


        for (int i = 0; i < polygon.Count; i++)
        {
            polygon[i].position = new Vector3(Figure[i, 0], Figure[i, 1], Figure[i, 2]);
        }

        PolygonTree[key] = polygon;

        foreach (vertex v in polygon)
        {
            RealVertices[v.iReal].position = v.position;
        }

        if (Overwrite) { OverwriteMesh(); }

        /*
        foreach (normal k in PolygonTree.Keys)
        {
            OverwritePolygon(k, mesh, mCollider);
        }
        //*/
    }

    #region 3D-transformation

    #region PerspectiveProection
    /*
    void PerspectiveProection(float[,] Figure, int N, float p, float q, float r)
    {
        float[,] Convet = new float[4, 4];
        float[,] newMatrix, vect_Figure_i;
        vect_Figure_i = new float[1, 4];
        newMatrix = new float[N, 4];
        float x, y, z, znamen;


        for (int i = 0; i < N; i++)
        {
            x = Figure[i, 0] + 3;
            y = Figure[i, 1] + 3;
            z = Figure[i, 2] + 3;
            znamen = p * x + q * y + r * z + 1;
            newMatrix[i, 0] = x / znamen;
            newMatrix[i, 1] = y / znamen;
            newMatrix[i, 2] = z / znamen;
            newMatrix[i, 3] = 1;
        }
    }
    void APerspectiveProection(float[,] Figure, int N, float p, float q, float r)
    {
        float[,] Convet = new float[4, 4];
        Convet[0, 0] = 1; Convet[0, 1] = 0; Convet[0, 2] = 0; Convet[0, 3] = p;
        Convet[1, 0] = 0; Convet[1, 1] = 1; Convet[1, 2] = 0; Convet[1, 3] = q;
        Convet[2, 0] = 0; Convet[2, 1] = 0; Convet[2, 2] = 1; Convet[2, 3] = r;
        Convet[3, 0] = 0; Convet[3, 1] = 0; Convet[3, 2] = 0; Convet[3, 3] = 1;

        float[,] newMatrix, vect_Figure_i;
        vect_Figure_i = new float[1, 4];
        newMatrix = new float[N, 4];
        for (int i = 0; i < N; i++)
        {
            vect_Figure_i[0, 0] = Figure[i, 0];
            vect_Figure_i[0, 1] = Figure[i, 1];
            vect_Figure_i[0, 2] = Figure[i, 2];
            vect_Figure_i[0, 3] = Figure[i, 3];
            vect_Figure_i = MultiplyMatrix(vect_Figure_i, Convet, 4, 1, 4, 4);
            newMatrix[i, 0] = vect_Figure_i[0, 0] * vect_Figure_i[0, 3];
            newMatrix[i, 1] = vect_Figure_i[0, 1] * vect_Figure_i[0, 3];
            newMatrix[i, 2] = vect_Figure_i[0, 2] * vect_Figure_i[0, 3];
            newMatrix[i, 3] = 1;
        }

    }
    */
    #endregion

    #region Translate Rotate & Scale
    #region RotateFigureAce
    public static float[,] RotateFigureX(float[,] Figure, int N, float rotate_deg)
    {
        float sin_value = Mathf.Sin(rotate_deg);
        float cos_value = Mathf.Cos(rotate_deg);
        float[,] Convet = new float[4, 4];
        Convet[0, 0] = 1; Convet[0, 1] = 0; Convet[0, 2] = 0; Convet[0, 3] = 0;
        Convet[1, 0] = 0; Convet[1, 1] = cos_value; Convet[1, 2] = sin_value; Convet[1, 3] = 0;
        Convet[2, 0] = 0; Convet[2, 1] = -sin_value; Convet[2, 2] = cos_value; Convet[2, 3] = 0;
        Convet[3, 0] = 0; Convet[3, 1] = 0; Convet[3, 2] = 0; Convet[3, 3] = 1;

        return UseConvertMatrix4x4(Figure, N, Convet);
    }
    public static float[,] RotateFigureY(float[,] Figure, int N, float rotate_deg)
    {
        float sin_value = Mathf.Sin(rotate_deg);
        float cos_value = Mathf.Cos(rotate_deg);
        float[,] Convet = new float[4, 4];
        Convet[0, 0] = cos_value; Convet[0, 1] = 0; Convet[0, 2] = -sin_value; Convet[0, 3] = 0;
        Convet[1, 0] = 0; Convet[1, 1] = 1; Convet[1, 2] = 0; Convet[1, 3] = 0;
        Convet[2, 0] = sin_value; Convet[2, 1] = 0; Convet[2, 2] = cos_value; Convet[2, 3] = 0;
        Convet[3, 0] = 0; Convet[3, 1] = 0; Convet[3, 2] = 0; Convet[3, 3] = 1;

        return UseConvertMatrix4x4(Figure, N, Convet);
    }
    public static float[,] RotateFigureZ(float[,] Figure, int N, float rotate_deg)
    {
        float sin_value = Mathf.Sin(rotate_deg);
        float cos_value = Mathf.Cos(rotate_deg);
        float[,] Convet = new float[4, 4];
        Convet[0, 0] = cos_value; Convet[0, 1] = sin_value; Convet[0, 2] = 0; Convet[0, 3] = 0;
        Convet[1, 0] = -sin_value; Convet[1, 1] = cos_value; Convet[1, 2] = 0; Convet[1, 3] = 0;
        Convet[2, 0] = 0; Convet[2, 1] = 0; Convet[2, 2] = 1; Convet[2, 3] = 0;
        Convet[3, 0] = 0; Convet[3, 1] = 0; Convet[3, 2] = 0; Convet[3, 3] = 1;

        return UseConvertMatrix4x4(Figure, N, Convet);
    }
    #endregion

    public static float[,] TranslationFigure(float[,] Figure, int N, Vector3 trans)
    {
        double trans_X = trans.x,
                trans_Y = trans.y,
                trans_Z = trans.z;
        float[,] Convet = new float[4, 4];
        Convet[0, 0] = 1; Convet[0, 1] = 0; Convet[0, 2] = 0; Convet[0, 3] = 0;
        Convet[1, 0] = 0; Convet[1, 1] = 1; Convet[1, 2] = 0; Convet[1, 3] = 0;
        Convet[2, 0] = 0; Convet[2, 1] = 0; Convet[2, 2] = 1; Convet[2, 3] = 0;
        Convet[3, 0] = (float)trans_X; Convet[3, 1] = (float)trans_Y; Convet[3, 2] = (float)trans_Z; Convet[3, 3] = 1;

        return UseConvertMatrix4x4(Figure, N, Convet);
    }

    public static float[,] ScaleFigure(float[,] Figure, int N, Vector3 coeff)
    {
        double Koef_X = coeff.x,
                Koef_Y = coeff.y,
                Koef_Z = coeff.z;
        float[,] Convet = new float[4, 4];
        Convet[0, 0] = (float)Koef_X; Convet[0, 1] = 0; Convet[0, 2] = 0; Convet[0, 3] = 0;
        Convet[1, 0] = 0; Convet[1, 1] = (float)Koef_Y; Convet[1, 2] = 0; Convet[1, 3] = 0;
        Convet[2, 0] = 0; Convet[2, 1] = 0; Convet[2, 2] = (float)Koef_Z; Convet[2, 3] = 0;
        Convet[3, 0] = 0; Convet[3, 1] = 0; Convet[3, 2] = 0; Convet[3, 3] = 1;

        return UseConvertMatrix4x4(Figure, N, Convet);
    }
    #endregion

    #region base
    private static float[,] UseConvertMatrix4x4(float[,] Figure, int N, float[,] Convet)
    {
        float[,] vect_Figure_i;
        vect_Figure_i = new float[1, 4];
        for (int i = 0; i < N; i++)
        {
            vect_Figure_i[0, 0] = Figure[i, 0];
            vect_Figure_i[0, 1] = Figure[i, 1];
            vect_Figure_i[0, 2] = Figure[i, 2];
            vect_Figure_i[0, 3] = Figure[i, 3];
            vect_Figure_i = MultiplyMatrix(vect_Figure_i, Convet, 4, 1, 4, 4);
            Figure[i, 0] = vect_Figure_i[0, 0];
            Figure[i, 1] = vect_Figure_i[0, 1];
            Figure[i, 2] = vect_Figure_i[0, 2];
            Figure[i, 3] = vect_Figure_i[0, 3];
        }
        return Figure;
    }
    private static float[,] MultiplyMatrix(float[,] Figure, float[,] Convet, int fig_x, int fig_y, int conv_x, int conv_y)
    {
        if (fig_x != conv_y) { return null; }
        else
        {
            float[,] newMatrix = new float[fig_y, conv_x];
            for (int newMat_y = 0; newMat_y < fig_y; newMat_y++)
            {
                for (int newMat_x = 0; newMat_x < conv_x; newMat_x++)
                {
                    newMatrix[newMat_y, newMat_x] = 0;
                    for (int i = 0; i < fig_x; i++)
                    {
                        newMatrix[newMat_y, newMat_x] += (Figure[newMat_y, i] * Convet[i, newMat_x]);
                    }
                }
            }
            return newMatrix;
        }
    }
    #endregion

    #endregion
}
