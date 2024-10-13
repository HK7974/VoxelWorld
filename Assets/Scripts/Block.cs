using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Block
{
    
    
    public Mesh mesh;
    Chunk parentChunk;

    
    // Start is called before the first frame update
    public Block(Vector3 offset, MeshUtils.Blocktype btype,Chunk chunk)
    {
        Vector3 localOffset = offset - chunk.location;
        if(btype == MeshUtils.Blocktype.AIR)return;

        parentChunk = chunk;
        List<Quad> quads = new List<Quad>();
        if(!HasSolidNeighbor((int)localOffset.x, (int)localOffset.y-1, (int)localOffset.z)) 
            quads.Add( new Quad(MeshUtils.BlockSide.BOTTOM, offset,btype==MeshUtils.Blocktype.GRASSIDE?MeshUtils.Blocktype.DIRT:btype));
        if(!HasSolidNeighbor((int)localOffset.x, (int)localOffset.y+1, (int)localOffset.z))
            quads.Add( new Quad(MeshUtils.BlockSide.TOP, offset,btype==MeshUtils.Blocktype.GRASSIDE?MeshUtils.Blocktype.GRASSTOP:btype));
        if(!HasSolidNeighbor((int)localOffset.x-1, (int)localOffset.y, (int)localOffset.z))
            quads.Add( new Quad(MeshUtils.BlockSide.LEFT, offset,btype));
        if(!HasSolidNeighbor((int)localOffset.x+1, (int)localOffset.y, (int)localOffset.z))
            quads.Add( new Quad(MeshUtils.BlockSide.RIGHT, offset,btype));
        if(!HasSolidNeighbor((int)localOffset.x, (int)localOffset.y, (int)localOffset.z+1))
            quads.Add( new Quad(MeshUtils.BlockSide.FRONT, offset,btype));
        if(!HasSolidNeighbor((int)localOffset.x, (int)localOffset.y, (int)localOffset.z-1))
            quads.Add( new Quad(MeshUtils.BlockSide.BACK, offset,btype));

        if(quads.Count == 0)return;

        Mesh[] sideMeshes = new Mesh[quads.Count];
        int sindex=0;
        foreach (Quad q in quads){
            sideMeshes[sindex] = q.mesh;
            sindex++;
        }
        

        mesh = MeshUtils.MergeMeshes(sideMeshes);
        mesh.name = "Cube_0_0_0"; 
        
    }

    public bool HasSolidNeighbor(int x, int y, int z){

        if(x<0 || x>=parentChunk.width || y<0 || y>=parentChunk.height || z<0 || z>=parentChunk.depth){
            return false;
        }
        if(parentChunk.chunkData[x + y*parentChunk.width + z*parentChunk.width*parentChunk.height] == MeshUtils.Blocktype.AIR ||
            parentChunk.chunkData[x + y*parentChunk.width + z*parentChunk.width*parentChunk.height] == MeshUtils.Blocktype.WATER){
                return false;
    }
    return true;}

    
}
