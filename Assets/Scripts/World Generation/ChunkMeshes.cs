﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChunkMeshes : MonoBehaviour{
    public Mesh lowMesh;
    public Mesh highMesh;
    public MeshFilter mf;

    //
    public Vector2 coordinates;

    // Generation Checkpoints 
    public bool doneBase = false;
    public bool doneObjects = false;
    public bool unloadedBase = false;
    public bool unloadedObjects = false;
    public bool detailed = false;
    public bool meshGenerated = false;
    public bool colliderGenerated = false;
    public bool treesGenerated = false;
    public bool doodadsGenerated = false;
    public bool shrinesGenerated = false;
    public bool obelisksGenerated = false;
    public bool lakesGenerated = false;

    public float XZDeviationRatio; //only deviates positively (sadly)
    public int XZDeviationSeed;
    public float detailDeviation;
    public int detailDeviationSeed;
    public int detailSubdivisions;

    //references
    private NoiseSynth synth;
    private GenerationManager genManager;
    private TreeManager treeManager;
    private ShrineManager shrineManager;
    private DoodadManager doodadManager;
    private WaterManager waterManager;

    // private
    private List<lakeStruct> lakes;
    private Biome biome;

    public void setReferences(NoiseSynth sy,GenerationManager g, TreeManager t,ShrineManager s,DoodadManager d,WaterManager w)
    {
        synth = sy;
        genManager = g;
        treeManager = t;
        shrineManager = s;
        doodadManager = d;
        waterManager = w;
    }

    public void loadBase()
    {
        if (System.DateTime.Now >= genManager.endTime) return;
        unloadedBase = false;
        if (!meshGenerated) generateMesh();
        if (meshGenerated && !colliderGenerated) generateMesh();
        else if (meshGenerated && colliderGenerated && !lakesGenerated) generateLakes();
        else if (meshGenerated && colliderGenerated && lakesGenerated) doneBase = true;
    }

    public void loadObjects()
    {
        if (System.DateTime.Now >= genManager.endTime) return;
        unloadedObjects = false;
        if (!doodadsGenerated) generateDoodads();
        if (doodadsGenerated && !treesGenerated) generateTrees();
        if (doodadsGenerated && treesGenerated && !shrinesGenerated) generateShrines();
        if (doodadsGenerated && treesGenerated && shrinesGenerated && !obelisksGenerated) generateObelisks();
        if (doodadsGenerated && treesGenerated && shrinesGenerated && obelisksGenerated) doneObjects = true;
    }

    public void unloadBase()
    {
        if (System.DateTime.Now >= genManager.endTime) return;
        if (meshGenerated) unloadMesh();
        if (!meshGenerated && lakesGenerated) unloadLakes();
        else if(!meshGenerated && !lakesGenerated) unloadedBase = true;
    }

    public void unloadObjects()
    {
        if (unloadedObjects || System.DateTime.Now >= genManager.endTime) return;
        if (treesGenerated) unloadTrees();
        if (!treesGenerated && doodadsGenerated) unloadDoodads();
        if (!treesGenerated && !doodadsGenerated && shrinesGenerated) unloadShrines();
        if (!treesGenerated && !doodadsGenerated && !shrinesGenerated && obelisksGenerated) unloadObelisks();
        if(!treesGenerated && !doodadsGenerated && !shrinesGenerated && !obelisksGenerated) unloadedObjects = true;
    }

    void generateMesh()
    {
        Biome biome = genManager.chooseBiome(coordinates);
        lakes = new List<lakeStruct>();

        lowMesh = new Mesh();
        lowMesh.name = "chunk (" + coordinates.x + "," + coordinates.y + ") [l]";

        Vector3 pos = new Vector3(coordinates.x * genManager.chunk_size, 0, coordinates.y * genManager.chunk_size);
        transform.position = pos;

        int originalSeed = Random.seed;

        // Generate lakes for this chunk
        Random.seed = Globals.SeedScript.seed + NoiseGen.hash((int)pos.x, (int)pos.y, (int)pos.z);

        for (int il = 0; il < biome.lakeCount; il++)
        {
            if (Random.value < biome.lakeChance)
            {
                Vector3 lakeSize = biome.lakeSize;
                Vector3 lakePos = pos;
                lakePos.x += Random.Range(0, genManager.chunk_size);
                lakePos.z += Random.Range(0, genManager.chunk_size);
                lakePos.x = Mathf.Clamp(lakePos.x, pos.x + lakeSize.x + 20, pos.x + genManager.chunk_size - lakeSize.x - 20);
                lakePos.z = Mathf.Clamp(lakePos.z, pos.z + lakeSize.z + 20, pos.z + genManager.chunk_size - lakeSize.z - 20);

                lakes.Add(new lakeStruct(lakePos, lakeSize));
            }
        }
        Random.seed = originalSeed;

        // Generate chunk_resolution^2 vertices
        Vector3[] vertices = new Vector3[(genManager.chunk_resolution * genManager.chunk_resolution)];

        for (int iy = 0; iy < genManager.chunk_resolution; iy++)
        {
            for (int ix = 0; ix < genManager.chunk_resolution; ix++)
            {
                float x = ix * genManager.chunk_size / (genManager.chunk_resolution - 1);
                float y = iy * genManager.chunk_size / (genManager.chunk_resolution - 1);
                float origx = x; float origy = y;

                float lakeOffset = 0;
                int lakeIntersections = 0;

                if (genManager.XZDeviationRatio != 0)
                {
                    Random.seed = Globals.SeedScript.seed + genManager.XZDeviationSeed + 
                        ((origx + coordinates.x * genManager.chunk_size).ToString() + "," + 
                        (origy + coordinates.y * genManager.chunk_size).ToString()).GetHashCode();
                    x = (ix + Random.value * genManager.XZDeviationRatio) *
                        genManager.chunk_size / (genManager.chunk_resolution - 1);
                    y = (iy + Random.value * genManager.XZDeviationRatio) *
                        genManager.chunk_size / (genManager.chunk_resolution - 1);
                }

                float xpos = x + pos.x;
                float zpos = y + pos.z;
                Vector3 vertexWorld = new Vector3(xpos, 0, zpos);

                // Take lakes into account when determining height,
                // BUt only if the vertex is not on the edge
                if (ix != 0 && iy != 0 && ix != genManager.chunk_resolution - 1 &&
                    iy != genManager.chunk_resolution - 1)
                {
                    foreach (lakeStruct lake in lakes)
                    {
                        // See if this vertex is in range of this lake
                        if (Vector3.Distance(vertexWorld, lake.position) <= lake.size.x)
                        {
                            lakeOffset += lake.size.y * Mathf.Max(0, Mathf.Pow((1 - (Vector3.Distance(vertexWorld, lake.position) / lake.size.x)), 3));
                            lakeIntersections++;
                        }
                    }
                }

                vertices[iy * genManager.chunk_resolution + ix] = 
                    new Vector3(x, synth.heightAt(origx + transform.position.x, origy + transform.position.z, 0) - lakeOffset, y);
            }
        }
        Random.seed = originalSeed;

        lowMesh.vertices = vertices;

        // Generate triangles using these vertices
        int[] triangles = new int[(genManager.chunk_resolution - 1) * (genManager.chunk_resolution - 1) * 6];
        int i = 0;
        // iterate through each quad in vertices
        for (int y = 0; y < genManager.chunk_resolution - 1; y++)
        {
            for (int x = 0; x < genManager.chunk_resolution - 1; x++)
            {
                // Specify quad edges as vertex indices
                //v1 ________ v2
                //  |        |
                //  |        |
                //  |        |
                //v3 _________  v4
                int v1 = x + y * genManager.chunk_resolution;
                int v2 = (x + 1) + y * genManager.chunk_resolution;
                int v3 = x + (y + 1) * genManager.chunk_resolution;
                int v4 = (x + 1) + (y + 1) * genManager.chunk_resolution;

                // Create two triangles from the quad
                if (Mathf.Repeat(x + y, 2) == 1)
                { //top left to bottom right
                    triangles[i] = v4;
                    triangles[i + 1] = v1;
                    triangles[i + 2] = v3;
                    triangles[i + 3] = v1;
                    triangles[i + 4] = v4;
                    triangles[i + 5] = v2;
                }
                else
                { //top right to bottom left
                    triangles[i] = v2;
                    triangles[i + 1] = v3;
                    triangles[i + 2] = v4;
                    triangles[i + 3] = v3;
                    triangles[i + 4] = v2;
                    triangles[i + 5] = v1;
                }

                i += 6;
            }
        }
        lowMesh.triangles = triangles;
        ReCalcTriangles(lowMesh);

        highMesh = Instantiate(lowMesh);
        highMesh.name = "chunk (" + coordinates.x + "," + coordinates.y + ") [h]";
        subDivide(highMesh, coordinates, detailSubdivisions);

        mf.mesh = lowMesh;

        MeshCollider collider = (MeshCollider) gameObject.AddComponent(typeof(MeshCollider));
        collider.sharedMesh = highMesh;
        meshGenerated = true;
    }

    void generateTrees()
    {
        treesGenerated = true;
    }

    void generateDoodads()
    {
        doodadsGenerated = true;
    }

    void generateShrines()
    {
        shrinesGenerated = true;
    }

    void generateObelisks()
    {
        obelisksGenerated = true;
    }

    void generateLakes()
    {
        foreach (lakeStruct lake in lakes)
        {
            waterManager.createWater(coordinates, lake.position, lake.size, biome);
        }
        lakesGenerated = true;
    }

    void unloadMesh()
    {
        meshGenerated = false;
    }    
         
    void unloadTrees()
    {
        treesGenerated = false;
    }    
         
    void unloadDoodads()
    {
        doodadsGenerated = false;
    }

    void unloadShrines()
    {
        shrinesGenerated = false;
    }
    void unloadObelisks()
    {
        obelisksGenerated = false;
    }    
         
    void unloadLakes()
    {
        lakesGenerated = false;
    }

    private void ReCalcTriangles(Mesh mesh)
    {
        Vector3[] oldVerts = mesh.vertices;
        int[] triangles = mesh.triangles;
        Vector3[] vertices = new Vector3[triangles.Length];
        for (int i = 0; i < triangles.Length; i++)
        {
            vertices[i] = oldVerts[triangles[i]];
            triangles[i] = i;
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    //assumes no shared vertices and returns no shared vertices
    private void subDivide(Mesh mesh, Vector2 coordinates, int numOfDivisions)
    {
        if (numOfDivisions < 1) return;
        Vector3[] oldVerts = mesh.vertices;
        Vector3[] vertices = new Vector3[oldVerts.Length * 4];
        int[] triangles = new int[oldVerts.Length * 4];

        int originalSeed = Random.seed;
        for (int i = 0; i < oldVerts.Length; i += 3)
        {
            Vector3 hypotMid = Vector3.Lerp(oldVerts[i], oldVerts[i + 1], 0.5f);
            Random.seed = Globals.SeedScript.seed + detailDeviationSeed + ((hypotMid.x + coordinates.x * genManager.chunk_size).ToString() + ","
                + (hypotMid.z + coordinates.y * genManager.chunk_size).ToString()).GetHashCode();
            hypotMid = new Vector3(hypotMid.x + Random.Range(-detailDeviation, detailDeviation), 
                hypotMid.y + Random.Range(-detailDeviation, detailDeviation), 
                hypotMid.z + Random.Range(-detailDeviation, detailDeviation));
            Vector3 midpoint1 = Vector3.Lerp(oldVerts[i + 1], oldVerts[i + 2], 0.5f);
            Vector3 midpoint2 = Vector3.Lerp(oldVerts[i + 2], oldVerts[i], 0.5f);
            vertices[i * 4] = hypotMid;
            vertices[i * 4 + 1] = oldVerts[i + 1];
            vertices[i * 4 + 2] = midpoint1;

            vertices[i * 4 + 3] = oldVerts[i + 2];
            vertices[i * 4 + 4] = hypotMid;
            vertices[i * 4 + 5] = midpoint1;

            vertices[i * 4 + 6] = hypotMid;
            vertices[i * 4 + 7] = oldVerts[i + 2];
            vertices[i * 4 + 8] = midpoint2;

            vertices[i * 4 + 9] = oldVerts[i];
            vertices[i * 4 + 10] = hypotMid;
            vertices[i * 4 + 11] = midpoint2;
        }
        Random.seed = originalSeed;
        for (int i = 0; i < triangles.Length; i++) triangles[i] = i;

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        if (numOfDivisions == 1)
        {
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
        }
        else subDivide(mesh, coordinates, numOfDivisions - 1);
    }

    private struct lakeStruct
    {
        public Vector3 position;
        public Vector3 size;

        public lakeStruct(Vector3 pos, Vector3 siz)
        {
            position = pos;
            size = siz;
        }
    }
}
