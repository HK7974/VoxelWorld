using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;

public class WorldController : MonoBehaviour
{

    public GameObject block_prefab;
    public int width = 10;
    public int height = 3;
    public int depth = 10;
    public int RandomMissing =3;
    

    
    [ContextMenu("Build World")]
    public void BuildWorldManual(){
        StartCoroutine(BuildWorld());
    }


    public IEnumerator BuildWorld(){
        for(int i =0; i<width; i++){
            for(int j=0; j<height; j++){
                for(int k=0; k<depth; k++){
                    if(j==height-1 && Random.Range(0,10)<RandomMissing)continue;
                    GameObject block = Instantiate(block_prefab, new Vector3(i,j,k), Quaternion.identity);
                    block.name = "Block "+i+" "+j+" "+k;
                    Debug.Log("building");
                }
                yield return null;
            }

        }
        
        
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(BuildWorld());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
}
