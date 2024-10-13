using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VertexData = System.Tuple<UnityEngine.Vector3, UnityEngine.Vector3, UnityEngine.Vector2>;

public static class MeshUtils 
{

    public enum Blocktype{
        GRASSTOP, GRASSIDE, STONE, WATER, DIRT, BED1,SAND,AIR,GOLD,DIAMOND,BEDROCK,REDSTONE,NOCRACK,CRACK1,CRACK2,CRACK3,CRACK4
    };

    public enum BlockSide { BOTTOM, TOP, LEFT, RIGHT, FRONT, BACK };

    // public static Vector2[,] blockUVs = {
    //     /*GRASTOP*/ {new Vector2(0.125f,0.375f), new Vector2(0.1875f,0.375f), new Vector2(0.125f,0.4375f), new Vector2(0.1875f,0.4375f)},
    //     /*GRASSIDE*/ {new Vector2(0.1875f,0.9375f), new Vector2(0.1875f,0.375f),  new Vector2(0.1875f,1f),new Vector2(0.25f,1f)},
    //     /*DIRT*/ {new Vector2(0.125f,0.9375f), new Vector2(0.1875f,0.9375f),  new Vector2(0.125f,1f),new Vector2(0.1875f,1f)},
    //     /*STONE*/ {new Vector2(0f,0.875f), new Vector2(0.0625f,0.875f),  new Vector2(0f,0.9375f),new Vector2(0.0625f,0.9375f)},
    //     /*WATER*/ {new Vector2(0.875f,0.125f), new Vector2(0.9375f,0.125f),  new Vector2(0.875f,0.1875f),new Vector2(0.9325f,0.1875f)},
    //     /*SAND*/ {new Vector2(0.125f,0.875f), new Vector2(0.1875f,0.875f),  new Vector2(0.125f,0.9375f),new Vector2(0.1825f,0.9375f)}
    // };

    public static Vector2[] blockUVs = {
        /*GRASTOP*/ new Vector2(1f,6f),
        /*GRASSIDE*/ new Vector2(3,15f),
        /*STONE*/ new Vector2(1f,15f),
        /*WATER*/ new Vector2(14f,2f),
        /*DIRT*/ new Vector2(2f,15f),
        /*BED1*/ new Vector2(6f,7f),
        /*SAND*/ new Vector2(2f,14f),
        /*AIR*/ new Vector2(4f,2f),
        /*GOLD*/ new Vector2(0,13f),
        /*DIAMOND*/ new Vector2(2f,12f),
        /*BEDROCK*/ new Vector2(1f,14f),
        /*REDSTONE*/ new Vector2(3f,12f),
        /*NOCRACK*/ new Vector2(0f,1f),
        /*CRACK1*/ new Vector2(0f,0f),
        /*CRACK2*/ new Vector2(2f,0f),
        /*CRACK3*/ new Vector2(4f,0f),
        /*CRACK4*/ new Vector2(6f,0f)
        
        
    };


    public static Mesh MergeMeshes(Mesh[] meshes)
    {
        Mesh mesh = new Mesh();

        Dictionary<VertexData, int> pointsOrder = new Dictionary<VertexData, int>();
        HashSet<VertexData> pointsHash = new HashSet<VertexData>();
        List<int> tris = new List<int>();

        int pIndex = 0;
        for (int i = 0; i < meshes.Length; i++) //loop through each mesh
        {
            if (meshes[i] == null) continue;
            for (int j = 0; j < meshes[i].vertices.Length; j++) //loop through each vertex of the current mesh
            {
                Vector3 v = meshes[i].vertices[j];
                Vector3 n = meshes[i].normals[j];
                Vector2 u = meshes[i].uv[j];
                VertexData p = new VertexData(v, n, u);
                if (!pointsHash.Contains(p))
                {
                    pointsOrder.Add(p, pIndex);
                    pointsHash.Add(p);

                    pIndex++;
                }

            }

            for (int t = 0; t < meshes[i].triangles.Length; t++)
            {
                int triPoint = meshes[i].triangles[t];
                Vector3 v = meshes[i].vertices[triPoint];
                Vector3 n = meshes[i].normals[triPoint];
                Vector2 u = meshes[i].uv[triPoint];
                VertexData p = new VertexData(v, n, u);

                int index;
                pointsOrder.TryGetValue(p, out index);
                tris.Add(index);
            }
            meshes[i] = null;
        }

        ExtractArrays(pointsOrder, mesh);
        mesh.triangles = tris.ToArray();
        mesh.RecalculateBounds();
        return mesh;
    }



    public static void ExtractArrays(Dictionary<VertexData, int> pointsOrder, Mesh mesh){
        List<Vector3> verts = new List<Vector3>();
        List<Vector3> norms = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        foreach (VertexData v in pointsOrder.Keys)
        {
            verts.Add(v.Item1);
            norms.Add(v.Item2);
            uvs.Add(v.Item3);
        }
        mesh.vertices = verts.ToArray();
        mesh.normals = norms.ToArray();
        mesh.uv = uvs.ToArray();

    }

    public static float fBM(float x, float z, float scale, float heightScale, int octaves, float heightOffset){
        float total =0;
        float freq =1f;
        for (int i=0;i<octaves;i++){
            total += Mathf.PerlinNoise(x*scale*freq,z*scale*freq)*heightScale;
            freq*=2;
        }
        return total + heightOffset;
    }
        public static float fBM3d(float x, float y,float z, float scale, float heightScale, int octaves, float heightOffset){
            float xy = fBM(x,y,scale,heightScale,octaves,heightOffset);
            float xz = fBM(x,z,scale,heightScale,octaves,heightOffset);
            float yz = fBM(y,z,scale,heightScale,octaves,heightOffset);
            float yx = fBM(y,x,scale,heightScale,octaves,heightOffset);
            float zx = fBM(z,x,scale,heightScale,octaves,heightOffset);
            float zy = fBM(z,y,scale,heightScale,octaves,heightOffset);

            return (xy+xz+yz+yx+zx+zy)/6f ;
        
        }

}
