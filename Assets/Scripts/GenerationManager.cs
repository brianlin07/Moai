﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GenerationManager : MonoBehaviour {

    public float time = 0.0f;

    public float chunk_size = 10;
    public int chunk_resolution = 10;
    public int chunk_load_dist = 1;
    public float amplitude = 20.0f;
    public int octaves = 2;
    public float persistence = 0.5f;
    public float smoothness = 0.02f;

    public GameObject[] chunk_prefabs;

    public GameObject player;
    public ChunkGenerator chunkGen;
    public TreeManager tree_manager;

    public Vector2 cur_chunk;
    List<Vector2> loaded_chunks;
    
	void Start () {
        player = GameObject.FindGameObjectWithTag("Player");
        tree_manager = gameObject.GetComponent<TreeManager>();
        chunkGen = gameObject.GetComponent<ChunkGenerator>();
        chunkGen.chunk_size = chunk_size;
        chunkGen.chunk_resolution = chunk_resolution;
        cur_chunk = new Vector2(-1, -1);
        loaded_chunks = new List<Vector2>();
        NoiseGen.init();
        NoiseGen.octaves = octaves;
        NoiseGen.persistence = persistence;
        NoiseGen.smoothness = smoothness;
    }
	
	// Update is called once per frame
	void Update () {
        checkPosition();
        if(Globals.time_scale > 1.0f) updateChunks();
    }

    // Checks where player is in current chunk. If outside current chunk, set new chunk to current, and reload surrounding chunks
    void checkPosition () {
        float player_x = player.transform.position.x;
        float player_y = player.transform.position.z; // In unity, y is vertical displacement
        Vector2 player_chunk = new Vector2(Mathf.FloorToInt(player_x/chunk_size), Mathf.FloorToInt(player_y / chunk_size));
        if(cur_chunk != player_chunk)
        {
            cur_chunk = player_chunk;
            unloadChunks();
            loadChunks();
        }
    }

    // Loads surrounding chunks within chunk_load_dist range
    void loadChunks() {
        for (int x = (int)cur_chunk.x - chunk_load_dist; x <= (int)cur_chunk.x + chunk_load_dist; x++)
        {
            for (int y = (int)cur_chunk.y - chunk_load_dist; y <= (int)cur_chunk.y + chunk_load_dist; y++)
            {
                Vector2 this_chunk = new Vector2(x, y);
                if (!loaded_chunks.Contains(this_chunk))
                {
                    generateChunk(x,y);
                    tree_manager.loadTrees(x, y);
                    loaded_chunks.Add(this_chunk);
                }
            }
        }
    }

    void unloadChunks()
    {
        for (int i = loaded_chunks.Count-1; i >= 0; i--)
        {
            Vector2 this_chunk = loaded_chunks[i];
            if (Mathf.Abs(this_chunk.x - cur_chunk.x) > chunk_load_dist ||
                Mathf.Abs(this_chunk.y - cur_chunk.y) > chunk_load_dist)
            {
                string chunk_name = "chunk (" + this_chunk.x + "," + this_chunk.y + ")";
                GameObject chunk = GameObject.Find(chunk_name);
                Destroy(chunk);
                tree_manager.unloadTrees((int)this_chunk.x, (int)this_chunk.y);
                loaded_chunks.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// STUB
    /// Generates a chunk from (noise function), using CreatePlane
    /// </summary>
    /// <param name="chunk_x"></param>
    /// <param name="chunk_y"></param>
    void generateChunk(int chunk_x, int chunk_y)
    {
        // Implement here
        if (Random.value > 0.5)
            chunkGen.generate(chunk_x, chunk_y, time, amplitude);
        else
        {
            string chunk_name = "chunk (" + chunk_x + "," + chunk_y + ")";
            Vector3 chunk_pos = new Vector3(chunk_x*chunk_size,0,chunk_y*chunk_size);
            GameObject prefab = chunk_prefabs[Mathf.FloorToInt(Random.value * chunk_prefabs.Length)];
            GameObject prefab_chunk = 
                Instantiate(prefab,chunk_pos, prefab.transform.rotation) as GameObject;
            prefab_chunk.name = chunk_name;
            prefab_chunk.GetComponent<PrefabChunk>().scaleToSettings(chunk_size,chunk_resolution);
        }
            

    }

    void updateChunks()
    {
        for (int i = loaded_chunks.Count - 1; i >= 0; i--)
        {
            Vector2 this_chunk = loaded_chunks[i];
            string chunk_name = "chunk (" + this_chunk.x + "," + this_chunk.y + ")";
            GameObject chunk = GameObject.Find(chunk_name);

            Vector3[] verts = chunk.GetComponent<MeshFilter>().mesh.vertices;
            for(int j = 0; j < verts.Length; j++)
            {
                float x = verts[j].x;
                float y = verts[j].z;
                float xpos = chunk.transform.position.x + x;
                float ypos = chunk.transform.position.z + y;

                verts[j] = new Vector3(x, amplitude * NoiseGen.genPerlin(xpos, ypos, Globals.time), y);
            }
            chunk.GetComponent<MeshFilter>().mesh.vertices = verts;
            chunk.GetComponent<MeshCollider>().sharedMesh = chunk.GetComponent<MeshFilter>().mesh;

            Color[] colors = new Color[verts.Length];
            for (int c = 0; c < verts.Length; c += 3)
            {
                float height = (verts[c].y + verts[c + 1].y + verts[c + 2].y) / 3;

                // colors[i] = environmentMapper.colorAtPos(xpos,vertices[c].y,ypos)
                Color color;
                if (height > 10)
                    color = new Color(0.9f, 0.9f, 0.9f);
                else if (height > -30)
                    color = new Color(0.1f, 0.4f, 0.2f);
                else
                    color = new Color(0.7f, 0.7f, 0.3f);
                colors[c] = color;
                colors[c + 1] = color;
                colors[c + 2] = color;
            }
            chunk.GetComponent<MeshFilter>().mesh.colors = colors;
        }
        

    }

}
