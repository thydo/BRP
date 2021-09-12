using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

/* Bunny file header
 ply
format ascii 1.0
comment zipper output
element vertex 35947
property float x
property float y
property float z
property float confidence
property float intensity
element face 69451
property list uchar int vertex_index
end_header
 */
[RequireComponent(typeof(MeshFilter))]
public class MeshTopologyRendere : MonoBehaviour
{
    [SerializeField]
    string objectPath;

    [SerializeField]
    int vertexCount;

    [SerializeField]
    int faceCount;

    Mesh mesh;

    List<Vector3> vertices;
    List<int> triangles;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        //CreateShape();
        ParseFile();
        UpdateMesh();
    }

    /*void CreateShape()
    {
        vertices = new Vector3[]
        {
            new Vector3 (0,0,0),
            new Vector3 (0,0,1),
            new Vector3 (1,0,0),
            new Vector3 (1,0,1)
        };

        triangles = new int[]
        {
            0,1,2,
            1,3,2,
        };
    }*/

    void ParseFile()
    {
        TextAsset bunnAsset = Resources.Load<TextAsset>("bun_zipper");
        //Debug.Log(bunnAsset.ToString());
        List<string> plyFile = bunnAsset.text.Split('\n').ToList<string>();//File.ReadLines(objectPath).ToList<string>();
        vertices = new List<Vector3>();
        triangles = new List<int>();

        plyFile.GetRange(0, vertexCount).ForEach(vertex =>
        {
            string[] xyzci = vertex.Split(' ');
            vertices.Add(new Vector3(float.Parse(xyzci[0]), float.Parse(xyzci[1]), float.Parse(xyzci[2])));
        });

        plyFile.GetRange(vertexCount, faceCount).ForEach(face =>
        {
            string[] faces = face.Split(' ');
            //Debug.Log(faces[1]);
            triangles.Add(int.Parse(faces[1]));
            triangles.Add(int.Parse(faces[2]));
            triangles.Add(int.Parse(faces[3]));
        });
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();
        mesh.SetIndices(triangles, MeshTopology.Points, 0);
    }

}
