using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ComputeShaderRenderer : MonoBehaviour
{
    [System.Serializable]
    public struct MeshlessMesh
    {
        public int vertexCount;
        public int faceCount;
        public float particleSize;
        public float meshScaleFactor;
        public Vector3 meshPosOffset;

        public ComputeBuffer MeshDataBuffer;
        public ComputeBuffer MeshUVDataBuffer;

        [HideInInspector]
        public RenderTexture meshTexture;
        [HideInInspector]
        public List<Vector3> vertices;
        [HideInInspector]
        public List<int> triangles;
        [HideInInspector]
        public List<Vector3> normals;
        [HideInInspector]
        public List<Vector2> uv;
    }

    // -------------------------------------------------------------------------------------------------------------------------------------------
    public ComputeShader cp;
    public int numberOfParticles = 60000;
    public Material mt;

    [Range(0, 1)]
    public float ColorizationStrength;



    [Header("Mesh")]
    public MeshlessMesh Mesh1;


    ComputeBuffer ParticleBuffer;
    ComputeBuffer OldParticleBuffer;

    bool isOld;
    // -------------------------------------------------------------------------------------------------------------------------------------------

    void Start()
    {
        ParseFile(ref Mesh1);
        GetUVs(ref Mesh1);
        PopulateBufferWithMeshPositions(ref Mesh1);
        Mesh1.meshTexture = new RenderTexture(256, 256, 24);
        Mesh1.meshTexture.enableRandomWrite = true;
        Mesh1.meshTexture.Create();

        ParticleBuffer = new ComputeBuffer(numberOfParticles, sizeof(float) * 8);
        OldParticleBuffer = new ComputeBuffer(numberOfParticles, sizeof(float) * 8);

    }

    // Update is called once per frame
    void Update()
    {
        ExecuteComputeShader();
    }


    private void OnRenderObject()
    {
        Rendershapes();
    }



    private void OnDestroy()
    {
        ParticleBuffer.Release();
        OldParticleBuffer.Release();
    }

    // -------------------------------------------------------------------------------------------------------------------------------------------
    void Rendershapes()
    {
        mt.SetPass(0);
        Matrix4x4 ow = this.transform.localToWorldMatrix;
        mt.SetMatrix("My_Object2World", ow);

        if (isOld) mt.SetBuffer("_ParticleDataBuff", OldParticleBuffer);
        else mt.SetBuffer("_ParticleDataBuff", ParticleBuffer);
        mt.SetFloat("_ParticleSize", Mesh1.particleSize);
        Graphics.DrawProceduralNow(MeshTopology.Points, 3 * 4, numberOfParticles);


    }

    void ExecuteComputeShader()
    {
        int kernelHandle = cp.FindKernel("CSMain");
        if (isOld)
        {
            cp.SetBuffer(kernelHandle, "_inParticleBuffer", ParticleBuffer);
            cp.SetBuffer(kernelHandle, "_outParticleBuffer", OldParticleBuffer);
        }
        else
        {
            cp.SetBuffer(kernelHandle, "_inParticleBuffer", OldParticleBuffer);
            cp.SetBuffer(kernelHandle, "_outParticleBuffer", ParticleBuffer);
        }

        //Setting meshOne variables
        cp.SetBuffer(kernelHandle, "_MeshDataOne", Mesh1.MeshDataBuffer);
        cp.SetBuffer(kernelHandle, "_MeshDataUVOne", Mesh1.MeshUVDataBuffer);
        cp.SetInt("_CachePointVertexcoundOne", Mesh1.vertexCount);
        cp.SetTexture(kernelHandle, "_MeshTextureOne", Mesh1.meshTexture);
        cp.SetVector("_transformInfoOne", new Vector4(Mesh1.meshPosOffset.x, Mesh1.meshPosOffset.y,
            Mesh1.meshPosOffset.z, Mesh1.meshScaleFactor));

        cp.SetInt("_NumberOfParticles", numberOfParticles);
        cp.SetVector("CameraPosition", this.transform.InverseTransformPoint(Camera.main.transform.position));
        cp.SetVector("CameraForward", this.transform.InverseTransformDirection(Camera.main.transform.forward));

        cp.Dispatch(kernelHandle, numberOfParticles / 10, 1, 1);
        isOld = !isOld;


    }

    void PopulateBufferWithMeshPositions(ref MeshlessMesh mesh)
    {
        mesh.MeshDataBuffer = new ComputeBuffer(mesh.vertexCount, sizeof(float) * 3);
        mesh.MeshDataBuffer.SetData(mesh.vertices);

        mesh.MeshUVDataBuffer = new ComputeBuffer(mesh.vertexCount, sizeof(float) * 2);
        mesh.MeshUVDataBuffer.SetData(mesh.uv);

    }

    void ParseFile(ref MeshlessMesh mesh)
    {
        TextAsset bunnAsset = Resources.Load<TextAsset>("bun_zipper");
        //Debug.Log(bunnAsset.ToString());
        List<string> plyFile = bunnAsset.text.Split('\n').ToList<string>();//File.ReadLines(objectPath).ToList<string>();
        List<Vector3> vert = new List<Vector3>();
        List<int> tri = new List<int>();

        plyFile.GetRange(0, mesh.vertexCount).ForEach(vertex =>
        {
            string[] xyzci = vertex.Split(' ');
            vert.Add(new Vector3(float.Parse(xyzci[0]), float.Parse(xyzci[1]), float.Parse(xyzci[2])));
        });

        plyFile.GetRange(mesh.vertexCount, mesh.faceCount).ForEach(face =>
        {
            string[] faces = face.Split(' ');
            tri.Add(int.Parse(faces[1]));
            tri.Add(int.Parse(faces[2]));
            tri.Add(int.Parse(faces[3]));
        });
        mesh.vertices = vert;
        mesh.triangles = tri;

    }

    void GetUVs(ref MeshlessMesh mesh)
    {
        mesh.uv = new List<Vector2>();
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            mesh.uv.Add(new Vector2(mesh.vertices[i].x, mesh.vertices[i].y));
        }
    }

}
