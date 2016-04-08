﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeatherManager : MonoBehaviour {
    // Tuning variables
    public float cloudHeight = 300;
    public Vector2 cloudMovement;
    public float chanceOfCloudFade;
    public float particlesHeight = 500;
    public float updateTime; //amount of time weather lasts (may change to same weather)

    public List<GameObject> cloudPrefabs;
    public float cloudPlacementRadius;
    public float cloudHeightVariation;
    public float cloudSizeVariation; //ratio (0.0 - 1.0)

    // Internal variables
    private float lastUpdated;
	private bool visibleParticles;
    private ParticleSystem activeParticleSystem;
    private List<Cloud> clouds; // holds all currently loaded clouds
    private Biome lastBiome;
    private Vector3 curParticlePosition;

    void Awake(){
        clouds = new List<Cloud>();
    }

    void Start() {
        visibleParticles = false;
        lastUpdated = 0;
        changeWeather();
        lastBiome = Globals.cur_biome;
        curParticlePosition = new Vector3(0, 0, 0);
    }

    // Update is called once per frame
    void Update(){
        if (Globals.time > lastUpdated + updateTime * Globals.time_resolution || lastBiome != Globals.cur_biome) {
            lastUpdated = Globals.time;
            changeWeather();
        }

        //check if visible
        visibleParticles = !(Globals.time_scale > 1 || Globals.PlayerScript.isUnderwater());
        if(activeParticleSystem) activeParticleSystem.gameObject.SetActive(visibleParticles);

        changeClouds();
        moveClouds();
        if(Random.value < chanceOfCloudFade * Globals.time_scale) {
            int randomCloud = (int)Mathf.Floor(Random.value * clouds.Count);
            clouds[randomCloud].dissipate();
            clouds.RemoveAt(randomCloud);
            createCloud();
        }

        lastBiome = Globals.cur_biome;
    }
	
	//public void hideWeather(){visibleParticles = false;}
	//public void showWeather(){visibleParticles = true;}
	//public void toggleWeather(){visibleParticles = !visibleParticles;}
	//public bool isVisible(){return visibleParticles;}

    public void changeWeather(){
        if (Globals.cur_biome == null) return;
        Weather lastWeather = null;
        if(Globals.cur_weather) lastWeather = Globals.cur_weather;

        //choose weather
        float roll = 0;
        for(int i = 0; i < Globals.cur_biome.weatherChance.Count; i++) roll += Globals.cur_biome.weatherChance[i];
        roll *= Random.value;
        for(int i = 0; i < Globals.cur_biome.weatherChance.Count; i++){
            if(roll - Globals.cur_biome.weatherChance[i] < 0) {
                Globals.cur_weather = Globals.cur_biome.weatherTypes[i];
                break;
            }else roll -= Globals.cur_biome.weatherChance[i];
        }

        // Switch to weather
        if (lastWeather != Globals.cur_weather) {
            if (activeParticleSystem) {
                Destroy(activeParticleSystem.gameObject);
                activeParticleSystem = null;
            }
            if (Globals.cur_weather.particleS) {
                activeParticleSystem = Instantiate(Globals.cur_weather.particleS);
                activeParticleSystem.transform.parent = transform;
                activeParticleSystem.transform.position = curParticlePosition;
            }
            Globals.cur_weather.imageSpace.applyToCamera();
        }
    }

    private void changeClouds() {
        if(clouds.Count < Globals.cur_weather.numberOfClouds) createCloud(); //i want more clouds
        else if(clouds.Count > Globals.cur_weather.numberOfClouds) { //i want less clouds
            clouds[0].dissipate();
            clouds.RemoveAt(0);
        }
    }

    private Cloud createCloud(int prefabNum, Vector3 location, Vector3 rotation, float scale) {
        GameObject c = Instantiate(cloudPrefabs[prefabNum]);
        c.transform.parent = transform;
        c.transform.eulerAngles = rotation;
        c.transform.localScale *= scale;
        c.transform.position = location;
        Cloud cl = c.GetComponent<Cloud>();
        clouds.Add(cl);
        return cl;
    }

    private Cloud createCloud() { //random everything
        Vector2 radial_offset = Random.insideUnitCircle * cloudPlacementRadius;
        return createCloud(Mathf.FloorToInt(Random.value * (cloudPrefabs.Count - 1)),
                           new Vector3(radial_offset.x + Globals.Player.transform.position.x,
                                       Random.Range(-cloudHeightVariation, cloudHeightVariation) + cloudHeight,
                                       radial_offset.y + Globals.Player.transform.position.z),
                           new Vector3(0, Random.Range(0f, 360f), 0),
                           1 + Random.Range(-cloudSizeVariation, cloudSizeVariation)
               );
    }

    private Cloud createCloud(Vector3 location) { //random everything except location
        return createCloud(Mathf.FloorToInt(Random.value * (cloudPrefabs.Count - 1)),
                           location, new Vector3(0, Random.Range(0f, 360f), 0),
                           1 + Random.Range(-cloudSizeVariation, cloudSizeVariation)
               );
    }

    private void moveClouds() {
        for(int i = 0; i < clouds.Count; i++) {
            clouds[i].gameObject.transform.position += new Vector3(cloudMovement.x, 0, cloudMovement.y) * Globals.time_scale;
            Vector2 cpos = new Vector2(clouds[i].gameObject.transform.position.x, clouds[i].gameObject.transform.position.z);
            Vector2 ppos = new Vector2(Globals.Player.transform.position.x, Globals.Player.transform.position.z);
            if(Vector2.Distance(cpos, ppos) > cloudPlacementRadius) {
                clouds[i].dissipate();
                clouds.RemoveAt(i);
                Vector2 npos = new Vector2(cloudMovement.x + ppos.x * 2 - cpos.x, cloudMovement.y + ppos.y * 2 - cpos.y);
                npos = Vector2.Lerp(npos, ppos, 0.05f);
                createCloud(new Vector3(npos.x, Random.Range(-cloudHeightVariation, cloudHeightVariation) + cloudHeight, npos.y));
                i--;
            }
        }
    }

    // moves particle system right above player
    // called from GenerationManager, not called constantly (will not follow player directly)
    public void moveParticles(Vector3 chunkCenter){
        curParticlePosition = new Vector3(chunkCenter.x, particlesHeight, chunkCenter.z);
        if (activeParticleSystem != null)
            activeParticleSystem.transform.position = curParticlePosition;
    }
}
