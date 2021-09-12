using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;


public class ParticleSystemRenderer : MonoBehaviour
{
    List<Vector3> vertices;

    [SerializeField]
    public Color color = Color.white;
    
    [SerializeField]
    public float particleSize = 5;
    
    [SerializeField]
    int vertexCount;

    [SerializeField]
    int scale;

    private void Start()
    {
        ParseFile();
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
            vertices.Add(new Vector3(float.Parse(xyzci[0]), float.Parse(xyzci[1]), float.Parse(xyzci[2]))*scale);
        });

        ApplyToParticleSystem(vertices);
    }

    public void ApplyToParticleSystem(List<Vector3> positions)
    {
        var ps = GetComponent<ParticleSystem>();
        Debug.Log(ps);
        if (ps == null)
            return;

        var particles = new ParticleSystem.Particle[positions.Count];

        for (int i = 0; i < particles.Length; ++i)
        {
            particles[i].position = positions[i];
            particles[i].startSize = particleSize;
            particles[i].startColor = color;
        }
        ps.SetParticles(particles);
        ps.Pause();
    }
}
