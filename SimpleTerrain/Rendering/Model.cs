namespace SimpleTerrain.Rendering;

using Silk.NET.Assimp;
using Silk.NET.OpenGL;
using System.Numerics;
using Core;
using AssimpMesh = Silk.NET.Assimp.Mesh;

public class Model : IDisposable
{
    private readonly GL _gl;
    private readonly Assimp _assimp;
    private List<GLTexture> _texturesLoaded = new();

    public string Directory { get; private set; } = string.Empty;
    public List<Mesh> Meshes { get; private set; } = new();

    public Model(GL gl, string path)
    {
        _gl     = gl;
        _assimp = Assimp.GetApi();
        LoadModel(path);
    }

    private unsafe void LoadModel(string path)
    {
        var scene = _assimp.ImportFile(path, (uint)(
            PostProcessSteps.Triangulate |
            PostProcessSteps.GenerateNormals |
            PostProcessSteps.CalculateTangentSpace |
            PostProcessSteps.FlipUVs
        ));

        if (scene == null
            || scene->MFlags == Assimp.SceneFlagsIncomplete
            || scene->MRootNode == null)
        {
            throw new Exception(_assimp.GetErrorStringS());
        }

        Directory = Path.GetDirectoryName(path) ?? string.Empty;
        ProcessNode(scene->MRootNode, scene);
    }

    private unsafe void ProcessNode(Node* node, Scene* scene)
    {
        for (var i = 0; i < node->MNumMeshes; i++)
        {
            var mesh = scene->MMeshes[node->MMeshes[i]];
            Meshes.Add(ProcessMesh(mesh, scene));
        }

        for (var i = 0; i < node->MNumChildren; i++)
        {
            ProcessNode(node->MChildren[i], scene);
        }
    }

    private unsafe Mesh ProcessMesh(AssimpMesh* mesh, Scene* scene)
    {
        var vertices = new List<Vertex>();
        var indices  = new List<uint>();

        // vertices
        for (uint i = 0; i < mesh->MNumVertices; i++)
        {
            var vertex = new Vertex
            {
                Position  = mesh->MVertices[i],
                Normal    = mesh->MNormals    != null ? mesh->MNormals[i]    : Vector3.Zero,
                Tangent   = mesh->MTangents   != null ? mesh->MTangents[i]   : Vector3.Zero,
                Bitangent = mesh->MBitangents != null ? mesh->MBitangents[i] : Vector3.Zero,
                TexCoords = mesh->MTextureCoords[0] != null
                    ? new Vector2(mesh->MTextureCoords[0][i].X, mesh->MTextureCoords[0][i].Y)
                    : Vector2.Zero
            };

            vertices.Add(vertex);
        }

        // indices
        for (uint i = 0; i < mesh->MNumFaces; i++)
        {
            var face = mesh->MFaces[i];
            for (uint j = 0; j < face.MNumIndices; j++)
                indices.Add(face.MIndices[j]);
        }

        return new Mesh(_gl, BuildVertices(vertices), BuildIndices(indices));
    }

    private float[] BuildVertices(List<Vertex> vertexCollection)
    {
        var vertices = new List<float>();

        foreach (var v in vertexCollection)
        {
            // position
            vertices.Add(v.Position.X);
            vertices.Add(v.Position.Y);
            vertices.Add(v.Position.Z);
            // normal
            vertices.Add(v.Normal.X);
            vertices.Add(v.Normal.Y);
            vertices.Add(v.Normal.Z);
            // uv
            vertices.Add(v.TexCoords.X);
            vertices.Add(v.TexCoords.Y);
        }

        return vertices.ToArray();
    }

    private uint[] BuildIndices(List<uint> indices) => indices.ToArray();

    public void Dispose()
    {
        foreach (var mesh in Meshes)
            mesh.Dispose();

        _texturesLoaded.Clear();
        _assimp.Dispose();
    }
}