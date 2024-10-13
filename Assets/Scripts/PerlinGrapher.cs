using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;


[ExecuteInEditMode]
public class PerlinGrapher : MonoBehaviour
{
    
    public LineRenderer lineRenderer;
    public float heightScale =2f;
    public float scale =0.5f;
    public int octaves =1;
    public float heightOffset=0f;
    [Range(0.0f,1.0f)]
    public float probability =1f;


    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 100;
        Graph();

    }


    // float fBM(float x, float z){
    //     float total =0;
    //     float freq =1f;
    //     for (int i=0;i<octaves;i++){
    //         total += Mathf.PerlinNoise(x*scale*freq,z*scale*freq)*heightScale;
    //         freq*=2;
    //     }
    //     return total;
    // }

    void Graph(){
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 100;
        int zPos = 40;
        Vector3[] positions = new Vector3[lineRenderer.positionCount];
        for (int x=0;x<lineRenderer.positionCount;x++){
            float y = MeshUtils.fBM(x,zPos,scale,heightScale,(int)octaves,heightOffset) ;
            positions[x] = new Vector3(x,y,zPos);
        }
        lineRenderer.SetPositions(positions);
    }


    //Validate runs whenevr value changes in inspector
    void OnValidate(){
        Graph();
    }
   
}
