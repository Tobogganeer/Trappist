using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class GrassManager : MonoBehaviour
{
    public Mesh mesh;
    public Material material;
    public Terrain terrain;
    private GrassChunk[] chunks;

    // private GraphicsBuffer buffer;
    // Trying to fix weird LOD overlap bug (quarter chunks)
    // Edit: It worked
    private Dictionary<int, GraphicsBuffer> buffers;
    private MaterialPropertyBlock block;
    //private GraphicsBuffer.IndirectDrawIndexedArgs[] commandData;
    private ComputeShader grassHeightShader;
    int kernel;
    const int CommandCount = 1;

    private Camera mainCam;
    private Plane[] planes = new Plane[6];

    [Space, Min(1)] public int chunkSideLength = 10;
    [Header("Highest detail first")]
    public ChunkDensitySettings[] densities;
    [Space] public bool gizmos = true;
    [Header("DO NOT USE")]
    public bool castShadows = false;
    public bool receiveShadows = false;

    void Start()
    {
        grassHeightShader = Resources.Load<ComputeShader>("Shaders/Compute/GrassPosition");
        block = new MaterialPropertyBlock();

        buffers = new Dictionary<int, GraphicsBuffer>(densities.Length);

        for (int i = 0; i < densities.Length; i++)
        {
            int dimension = densities[i].grassDimensions;
            buffers.Add(dimension, InitBuffer((uint)dimension));
        }

        /*
         * This is the info the compute shader needs
        Texture2D<float4> _HeightMap; // Terrain heightmap
        SamplerState sampler_HeightMap;
        RWStructuredBuffer<GrassData> _GrassInstances; // Output
        float3 _TerrainSize; // Terrain LxWxH
        float _SideLength; // Grass blades per side
        float4 _Bounds; // Chunk bounds (minx, minz, maxx, maxz)
        float _GrassScaleX;
        float _GrassScaleY;
        */

        kernel = grassHeightShader.FindKernel("CSMain"); // I think it's 0?

        grassHeightShader.SetTexture(kernel, "_HeightMap", terrain.terrainData.heightmapTexture);
        grassHeightShader.SetVector("_TerrainSize", terrain.terrainData.size);

        Bounds totalBounds = terrain.terrainData.bounds;
        totalBounds.center += terrain.transform.position;

        chunks = new GrassChunk[chunkSideLength * chunkSideLength];

        for (int x = 0; x < chunkSideLength; x++)
        {
            for (int y = 0; y < chunkSideLength; y++)
            {
                Bounds b = GetBounds(new Vector2Int(x, y), totalBounds, chunkSideLength);
                GrassChunk chunk = new GrassChunk(b);
                chunks[x + y * chunkSideLength] = chunk;
            }
        }

        mainCam = Camera.main;

        //InitBuffer();
    }

    void GenerateGrass(GrassChunk chunk, ChunkDensitySettings density)
    {
        if (chunk.density == density.grassDimensions)
        {
            return;
        }

        chunk.density = density.grassDimensions;

        chunk.grassBuffer?.Release();
        chunk.grassBuffer = null;

        if (chunk.density == 0)
        {
            return;
        }

        int dimension = chunk.density;

        int total = dimension * dimension;
        int threadGroups = Mathf.CeilToInt(total / 128f);

        chunk.grassBuffer = new ComputeBuffer(total, GrassInstance.Size);

        grassHeightShader.SetFloat("_SideLength", dimension);

        Bounds b = chunk.bounds;
        Vector4 cBounds = new Vector4(b.min.x, b.min.z, b.max.x, b.max.z);
        grassHeightShader.SetVector("_Bounds", cBounds);
        grassHeightShader.SetBuffer(kernel, "_GrassInstances", chunk.grassBuffer);
        grassHeightShader.SetFloat("_GrassScaleX", density.grassScale.x);
        grassHeightShader.SetFloat("_GrassScaleY", density.grassScale.y);

        grassHeightShader.Dispatch(kernel, threadGroups, 1, 1);
    }

    GraphicsBuffer InitBuffer(uint dimension)
    {
        int stride = GraphicsBuffer.IndirectDrawIndexedArgs.size;
        GraphicsBuffer buffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, CommandCount, stride);
        var commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[CommandCount];
        commandData[0].indexCountPerInstance = mesh.GetIndexCount(0);
        commandData[0].instanceCount = dimension * dimension;
        buffer.SetData(commandData);

        return buffer;
    }

    ChunkDensitySettings GetDensityForDistance(GrassChunk chunk, Vector3 camPosition)
    {
        // Maybe just compare bounds center for extra perf?
        float distance = camPosition.Dist(chunk.bounds.ClosestPoint(camPosition));
        for (int i = 0; i < densities.Length; i++)
        {
            if (distance < densities[i].maxDistance)
                return densities[i];
        }

        return new ChunkDensitySettings();
    }

    private void OnDestroy()
    {
        //buffer?.Release();
        //buffer = null;

        foreach (GraphicsBuffer buffer in buffers.Values)
        {
            buffer?.Release();
        }

        buffers.Clear();
        buffers = null;

        if (chunks != null)
        {
            for (int i = 0; i < chunks.Length; i++)
            {
                chunks[i]?.grassBuffer?.Dispose();
            }
        }
    }

    void Update()
    {
        RenderParams rParams = new RenderParams(material);
        rParams.matProps = block;
        if (castShadows)
            rParams.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        rParams.receiveShadows = receiveShadows;

        if (mainCam == null)
        {
            mainCam = FindObjectOfType<Camera>();
            return;
        }

        GeometryUtility.CalculateFrustumPlanes(mainCam, planes);

        foreach (GrassChunk chunk in chunks)
        {
            if (!GeometryUtility.TestPlanesAABB(planes, chunk.bounds))
            {
                // Free memory of chunks not in view
                GenerateGrass(chunk, default);
                continue;
            }

            // Update LODs
            Vector3 cameraPos = mainCam.transform.position;
            GenerateGrass(chunk, GetDensityForDistance(chunk, cameraPos));

            if (chunk.density == 0)
                continue;

            //commandData[0].instanceCount = (uint)chunk.grassBuffer.count;
            // mmm smth not working quite right here
            // Sometimes grass chunks are usuing the instance counts from lower LODs
            // Results in weird half or quarter chunks
            //commandData[0].instanceCount = (uint)chunk.density * (uint)chunk.density;
            //buffer.SetData(commandData);
            rParams.worldBounds = chunk.bounds;
            block.SetBuffer("_Properties", chunk.grassBuffer);

            Graphics.RenderMeshIndirect(rParams, mesh, buffers[chunk.density], CommandCount);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (terrain != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(terrain.terrainData.bounds.center, terrain.terrainData.bounds.size);
        }

        if (!gizmos) return;

        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;

            foreach (GrassChunk chunk in chunks)
            {
                Gizmos.DrawWireCube(chunk.bounds.center, chunk.bounds.size);
            }
        }
        else
        {
            Gizmos.color = new Color(0.7f, 0.1f, 0.1f, 1.0f);

            Bounds totalBounds = terrain.terrainData.bounds;
            totalBounds.center += terrain.transform.position;

            for (int x = 0; x < chunkSideLength; x++)
            {
                for (int y = 0; y < chunkSideLength; y++)
                {
                    Bounds b = GetBounds(new Vector2Int(x, y), totalBounds, chunkSideLength);
                    Gizmos.DrawWireCube(b.center, b.size);
                }
            }
        }
    }

    private static Bounds GetBounds(Vector2Int coord, Bounds total, int splits)
    {
        // I'm commenting this code bc its silly
        // Size of one individual chunk
        float localSize = total.size.x / splits;
        // Position + size * chunkIndexThing
        Vector3 worldPosition = total.min + new Vector3(coord.x * localSize, 0, coord.y * localSize);
        // Min + size / 2 (extents)
        Vector3 center = worldPosition + Vector3.one * localSize * 0.5f;
        center.y = total.center.y; // We don't touch that
        return new Bounds(center, new Vector3(localSize, total.size.y, localSize));
    }

    class GrassChunk
    {
        public Bounds bounds;
        public int density;

        public ComputeBuffer grassBuffer;

        public GrassChunk(Bounds bounds)
        {
            this.bounds = bounds;
            density = 0;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct GrassInstance
    {
        //public Vector4 positionWS;
        public Matrix4x4 mat;
        //public Matrix4x4 matInverse;

        public const int Size = sizeof(float) * 4 * 4;// * 2;
        //public const int Size = sizeof(float) * 4;

        public GrassInstance(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            //positionWS = position;
            mat = Matrix4x4.TRS(position, rotation, scale);
            //matInverse = mat.inverse;
        }
    }

    /*
    [StructLayout(LayoutKind.Sequential)]
    struct ComputeGrassData
    {
        public Vector4 position;
        public float rotation;
        public Vector3 scale;

        public const int Size = sizeof(float) * 8;
    }
    */

    [System.Serializable]
    public struct ChunkDensitySettings
    {
        public float maxDistance;
        public int grassDimensions;
        public Vector2 grassScale;
    }
}
