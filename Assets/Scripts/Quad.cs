using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quad 
{
    public Mesh mesh;
    public float blockScale =1f;
        
    public Quad(MeshUtils.BlockSide side, Vector3 offset, MeshUtils.Blocktype bType)
    {
      
        mesh = new Mesh();
        mesh.name = "Procedural Quad";
        Vector3[] vertices = new Vector3[4];
        Vector3[] normals = new Vector3[4];
        Vector2[] uvs = new Vector2[4];
        int[] triangles = new int[6];

        Vector2 uv00 =new Vector2(0,0) + MeshUtils.blockUVs[(int)bType]*0.0625f;
        Vector2 uv01 =new Vector2(0,0.0625f)+ MeshUtils.blockUVs[(int)bType]*0.0625f;
        Vector2 uv10 =new Vector2(0.0625f,0)+ MeshUtils.blockUVs[(int)bType]*0.0625f;
        Vector2 uv11 =new Vector2(0.0625f,0.0625f)+ MeshUtils.blockUVs[(int)bType]*0.0625f;

        Vector3 v0 = (new Vector3(-0.5f,-0.5f,0.5f) + offset) * blockScale;
        Vector3 v1 = (new Vector3(0.5f,-0.5f,0.5f) + offset)* blockScale;
        Vector3 v2 = (new Vector3(0.5f,-0.5f,-0.5f) + offset)* blockScale;
        Vector3 v3 = (new Vector3(-0.5f,-0.5f,-0.5f) + offset)* blockScale;
        Vector3 v4 = (new Vector3(-0.5f,0.5f,0.5f) + offset)* blockScale;
        Vector3 v5 = (new Vector3(0.5f,0.5f,0.5f) + offset)* blockScale;
        Vector3 v6 = (new Vector3(0.5f,0.5f,-0.5f) + offset)* blockScale;
        Vector3 v7 = (new Vector3(-0.5f,0.5f,-0.5f) + offset)* blockScale;

        switch (side){
            case MeshUtils.BlockSide.FRONT:{
                vertices = new Vector3[] {v4, v5, v1,v0};
                normals =new Vector3[] {Vector3.forward,Vector3.forward,Vector3.forward,Vector3.forward};
                uvs = new Vector2[] {uv11,uv01,uv00,uv10};
                triangles = new int[] {3,1,0,3,2,1};
                break;
            }
            case MeshUtils.BlockSide.BACK:{
                vertices = new Vector3[] {v6, v7, v3,v2};
                normals =new Vector3[] {Vector3.back,Vector3.back,Vector3.back,Vector3.back};
                uvs = new Vector2[] {uv11,uv01,uv00,uv10};
                triangles = new int[] {3,1,0,3,2,1};
                break;
            }
            case MeshUtils.BlockSide.RIGHT:{
                vertices = new Vector3[] {v5, v6, v2,v1};
                normals =new Vector3[] {Vector3.right,Vector3.right,Vector3.right,Vector3.right};
                uvs = new Vector2[] {uv11,uv01,uv00,uv10};
                triangles = new int[] {3,1,0,3,2,1};
                break;
            }
            case MeshUtils.BlockSide.LEFT:{
                vertices = new Vector3[] {v7, v4, v0,v3};
                normals =new Vector3[] {Vector3.left,Vector3.left,Vector3.left,Vector3.left};
                uvs = new Vector2[] {uv11,uv01,uv00,uv10};
                triangles = new int[] {3,1,0,3,2,1};
                break;
            }
            case MeshUtils.BlockSide.TOP:{
                vertices = new Vector3[] {v7, v6, v5,v4};
                normals =new Vector3[] {Vector3.up,Vector3.up,Vector3.up,Vector3.up};
                uvs = new Vector2[] {uv11,uv01,uv00,uv10};
                triangles = new int[] {3,1,0,3,2,1};
                break;
            }
            case MeshUtils.BlockSide.BOTTOM:{
                vertices = new Vector3[] {v0, v1, v2,v3};
                normals =new Vector3[] {Vector3.down,Vector3.down,Vector3.down,Vector3.down};
                uvs = new Vector2[] {uv11,uv01,uv00,uv10};
                triangles = new int[] {3,1,0,3,2,1};
                break;
            }
                
        }
        

        mesh.vertices = vertices;
        mesh.normals= normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
       
    }

    
}
