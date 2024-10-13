using Unity.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

public class Chunk : MonoBehaviour
{
    // Flat Array[x + y*width + z*width*depth] = blocks[x,y,z]
    // x = i % width, y = (i / width) % height, z = i / (width*height)


    public Material atlas;
    public int width =2, height = 2, depth = 2;
    public Block [,,]blocks;
    public MeshUtils.Blocktype[] chunkData;
    public MeshRenderer meshRenderer;

    // [Header("Perlin Settings")]
    // public float scale = 0.001f;
    // public float heightScale = 10f, heightOffset = 0f;
    // public int octaves = 8;

    public Vector3 location;

    //////////////////////////////////////////////////////////



    //Perlin noise can be calculated in parallel -> for optimisation -> doing using jobs
    //https://docs.unity3d.com/Manual/JobSystem.html :lol autocomplete wrote this comment

    CalculateBlockTypes calculateBlockTypes;
    JobHandle jobHandleBlockType;
    public NativeArray<Unity.Mathematics.Random> RandomArray {get; private set;}

    struct CalculateBlockTypes:IJobParallelFor{
        public NativeArray<MeshUtils.Blocktype> cData;
        public int width;
        public int height;
        public Vector3 location;
        public NativeArray<Unity.Mathematics.Random> randoms;

        public void Execute(int i){
            int x = i % width + (int)location.x, y = (i / width) % height + (int)location.y, z = i / (width*height) + (int)location.z; 
            var random = randoms[i];


            int surfaceHeight = (int)MeshUtils.fBM(x,z,World.surfaceSettings.scale,World.surfaceSettings.heightScale,
                                                    World.surfaceSettings.octaves,World.surfaceSettings.heightOffset);

            int stoneHeight = (int)MeshUtils.fBM(x,z,World.stoneSettings.scale,World.stoneSettings.heightScale,
                                                    World.stoneSettings.octaves,World.stoneSettings.heightOffset);

            int DiamondTHeight = (int)MeshUtils.fBM(x,z,World.diamondTSettings.scale,World.diamondTSettings.heightScale,
                                                    World.diamondTSettings.octaves,World.diamondTSettings.heightOffset);
            int DiamondBHeight = (int)MeshUtils.fBM(x,z,World.diamondBSettings.scale,World.diamondBSettings.heightScale,
                                                    World.diamondBSettings.octaves,World.diamondBSettings.heightOffset);
            int digCave = (int)MeshUtils.fBM3d(x,y,z,World.caveSettings.scale,World.caveSettings.heightScale,
                                                    World.caveSettings.octaves,World.caveSettings.heightOffset);
            int BedrockHeight = (int)MeshUtils.fBM(x,z,World.bedrockSettings.scale,World.bedrockSettings.heightScale,
                                                    World.bedrockSettings.octaves,World.bedrockSettings.heightOffset);
            


            if(y<=BedrockHeight || y==0){cData[i]=MeshUtils.Blocktype.BEDROCK;return;}
            if (digCave<World.caveSettings.probability) {cData[i] = MeshUtils.Blocktype.AIR;return;}

            if(surfaceHeight<y)cData[i] = MeshUtils.Blocktype.AIR;
            else if(y==surfaceHeight)cData[i] = MeshUtils.Blocktype.GRASSIDE;
            else if(y<stoneHeight)cData[i] = MeshUtils.Blocktype.STONE;
            else  cData[i] = MeshUtils.Blocktype.DIRT;

            if(y<DiamondTHeight && y>DiamondBHeight && random.NextFloat(1)<World.diamondTSettings.probability)
            cData[i] = MeshUtils.Blocktype.DIAMOND;
        }
    }




    public void BuildChunkData(){
        int totalBlockCount = width*height*depth;
        chunkData = new MeshUtils.Blocktype[totalBlockCount];
        NativeArray<MeshUtils.Blocktype> blocktypes = new(chunkData, Allocator.Persistent);

        var randomArray = new Unity.Mathematics.Random[totalBlockCount];
        var seed = new System.Random();
        for(int i=0; i<totalBlockCount;++i){
            randomArray[i] = new Unity.Mathematics.Random((uint)seed.Next());
        }
        RandomArray = new NativeArray<Unity.Mathematics.Random>(randomArray, Allocator.Persistent);

        calculateBlockTypes = new CalculateBlockTypes(){
            cData=blocktypes, width=width, height=height, location=location, randoms = RandomArray
        };
        jobHandleBlockType = calculateBlockTypes.Schedule(chunkData.Length, 64);
        jobHandleBlockType.Complete();
        calculateBlockTypes.cData.CopyTo(chunkData);
        blocktypes.Dispose();  //remember garbage collecting
        RandomArray.Dispose();
        

    }




    void Start(){

    }

   
    public void CreateChunk(Vector3 dimension, Vector3 position, bool buildchunkdataenabled = true)
    {

        location = position;
        width = (int)dimension.x;
        height = (int)dimension.y;
        depth = (int)dimension.z;

        Mesh[] varmeshes = new Mesh[width*height*depth];
        MeshFilter mf = this.gameObject.AddComponent<MeshFilter>();
        MeshRenderer mr = this.gameObject.AddComponent<MeshRenderer>();
        meshRenderer = mr;
        mr.material = atlas;
        blocks = new Block[width,height,depth];

        if (buildchunkdataenabled)BuildChunkData();

        var inputMeshes = new List<Mesh>();
        int vertexStart =0, triStart=0, meshCount = width*height*depth, cnt=0;
        var jobs = new ProcessMeshDataJob();
        jobs.vertexStart = new NativeArray<int>(meshCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        jobs.triStart = new NativeArray<int>(meshCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);


        for (int z=0; z<depth; z++) {
            for (int y=0; y<height; y++) {
                for (int x=0; x<width; x++) {


                    blocks[x,y,z] = new Block(new Vector3(x,y,z) + location, chunkData[x + width *(y+height*z) ],this);
                    
                    if(blocks[x,y,z].mesh==null)continue;   
                    inputMeshes.Add(blocks[x,y,z].mesh);
                    var vcount = blocks[x,y,z].mesh.vertexCount;
                    var icount = (int)blocks[x,y,z].mesh.GetIndexCount(0); // 0th submesh -> main mesh
                    jobs.vertexStart[cnt] = vertexStart;
                    jobs.triStart[cnt] = triStart;

                    vertexStart += vcount;
                    triStart += icount;
                    cnt++;


                }
            }
        }

        jobs.meshDataA = Mesh.AcquireReadOnlyMeshData(inputMeshes);
        var outputMeshData = Mesh.AllocateWritableMeshData(1);
        jobs.outputMesh = outputMeshData[0]; 
        jobs.outputMesh.SetIndexBufferParams(triStart,IndexFormat.UInt32);
        jobs.outputMesh.SetVertexBufferParams(vertexStart,
            new VertexAttributeDescriptor(VertexAttribute.Position),
            new VertexAttributeDescriptor(VertexAttribute.Normal, stream: 1),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, stream:2));

        var handle = jobs.Schedule(inputMeshes.Count, 4);
        var newMesh = new Mesh();
        newMesh.name = "Chunk_"+location.x+"_"+location.y+"_"+location.z;
        var sm = new SubMeshDescriptor(0,triStart,MeshTopology.Triangles );
        sm.firstVertex = 0;
        sm.vertexCount = vertexStart;
        handle.Complete();

        jobs.outputMesh.subMeshCount = 1;
        jobs.outputMesh.SetSubMesh(0,sm);

        Mesh.ApplyAndDisposeWritableMeshData(outputMeshData, new[] {newMesh});
        jobs.meshDataA.Dispose();
        jobs.vertexStart.Dispose();
        jobs.triStart.Dispose   ();
        newMesh.RecalculateBounds();
        mf.mesh= newMesh;

        MeshCollider mco = gameObject.AddComponent<MeshCollider>();
        mco.sharedMesh = mf.mesh;

        

    }



    [BurstCompile]
    struct ProcessMeshDataJob : IJobParallelFor
    {
        [ReadOnly] public Mesh.MeshDataArray meshDataA;
        public Mesh.MeshData outputMesh;

        public NativeArray<int> vertexStart;
        public NativeArray<int> triStart;

        public void Execute(int index){
            var data  =  meshDataA[index];
            var vCount = data.vertexCount;
            var vStart = vertexStart[index];

            var verts = new NativeArray<float3>(vCount, Allocator.Temp,NativeArrayOptions.UninitializedMemory);
            data.GetVertices(verts.Reinterpret<Vector3>());
            var normals =new  NativeArray<float3>(vCount, Allocator.Temp,NativeArrayOptions.UninitializedMemory);
            data.GetNormals(normals.Reinterpret<Vector3>());
            var uvs = new NativeArray<float3>(vCount, Allocator.Temp,NativeArrayOptions.UninitializedMemory);
            data.GetUVs(0, uvs.Reinterpret<Vector3>());

            var outputVerts =outputMesh.GetVertexData<Vector3>();
            var outputNormals =outputMesh.GetVertexData<Vector3>(stream:1);
            var outputUvs =outputMesh.GetVertexData<Vector3>(stream:2);

            for (int i = 0; i < vCount; i++)
            {
                outputVerts[vStart+i] = verts[i];
                outputNormals[vStart+i] = normals[i];
                outputUvs[vStart+i] = uvs[i];
            }

            //gotta do some memory cleanup
            verts.Dispose(); normals.Dispose(); uvs.Dispose();

            //triangles are the worst, they needs differne things
            var tStart = triStart[index];
            var tCount = data.GetSubMesh(0).indexCount;
            var outputTris = outputMesh.GetIndexData<int>();
            if(data.indexFormat == IndexFormat.UInt16){
                var tris = data.GetIndexData<ushort>();
                for(int i=0; i<tCount; i++){
                    int idx = tris[i];
                    outputTris[tStart+i] = vStart +idx;
                }
            }
            else{
                var tris = data.GetIndexData<int>();
                for(int i=0; i<tCount; i++){
                    int idx = tris[i];
                    outputTris[tStart+i] = vStart +idx;
                }

            }

        }
    }


    void Update()
    {
        
    }
}
