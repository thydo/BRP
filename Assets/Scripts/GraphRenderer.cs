using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GraphRenderer : MonoBehaviour
{
    List<Vector3> vertices;

    [SerializeField]
    public float particleSize = 5;

    [SerializeField]
    int vertexCount;

    [SerializeField]
    int scale;

    [SerializeField]
    Transform pointPrefab;

    private void Awake()
    {
        ParseFile();
        for (int i = 0; i < vertexCount; i++)
        {
            Transform point = Instantiate(pointPrefab);
            point.localPosition = vertices[i];
            point.localScale = Vector3.one * particleSize;
            point.SetParent(transform, false);
        }
    }

    void ParseFile()
    {
        TextAsset bunnAsset = Resources.Load<TextAsset>("bun_zipper");
        //Debug.Log(bunnAsset.ToString());
        List<string> plyFile = bunnAsset.text.Split('\n').ToList<string>();//File.ReadLines(objectPath).ToList<string>();
        vertices = new List<Vector3>();
        plyFile.GetRange(0, vertexCount).ForEach(vertex =>
        {
            string[] xyzci = vertex.Split(' ');
            vertices.Add(new Vector3(float.Parse(xyzci[0]), float.Parse(xyzci[1]), float.Parse(xyzci[2])) * scale);
        });
    }
}
