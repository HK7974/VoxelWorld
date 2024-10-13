using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/*
    All the Vector3 have been converted to Vector3Int

*/
public struct PerlinSettings{
    public float heightScale;
    public float scale;
    public int octaves;
    public float heightOffset;
    public float probability;

    public PerlinSettings(float hs, float s, int o, float ho, float p){
        heightOffset = ho;
        heightScale = hs;
        scale = s;
        octaves = o;
        probability = p;
    }


}


public class World : MonoBehaviour
{
    /// <summary>
    /// declared variables and objects
    /// </summary>

    [SerializeField]
    public static Vector3Int worldDimensions = new Vector3Int(5,5,5);
    public static Vector3Int extraWorldDimensions = new Vector3Int(5,5,5);
    [SerializeField]
    public static Vector3Int chunkDimensions = new Vector3Int(10,15,10);
    public GameObject chunkPrefab;
    public GameObject mCamera;
    public GameObject fpc;
    public Slider progSlider;
    public static PerlinSettings surfaceSettings;
    public PerlinGrapher surface;
    public static PerlinSettings stoneSettings;
    public PerlinGrapher stone;
    public static PerlinSettings diamondTSettings;
    public PerlinGrapher diamondT;
    public static PerlinSettings diamondBSettings;
    public PerlinGrapher diamondB;
    public static PerlinSettings caveSettings;
    public PerlinGrapher3D cave;
    public static PerlinSettings bedrockSettings;
    public PerlinGrapher bedrock;

    public int drawRadius=5;

    
    /// /////////
    
    MeshUtils.Blocktype onClickBuildType;
    public void SetBuildType(int type){
        onClickBuildType = (MeshUtils.Blocktype)type;
    }

    
    /// //////////
    
    
    
        // world generation on movement
    HashSet<Vector3Int> chunkChecker = new HashSet<Vector3Int>();
    HashSet<Vector2Int> chunkColumns= new HashSet<Vector2Int>();
    Dictionary<Vector3Int, Chunk> chunksDict = new Dictionary<Vector3Int, Chunk>();
    Vector3 lastBuildPos;

    Queue<IEnumerator> buildQueue = new Queue<IEnumerator>();

    IEnumerator BuildCoordinator()
    {
        while (true)
        {
            while (buildQueue.Count > 0)
                yield return StartCoroutine(buildQueue.Dequeue());
            yield return null;
        }
    }


    //////////////////////////////////////////////
   
    void Start(){
        progSlider.maxValue = worldDimensions.x*worldDimensions.z;
        surfaceSettings = new PerlinSettings(surface.heightScale,surface.scale,surface.octaves,surface.heightOffset,surface.probability);
        stoneSettings = new PerlinSettings(stone.heightScale,stone.scale,stone.octaves,stone.heightOffset,stone.probability);
        diamondTSettings = new PerlinSettings(diamondT.heightScale,diamondT.scale,diamondT.octaves,diamondT.heightOffset,diamondT.probability);
        diamondBSettings = new PerlinSettings(diamondB.heightScale,diamondB.scale,diamondB.octaves,diamondB.heightOffset,diamondB.probability);
        caveSettings = new PerlinSettings(cave.heightScale,cave.scale,cave.octaves,cave.heightOffset,cave.drawCutOff);
        bedrockSettings = new PerlinSettings(bedrock.heightScale,bedrock.scale,bedrock.octaves,bedrock.heightOffset,bedrock.probability);
        StartCoroutine(BuildWorld());



    }

    

    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) 
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit,10)){
                Vector3 hitBlock = Vector3.zero;
                if(Input.GetMouseButtonDown(0)){
                    hitBlock = hit.point-hit.normal/2.0f;   

                }
                else{
                    hitBlock = hit.point+hit.normal/2.0f;   

                }
                // Debug.Log(hitBlock);
                Chunk thischunk = hit.collider.gameObject.GetComponent<Chunk>();
                int bx = (int)(Mathf.Round(hitBlock.x) - thischunk.location.x);
                int by = (int)(Mathf.Round(hitBlock.y) - thischunk.location.y);
                int bz = (int)(Mathf.Round(hitBlock.z) - thischunk.location.z);

                //If we cross chunks

                Vector3Int neighbor;
                if(bx== chunkDimensions.x){
                    neighbor = new Vector3Int((int)thischunk.location.x+chunkDimensions.x,(int)thischunk.location.y,(int)thischunk.location.z);
                    thischunk = chunksDict[neighbor];
                    bx = 0;
                }
                else if (bx==-1){
                    neighbor = new Vector3Int((int)thischunk.location.x-chunkDimensions.x,(int)thischunk.location.y,(int)thischunk.location.z);
                    thischunk = chunksDict[neighbor];
                    bx = chunkDimensions.x-1;
                } //for y ////////////
                if(by== chunkDimensions.y){
                    neighbor = new Vector3Int((int)thischunk.location.x,(int)thischunk.location.y+chunkDimensions.y,(int)thischunk.location.z);
                    thischunk = chunksDict[neighbor];
                    by = 0;
                }
                else if (by==-1){
                    neighbor = new Vector3Int((int)thischunk.location.x,(int)thischunk.location.y-chunkDimensions.y,(int)thischunk.location.z);
                    thischunk = chunksDict[neighbor];
                    by = chunkDimensions.y-1;
                } // for z ////////////
                if(bz== chunkDimensions.z){
                    neighbor = new Vector3Int((int)thischunk.location.x,(int)thischunk.location.y,(int)thischunk.location.z+chunkDimensions.z);
                    thischunk = chunksDict[neighbor];
                    bz = 0;
                }
                else if (bz==-1){
                    neighbor = new Vector3Int((int)thischunk.location.x,(int)thischunk.location.y,(int)thischunk.location.z-chunkDimensions.z);
                    thischunk = chunksDict[neighbor];
                    bz = chunkDimensions.z-1;
                }



                int i = bx + chunkDimensions.x*(by+ chunkDimensions.y*bz);
                // Debug.Log(thischunk.location);
                // Debug.Log(thischunk.chunkData[i]);
                if(Input.GetMouseButtonDown(0)){
                    thischunk.chunkData[i] = MeshUtils.Blocktype.AIR;
                }
                else{
                    thischunk.chunkData[i] = onClickBuildType;
                }
                
                
                // Debug.Log(thischunk.chunkData[i]);
                // Debug.Log(thischunk.location + new Vector3(bx,by,bz));
                DestroyImmediate(thischunk.GetComponent<Collider>());
                DestroyImmediate(thischunk.GetComponent<MeshFilter>());
                DestroyImmediate(thischunk.GetComponent<MeshRenderer>());
                
                thischunk.CreateChunk(chunkDimensions, thischunk.location, false);
            }
        }
    }


    // GetComponent<>() is very time consuming apparently, so reduce the number of times it is used.
    void BuildChunkColumn(int x, int z, bool meshenabled = true) {
        for (int y=0; y<worldDimensions.y; y++) {
           Vector3Int position = new Vector3Int(x, y * chunkDimensions.y, z);
            if (!chunkChecker.Contains(position))
            {
                GameObject chunk = Instantiate(chunkPrefab);
                chunk.name = "Chunk_" + position.x + "_" + position.y + "_" + position.z;
                Chunk c = chunk.GetComponent<Chunk>();
                c.CreateChunk(chunkDimensions, position);
                chunkChecker.Add(position);
                chunksDict.Add(position, c);
            }
            
                chunksDict[position].meshRenderer.enabled = meshenabled;
            
        }
        chunkColumns.Add(new Vector2Int(x,z));
    }


    IEnumerator BuildWorld()
    {
        for (int z=0; z<worldDimensions.z; z++) {
            
            for (int x=0; x<worldDimensions.x; x++) {
                BuildChunkColumn(x*chunkDimensions.x,z*chunkDimensions.z);

                progSlider.value +=1;

                yield return null;
            }
        }
        
        mCamera.SetActive(false);

        int xpos = worldDimensions.x*chunkDimensions.x/2;
        int zpos = worldDimensions.z*chunkDimensions.z/2;
        int ypos = (int)MeshUtils.fBM(xpos,zpos,surface.scale,surface.heightScale,surface.octaves,surface.heightOffset) +1;   
        
        fpc.transform.position = new Vector3Int(xpos,ypos,zpos);
        progSlider.gameObject.SetActive(false);
        fpc.SetActive(true);
        lastBuildPos = Vector3Int.CeilToInt(fpc.transform.position);
        StartCoroutine(BuildCoordinator());
        StartCoroutine(UpdateWorld());
        StartCoroutine(BuildExtraWorld());
    }

IEnumerator BuildExtraWorld()
    {
        int zEnd = worldDimensions.z +extraWorldDimensions.z;
        int zStart = worldDimensions.z;
        int xEnd = worldDimensions.x +extraWorldDimensions.x;
        int xStart = worldDimensions.x;
        for (int z=zStart; z<zEnd; z++) {
            
            for (int x=0; x<xEnd; x++) {
                BuildChunkColumn(x*chunkDimensions.x,z*chunkDimensions.z,false);

                

                yield return wfs;
            }
        }
        for (int z=0; z<zEnd; z++) {
            
            for (int x=xStart; x<xEnd; x++) {
                BuildChunkColumn(x*chunkDimensions.x,z*chunkDimensions.z,false);

                

                yield return wfs;
            }
        }
        
    }



    IEnumerator BuildRecursiveWorld(int x, int z, int rad){
        int newRad = rad-1;
        if(newRad<=0)yield break;

        BuildChunkColumn(x,z+chunkDimensions.z);
        buildQueue.Enqueue(BuildRecursiveWorld(x, z+chunkDimensions.z, newRad));
        yield return null;

        BuildChunkColumn(x,z-chunkDimensions.z);
        buildQueue.Enqueue(BuildRecursiveWorld(x, z-chunkDimensions.z, newRad));
        yield return null;

        BuildChunkColumn(x+chunkDimensions.x,z);
        buildQueue.Enqueue(BuildRecursiveWorld(x+chunkDimensions.x, z, newRad));
        yield return null;

        BuildChunkColumn(x-chunkDimensions.x,z);
        buildQueue.Enqueue(BuildRecursiveWorld(x-chunkDimensions.x, z, newRad));
        yield return null;
        
    }


    public void HideChunkColumns(int x, int z){
        for(int y=0; y<worldDimensions.y; y++){
            Vector3Int pos = new Vector3Int(x,y*chunkDimensions.y,z);
            if(chunkChecker.Contains(pos)){
                chunksDict[pos].meshRenderer.enabled=false;
            }
        }
    }


    IEnumerator HideColumns(int x, int z ){
        Vector2Int playerpos = new Vector2Int(x,z);
        foreach( Vector2Int cc in chunkColumns){
            if((cc-playerpos).magnitude>=drawRadius*chunkDimensions.x)HideChunkColumns(cc.x,cc.y);
        } 
        yield return null;
    }

    WaitForSeconds wfs=new WaitForSeconds(0.5f);
    WaitForSeconds wfs2 = new WaitForSeconds(0.25f);
    IEnumerator UpdateWorld(){
        while(true){
            if((lastBuildPos-fpc.transform.position).magnitude>chunkDimensions.x){
                lastBuildPos = Vector3Int.CeilToInt(fpc.transform.position);
                int posx = (int)(Mathf.Round(fpc.transform.position.x / chunkDimensions.x) * chunkDimensions.x);
                int posz = (int)(Mathf.Round(fpc.transform.position.z / chunkDimensions.z) * chunkDimensions.z);
                buildQueue.Enqueue(BuildRecursiveWorld(posx, posz, drawRadius));
                buildQueue.Enqueue(HideColumns(posx, posz));
            }
            yield return wfs2;
        }
    }

   
}
