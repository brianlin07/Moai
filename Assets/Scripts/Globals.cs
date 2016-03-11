﻿using UnityEngine;
using System.Collections;

public class Globals : MonoBehaviour {

    public static float time = 0.0f; //incremented by Player.cs
    public static float time_scale = 1.0f;
	public static float time_resolution = Mathf.Pow(10, -20.0f);
    public static float deltaTime;

    public static float timeOfDay = 0;
    //0 for midnight, 90 for dawn
    //180 for noon, 270 for dusk
    //should not be or exceed 360. incremeted by Sky.cs

    public static Weather cur_weather;
    public static float water_level = 5;
    public static Biome cur_biome;

    public static GameObject Player = GameObject.Find("Player");
    public static Player PlayerScript = Player.GetComponent<Player>();
    public static Sky SkyScript = GameObject.Find("Sky").GetComponent<Sky>();

    public static T CopyComponent<T>(GameObject destination, T source) where T : Component {
        System.Type type = source.GetType();
        T component = destination.GetComponent<T>();
        if (!component) component = destination.AddComponent(type) as T;
        System.Reflection.FieldInfo[] fields = type.GetFields();
        foreach (System.Reflection.FieldInfo f in fields) {
            if (f.IsStatic) continue;
            f.SetValue(component, f.GetValue(source));
        }
        System.Reflection.PropertyInfo[] properties = type.GetProperties();
        foreach (System.Reflection.PropertyInfo prop in properties) {
            if (!prop.CanWrite || !prop.CanWrite || prop.Name == "name") continue;
            prop.SetValue(component, prop.GetValue(source, null), null);
        }
        return component;
    }
    public static Vector2 heatMoistureOrigin = new Vector2(25,25);
    public static Vector2 heatMoistureVector = Vector2.zero;
    public static float heatMoistureMin = 50;
    public static float heatMostureDistGuaranteed = 0.1f;  // The distance from the center point a biome is guaranteed to have puzzle
                                                           // FindObjectsOfType added to a shrine
}
