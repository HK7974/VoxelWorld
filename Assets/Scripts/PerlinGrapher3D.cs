using System.Collections;
using System.Collections.Generic;

using UnityEngine;


[ExecuteInEditMode]
public class PerlinGrapher3D : MonoBehaviour
{

    Vector3 dimensions = new Vector3(10,10,10);
    public float heightScale =2f;
    public float scale =0.5f;
    public int octaves =1;
    public float heightOffset=0f;
    [Range(0.0f,10.0f)]
    public float drawCutOff =1f;



    void CreateCube(){
        for (int z=0; z<dimensions.z; z++) {
            for (int y=0; y<dimensions.y; y++) {
                for (int x=0; x<dimensions.x; x++) {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.name = x + " " + y + " " + z;
                    cube.transform.parent = this.transform;
                    cube.transform.position = new Vector3(x,y,z);
                    
                }}}
    }

    void Graph(){
        MeshRenderer[] mr = this.GetComponentsInChildren<MeshRenderer>();
        if (mr.Length==0) CreateCube();
        if(mr.Length==0) return;


        for (int z=0; z<dimensions.z; z++) {
            for (int y=0; y<dimensions.y; y++) {
                for (int x=0; x<dimensions.x; x++) {
                    float val = MeshUtils.fBM3d(x,y,z,scale,heightScale,octaves,heightOffset);
                    if(val<drawCutOff){
                        mr[x+(int)dimensions.x*(y+(int)dimensions.z*z)].enabled = false;
                    }
                    else mr[x+(int)dimensions.x*(y+(int)dimensions.z*z)].enabled = true;
                    
                }}}
    }
    void OnValidate(){
        Graph();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
