﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Globals : MonoBehaviour {
    public static float time = 0.0f; //incremented by TimeScript
    public static float time_scale = 1.0f;
    public static float time_resolution = Mathf.Pow(10, -20.0f);
    public static float deltaTime;
    public static int mode = -1; // -1 main menu, 0 playing, 1 paused
    public static bool loading = false;

    public static float timeOfDay = 0;
    //incremeted by Sky
    //0 for midnight, 90 for dawn
    //180 for noon, 270 for dusk
    //should not be or exceed 360

    public static Weather cur_weather; // set by WeatherManager
    public static float water_level; // set by WaterScript
    public static Biome cur_biome;
    public static Vector2 cur_chunk;

    //references
    public static GameObject Player = GameObject.Find("Player");
    public static Player PlayerScript = Player.GetComponent<Player>();
    public static Sky SkyScript = GameObject.Find("Sky").GetComponent<Sky>();
    public static WeatherManager WeatherManagerScript = GameObject.Find("Weather").GetComponent<WeatherManager>();
    public static GameObject WorldGen = GameObject.Find("WorldGen");
    public static GenerationManager GenerationManagerScript = WorldGen.GetComponent<GenerationManager>();
    public static TreeManager TreeManagerScript = WorldGen.GetComponent<TreeManager>();
    public static WaterManager WaterManagerScript = WorldGen.GetComponent<WaterManager>();
    public static Seed SeedScript = WorldGen.GetComponent<Seed>();
    public static GameObject UI = GameObject.Find("UI");
    public static Menus MenusScript = UI.GetComponent<Menus>();

    //cheats
    public static bool chrono = false;

    //Biome Elements
    public static Vector2 WaterFireEarthAirOrigin = new Vector2(0.5f, 0.5f);
    public static Vector2 WaterFireEarthAirVector = Vector2.zero;
    public static float WaterFireEarthAirMin = 0.5f;
    public static float WaterFireEarthAirDistGuaranteed = 10f;  // The distance from the center point a biome is guaranteed to have puzzle
                                                                // FindObjectsOfType added to a shrine

    public static Dictionary<string, List<GameObject>> Stars = new Dictionary<string, List<GameObject>>() {
        { "fire",  new List<GameObject>() },
        { "water", new List<GameObject>() },
        { "air",   new List<GameObject>() },
        { "earth", new List<GameObject>() },
    };

    public static Dictionary<string, int> settings = new Dictionary<string, int>(){
        // Gameplay
        { "FOV", 60 },          // int 30-110
        { "DOF", 1 },           // bool
        { "WaitCinematic", 1},  // bool
        { "Bobbing", 1 },       // bool
        { "FOVKick", 1 },       // bool
        { "SmoothCamera", 0 },  // bool

        // HUD
        { "ShowHUD", 1 },       // bool
        { "Crosshair", 1 },     // bool
        { "Tooltip", 1 },       // bool
        { "StarIcons", 1 },     // bool

        // Controls
        { "InvertMouse", 0 },   // bool
        { "Sensitivity", 4 },   // int 1-10

        // Video
        { "Resolution", 0 },    // int (index of Screen.resolutions)
        { "Screenmode", 1 },    // 0 windowed, 1 full, 2 borderless
        { "LoadDist", 8 },      // int
        { "Brightness", 50 },   // percent 0-100
        { "ShadowDist", 15 },  // int

        // Audio
        { "MasterVol", 100 },   // percent 0-100
        { "MusicVol", 50 },     // percent 0-100
        { "SFXVol", 100 },      // percent 0-100
    };

    public static T CopyComponent<T>(GameObject destination, T source) where T : Component{
        System.Type type = source.GetType();
        T component = destination.GetComponent<T>();
        if (!component) component = destination.AddComponent(type) as T;
        System.Reflection.FieldInfo[] fields = type.GetFields();
        foreach (System.Reflection.FieldInfo f in fields)
        {
            if (f.IsStatic) continue;
            f.SetValue(component, f.GetValue(source));
        }
        System.Reflection.PropertyInfo[] properties = type.GetProperties();
        foreach (System.Reflection.PropertyInfo prop in properties)
        {
            if (!prop.CanWrite || !prop.CanWrite || prop.Name == "name") continue;
            prop.SetValue(component, prop.GetValue(source, null), null);
        }
        return component;
    }

    public static void Log(string message) {
        UI.GetComponent<CheatConsole>().Log(message);
    }
}
