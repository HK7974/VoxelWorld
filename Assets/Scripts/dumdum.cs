using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class dumdum : MonoBehaviour
{

      public float voxelSize = 0.5f;  // Size of each voxel
    public Material voxelMaterial;  // Material for voxels

    private Mesh mesh;
    private MeshCollider meshCollider;

    void Start()
    {
        // Retrieve the mesh from the MeshFilter
        mesh = GetComponent<MeshFilter>().sharedMesh;
        if (mesh == null)
        {
            Debug.LogError("MeshFilter has no mesh assigned.");
            return;
        }

        // Add a MeshCollider for raycasting
        meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;

        // Generate and display the voxels
        GenerateVoxels();
    }

    void GenerateVoxels()
    {
        // Get the world space bounds of the mesh
        Bounds bounds = meshCollider.bounds;

        // Iterate through the bounding box of the mesh in voxel-sized steps
        for (float x = bounds.min.x; x < bounds.max.x; x += voxelSize)
        {
            for (float y = bounds.min.y; y < bounds.max.y; y += voxelSize)
            {
                for (float z = bounds.min.z; z < bounds.max.z; z += voxelSize)
                {
                    // Compute the center of the current voxel in world space
                    Vector3 voxelCenter = new Vector3(x, y, z) + Vector3.one * voxelSize / 2f;

                    // Check if this voxel is inside the mesh
                    if (IsPointInsideMesh(voxelCenter))
                    {
                        // Create a voxel at this position
                        CreateVoxel(voxelCenter);
                    }
                }
            }
        }
    }

    // Create a cube voxel at the given position
    void CreateVoxel(Vector3 position)
    {
        GameObject voxel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        voxel.transform.position = position;
        voxel.transform.localScale = Vector3.one * voxelSize;

        // Set the voxel's material
        if (voxelMaterial != null)
        {
            voxel.GetComponent<Renderer>().material = voxelMaterial;
        }

        // Remove the collider for performance (optional)
        Destroy(voxel.GetComponent<Collider>());
    }

    // Check if a point is inside the mesh using raycasting
    bool IsPointInsideMesh(Vector3 point)
    {
        // Cast a ray from the point in a direction (positive x-axis)
        Vector3 rayDirection = Vector3.right;

        // Perform a raycast and count intersections
        Ray ray = new Ray(point, rayDirection);
        int hitCount = 0;

        // Use RaycastAll to find all intersections with the mesh
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);

        foreach (RaycastHit hit in hits)
        {
            // Check if we hit the correct mesh collider
            if (hit.collider == meshCollider)
            {
                hitCount++;
            }
        }

        // Odd number of hits means the point is inside the mesh
        return hitCount % 2 == 1;
    }

}
    




