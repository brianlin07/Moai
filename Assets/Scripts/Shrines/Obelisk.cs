﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Obelisk : MonoBehaviour {

    // Tuning variables
    public float lightUpDistance = 10f;
    // list of islands that can spawn above an obelisk
    public List<GameObject> possibleIslands;
    public float islandHeight = 200f;

        // Requirements
    public int maxTotalReqs = 4;
    public int minTotalReqs = 2;
    public float teleportDelay = 10f;

    // Set materials for indicators
    public Material fireIndicatorDull;
    public Material fireIndicatorLit;
    public Material airIndicatorDull;
    public Material airIndicatorLit;
    public Material waterIndicatorDull;
    public Material waterIndicatorLit;
    public Material earthIndicatorDull;
    public Material earthIndicatorLit;
    public Material baseMat;
    public Material completedMat;
    
    // Control lighting up
    private bool litUp = false;

    // if star currency has been used on it
    private bool isDone = false;

    // if it has been snapped to terrain succefully
    private bool isPlaced = false;

    // References
    private Renderer m_renderer;
    private GameObject islandInstance;

    // Requirements
    // A dictionary of element name : count that is required to activate the obelisk
    private Dictionary<string, int> requirements;

	// Screen Fader
	private FadeInOut fader;

	private Vector3 telePos;

	private bool fromObelisk = false;

    public Vector3 saved_position;
    public Quaternion saved_rotation;
    public Vector3 saved_scale;

    //Audio Sources

    public AudioClip ObeliskSuccess;
    public AudioClip ObeliskFail;
    AudioSource SuccessAudio;
    AudioSource FailAudio;

    // Cull size
    public float cullSize;

	// Use this for initialization
	void Start () {
        requirements = new Dictionary<string, int>();
        generateRequirements();
        m_renderer = GetComponent<Renderer>();
        initMaterials();
        createIsland();
		fader = Globals.MenusScript.GetComponent<FadeInOut> ();
		telePos = islandInstance.GetComponentInChildren<TeleportStone> ().gameObject.transform.position;
        
        SuccessAudio = GetComponent<AudioSource>();
        FailAudio = GetComponent<AudioSource>();

		killTrees();
    }
	
	// Update is called once per frame
	void Update () 
    {
        if (!isPlaced) transform.position = snapToTerrain(transform.position);
        if (fromObelisk)
		{
			if (!fader.isFading() && fader.targetColor == Color.white)
			{
                Globals.Player.transform.position = telePos + new Vector3(-10, 100000,-10) * islandInstance.transform.localScale.x;

                RaycastHit hit;
                Ray rayDown = new Ray(Globals.Player.transform.position + Vector3.up * 100, Vector3.down);
                if(Physics.Raycast(rayDown, out hit, Mathf.Infinity, LayerMask.GetMask("Islands")))
                    Globals.Player.transform.position = new Vector3(Globals.Player.transform.position.x, hit.point.y + Camera.main.transform.localPosition.y, Globals.Player.transform.position.z);

                fader.fade(Color.clear, 1.5f);
            } 
			else if(!fader.isFading() && fader.targetColor == Color.clear)
			{
			    fromObelisk = false;
			}	
		}

	}

    void OnMouseOver()
    {
        if(isDone) return;
        float dist = Vector3.Distance(Globals.Player.transform.position, transform.position);
        if (!litUp && Globals.time_scale > 0 && Time.timeScale > 0 && dist < transform.localScale.x * 0.5f * lightUpDistance )
        {
            lightIndicators();
            
        }
        else if(litUp && dist > lightUpDistance)
        {
            dullIndicators();
        }

    }

    void OnMouseExit()
    {
        if(isDone) return;
        if (litUp && Globals.time_scale > 0 && Time.timeScale > 0)
        {
            dullIndicators();

        }
    }

    // Sound here
    void OnMouseDown()
    {
        float dist = Vector3.Distance(Globals.Player.transform.position, transform.position);
        if (litUp && dist < (transform.localScale.x -1 ) * 0.5f * lightUpDistance + lightUpDistance && Globals.time_scale == 1 && Time.timeScale == 1 && !fromObelisk && (isDone || areReqsMet()))
        {
            fader.fade(Color.white, 1.5f);
			fromObelisk = true;
            if (!isDone)
            {
                spendStars();
                // Success sound
                SuccessAudio.PlayOneShot(ObeliskSuccess, .5F);
                lightIndicators();
            }
        }
        else
        {
            // Failure sound
            FailAudio.PlayOneShot(ObeliskFail, .5F);
        }
        Globals.MenusScript.GetComponent<HUD>().ping();
    }



    public bool usable() {
        return isDone || areReqsMet();
    }

    private void spendStars()
    {
        // Get rid of stars
        foreach(KeyValuePair<string, List<GameObject>> p in Globals.Stars) {
            for(int i = 0; i < requirements[p.Key]; i++) {
                int r = Random.Range(0, Globals.Stars[p.Key].Count);
                //Globals.SkyScript.changeStar("normal", Globals.Stars[p.Key][r]); // may clutter up middle part
                Globals.SkyScript.removeStar(Globals.Stars[p.Key][r]); // this just removes the star
                Globals.Stars[p.Key].RemoveAt(r);
                Globals.MenusScript.GetComponent<StarHUD>().removeStar(p.Key);
                for(int j = 0; j < Globals.SkyScript.extraStars; j++) Globals.SkyScript.addStar();
            }
        }

        isDone = true;
    }

    private bool areReqsMet()
    {
        foreach(KeyValuePair<string, List<GameObject>> p in Globals.Stars)
            if(Globals.Stars[p.Key].Count < requirements[p.Key]) return false;
        return true;
    }

    private void generateRequirements()
    {
        int count = Random.Range(minTotalReqs, maxTotalReqs);
        requirements["air"] = 0;
        requirements["earth"] = 0;
        requirements["fire"] = 0;
        requirements["water"] = 0;
        for (int i = 0;i < count;i ++)
        {
            string element;
            float rand = Random.value;
            if (rand > 0.75) element = "fire";
            else if (rand > 0.5) element = "water";
            else if (rand > 0.25) element = "earth";
            else element = "air";
            if (!requirements.ContainsKey(element)) requirements[element] = 0;
            requirements[element]++;
        }
    }

    private void createIsland()
    {
        GameObject prefab = possibleIslands[Random.Range(0, possibleIslands.Count)];
        islandInstance = Instantiate(prefab,transform.position + Vector3.up * (islandHeight +prefab.transform.position.y), Quaternion.Euler(-90, Random.Range(0, 360), 0)) as GameObject;
        islandInstance.GetComponentInChildren<TeleportStone>().linkedObelisk = gameObject;
        islandInstance.transform.parent = transform;
    }

    /* 
    air
        1 - [2]
        2 - [1]
        3 - [3]
        4 - [4]
        5 - [5]
      
    fire
        1 - [9]
        2 - [10]
        3 - [8]
        4 - [7]
        5 - [6]
    earth
        1 - [15]
        2 - [16]
        3 - [14]
        4 - [13]
        5 - [12]
    water
        1 - [20]
        2 - [19]
        3 - [21]
        4 - [22]
        5 - [23]
    */

    void initMaterials()
    {
        Material[] tempMats = m_renderer.sharedMaterials;
        // Air
        tempMats[2] = baseMat;
        tempMats[1] = baseMat;
        tempMats[3] = baseMat;
        tempMats[4] = baseMat;
        tempMats[5] = baseMat;

        // Fire
        tempMats[9] = baseMat;
        tempMats[10] = baseMat;
        tempMats[8] = baseMat;
        tempMats[7] = baseMat;
        tempMats[6] = baseMat;

        // Earth
        tempMats[15] = baseMat;
        tempMats[16] = baseMat;
        tempMats[14] = baseMat;
        tempMats[13] = baseMat;
        tempMats[12] = baseMat;

        // Water
        tempMats[20] = baseMat;
        tempMats[19] = baseMat;
        tempMats[21] = baseMat;
        tempMats[22] = baseMat;
        tempMats[23] = baseMat;
        m_renderer.sharedMaterials = tempMats;
        dullIndicators();
    }

    void lightIndicators()
    {
        int airLevel    = Mathf.Min(requirements["air"],   Globals.Stars["air"].Count);
        int earthLevel  = Mathf.Min(requirements["earth"], Globals.Stars["earth"].Count);
        int fireLevel   = Mathf.Min(requirements["fire"],  Globals.Stars["fire"].Count);
        int waterLevel  = Mathf.Min(requirements["water"], Globals.Stars["water"].Count);
        // Light 
        // Dull the active slots
        Material[] tempMats = m_renderer.sharedMaterials;
        if (!isDone)
        {
            // Air
            if (requirements.ContainsKey("air"))
            {
                if (airLevel >= 1) tempMats[2] = airIndicatorLit;
                if (airLevel >= 2) tempMats[1] = airIndicatorLit;
                if (airLevel >= 3) tempMats[3] = airIndicatorLit;
                if (airLevel >= 4) tempMats[4] = airIndicatorLit;
                if (airLevel >= 5) tempMats[5] = airIndicatorLit;
            }

            // Fire
            if (requirements.ContainsKey("fire"))
            {
                if (fireLevel >= 1) tempMats[9] = fireIndicatorLit;
                if (fireLevel >= 2) tempMats[10] = fireIndicatorLit;
                if (fireLevel >= 3) tempMats[8] = fireIndicatorLit;
                if (fireLevel >= 4) tempMats[7] = fireIndicatorLit;
                if (fireLevel >= 5) tempMats[6] = fireIndicatorLit;
            }

            // Earth
            if (requirements.ContainsKey("earth"))
            {
                if (earthLevel >= 1) tempMats[15] = earthIndicatorLit;
                if (earthLevel >= 2) tempMats[16] = earthIndicatorLit;
                if (earthLevel >= 3) tempMats[14] = earthIndicatorLit;
                if (earthLevel >= 4) tempMats[13] = earthIndicatorLit;
                if (earthLevel >= 5) tempMats[12] = earthIndicatorLit;
            }

            // Water
            if (requirements.ContainsKey("water"))
            {
                if (waterLevel >= 1) tempMats[20] = waterIndicatorLit;
                if (waterLevel >= 2) tempMats[19] = waterIndicatorLit;
                if (waterLevel >= 3) tempMats[21] = waterIndicatorLit;
                if (waterLevel >= 4) tempMats[22] = waterIndicatorLit;
                if (waterLevel >= 5) tempMats[23] = waterIndicatorLit;
            }
        }
        // If done, light it all up in white
        else
        {
            tempMats[2]    = completedMat;
            tempMats[1]    = completedMat;
            tempMats[3]    = completedMat;
            tempMats[4]    = completedMat;
            tempMats[5]    = completedMat;
            tempMats[9]    = completedMat;
            tempMats[10]   = completedMat;
            tempMats[8]    = completedMat;
            tempMats[7]    = completedMat;
            tempMats[6]    = completedMat;
            tempMats[15]   = completedMat;
            tempMats[16]   = completedMat;
            tempMats[14]   = completedMat;
            tempMats[13]   = completedMat;
            tempMats[12]   = completedMat;
            tempMats[20]   = completedMat;
            tempMats[19]   = completedMat;
            tempMats[21]   = completedMat;
            tempMats[22]   = completedMat;
            tempMats[23] = completedMat;
        }

        m_renderer.sharedMaterials = tempMats;
        litUp = true;
    }

    void dullIndicators()
    {
        // Dull the active slots
        Material[] tempMats = m_renderer.sharedMaterials;

        if (!isDone)
        {
            // Air
            if (requirements.ContainsKey("air"))
            {
                if (requirements["air"] >= 1) tempMats[2] = airIndicatorDull;
                if (requirements["air"] >= 2) tempMats[1] = airIndicatorDull;
                if (requirements["air"] >= 3) tempMats[3] = airIndicatorDull;
                if (requirements["air"] >= 4) tempMats[4] = airIndicatorDull;
                if (requirements["air"] >= 5) tempMats[5] = airIndicatorDull;
            }


            // Fire
            if (requirements.ContainsKey("fire"))
            {
                if (requirements["fire"] >= 1) tempMats[9] = fireIndicatorDull;
                if (requirements["fire"] >= 2) tempMats[10] = fireIndicatorDull;
                if (requirements["fire"] >= 3) tempMats[8] = fireIndicatorDull;
                if (requirements["fire"] >= 4) tempMats[7] = fireIndicatorDull;
                if (requirements["fire"] >= 5) tempMats[6] = fireIndicatorDull;
            }

            // Earth
            if (requirements.ContainsKey("earth"))
            {
                if (requirements["earth"] >= 1) tempMats[15] = earthIndicatorDull;
                if (requirements["earth"] >= 2) tempMats[16] = earthIndicatorDull;
                if (requirements["earth"] >= 3) tempMats[14] = earthIndicatorDull;
                if (requirements["earth"] >= 4) tempMats[13] = earthIndicatorDull;
                if (requirements["earth"] >= 5) tempMats[12] = earthIndicatorDull;
            }

            // Water
            if (requirements.ContainsKey("water"))
            {
                if (requirements["water"] >= 1) tempMats[20] = waterIndicatorDull;
                if (requirements["water"] >= 2) tempMats[19] = waterIndicatorDull;
                if (requirements["water"] >= 3) tempMats[21] = waterIndicatorDull;
                if (requirements["water"] >= 4) tempMats[22] = waterIndicatorDull;
                if (requirements["water"] >= 5) tempMats[23] = waterIndicatorDull;
            }
        }
        // If it's done, just shut it all down
        else
        {
            // Air
            tempMats[2] = baseMat;
            tempMats[1] = baseMat;
            tempMats[3] = baseMat;
            tempMats[4] = baseMat;
            tempMats[5] = baseMat;

            // Fire
            tempMats[9] = baseMat;
            tempMats[10] = baseMat;
            tempMats[8] = baseMat;
            tempMats[7] = baseMat;
            tempMats[6] = baseMat;

            // Earth
            tempMats[15] = baseMat;
            tempMats[16] = baseMat;
            tempMats[14] = baseMat;
            tempMats[13] = baseMat;
            tempMats[12] = baseMat;

            // Water
            tempMats[20] = baseMat;
            tempMats[19] = baseMat;
            tempMats[21] = baseMat;
            tempMats[22] = baseMat;
            tempMats[23] = baseMat;
        }

        m_renderer.sharedMaterials = tempMats;
        litUp = false;
    }

    private void killTrees()
    {
        Vector3 half_extents = new Vector3(cullSize, 100000, cullSize);
        LayerMask tree_mask = LayerMask.GetMask("Tree", "Doodad", "BigDoodad");

        Collider[] colliders = Physics.OverlapBox(transform.position, half_extents, Quaternion.identity, tree_mask);
        for (int i = 0; i < colliders.Length; i++)
        {

            GameObject tree = colliders[i].gameObject;
            Destroy(tree);
        }
    }

    public void saveTransforms()
    {
        saved_position = transform.position;
        saved_rotation = transform.rotation;
        saved_scale = transform.localScale;
    }

    public void copyFrom(Obelisk obelisk)
    {
        isDone = obelisk.isDone;
        cullSize = obelisk.cullSize;
        saved_position = obelisk.saved_position;
        saved_rotation = obelisk.saved_rotation;
        saved_scale = obelisk.saved_scale;
    }

    private Vector3 snapToTerrain(Vector3 pos)
    {

        Vector3 ret = pos;
        RaycastHit hit;
        Ray rayDown = new Ray(new Vector3(pos.x, 10000000, pos.z), Vector3.down);
        int terrain = LayerMask.GetMask("Terrain");

        if (Physics.Raycast(rayDown, out hit, Mathf.Infinity, terrain))
        {
            isPlaced = true;
            ret = new Vector3(pos.x, hit.point.y + 0.1f, pos.z);
            
        }
        return ret;
    }
}
