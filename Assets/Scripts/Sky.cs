﻿using UnityEngine;
using System.Collections;

public class Sky : MonoBehaviour {
	//attach to sky
	public float timeresPerDegree = 10; //x axis
	public GameObject SunLight;
	public GameObject MoonLight;
	public float horizonBufferAngle = 0f;
	public float sunAxisShift = 30;
	public float timeresPerAxisShiftDegrees = 1000000; // z axis
	
	
	//finals
	private GameObject Player;
	private Vector3 originalAngle;
	private float sunMaxIntensity; //will take intensity of light set in editor
	private float moonMaxIntensity; //ditto above
	
	private float timeOfDay; //0 for dawn, 90 for noon, 180 for dusk, 270 for midnight. Should not be or exceed 360.
	private Skybox skyGoal;
	private Skybox skyDelta;
	private float timeRemain;
	
	//temp
	private Skybox am4 = new Skybox(new Color(0.094f, 0.149f, 0.212f, 1.0f), new Color(0.129f, 0.149f, 0.271f, 1.0f), new Color(0.173f, 0.161f, 0.278f, 1.0f), 1.0f, 1.0f, 1.0f);
	private Skybox am8 = new Skybox(new Color(0.220f, 0.600f, 1.000f, 1.0f), new Color(0.800f, 0.592f, 0.961f, 1.0f), new Color(0.867f, 0.753f, 0.659f, 1.0f), 1.0f, 1.0f, 1.0f);
	private Skybox pm12= new Skybox(new Color(0.463f, 0.882f, 0.882f, 1.0f), new Color(0.475f, 0.882f, 1.000f, 1.0f), new Color(0.718f, 0.945f, 1.000f, 1.0f), 1.0f, 1.0f, 1.0f);
	private Skybox pm4 = new Skybox(new Color(0.604f, 0.961f, 1.000f, 1.0f), new Color(0.729f, 0.973f, 1.000f, 1.0f), new Color(0.855f, 0.984f, 1.000f, 1.0f), 1.0f, 1.0f, 1.0f);
	private Skybox pm8 = new Skybox(new Color(0.490f, 0.067f, 0.620f, 1.0f), new Color(0.792f, 0.118f, 0.408f, 1.0f), new Color(0.933f, 0.212f, 0.243f, 1.0f), 1.0f, 1.0f, 1.0f);
	private Skybox am12= new Skybox(new Color(0.055f, 0.114f, 0.141f, 1.0f), new Color(0.086f, 0.114f, 0.208f, 1.0f), new Color(0.114f, 0.125f, 0.208f, 1.0f), 1.0f, 1.0f, 1.0f);
	
	private class Skybox{
		public Color color1;
		public Color color2;
		public Color color3;
		public float exponent1;
		public float exponent2;
		public float intensity;
		
		public Skybox(Color c1, Color c2, Color c3, float e1, float e2, float i){
			color1 = c1; color2 = c2; color3 = c3; exponent1 = e1; exponent2 = e2; intensity = i;
		}
		
		public static Skybox operator+(Skybox a, Skybox b){
			return new Skybox(a.color1 + b.color1, a.color2 + b.color2, a.color3 + b.color3, a.exponent1 + b.exponent1, a.exponent2 + b.exponent2, a.intensity + b.intensity);
		}
		
		public static Skybox operator-(Skybox a, Skybox b){
			return new Skybox(a.color1 - b.color1, a.color2 - b.color2, a.color3 - b.color3, a.exponent1 - b.exponent1, a.exponent2 - b.exponent2, a.intensity - b.intensity);
		}
		
		public static Skybox operator*(Skybox s, float f){
			return new Skybox(s.color1 * f, s.color2 * f, s.color3 * f, s.exponent1 * f, s.exponent2 * f, s.intensity * f);
		}
		
		public static Skybox operator/(Skybox s, float f){
			return new Skybox(s.color1 / f, s.color2 / f, s.color3 / f, s.exponent1 / f, s.exponent2 / f, s.intensity / f);
		}
	}
	
	void Awake(){ //set finals
		Player = GameObject.FindGameObjectWithTag("Player");
		originalAngle = transform.eulerAngles;
		sunMaxIntensity = SunLight.GetComponent<Light>().intensity;
		moonMaxIntensity = MoonLight.GetComponent<Light>().intensity;
		//changeSkyByTime(am8, 0); //turn sky back to default color
		RenderSettings.skybox = Object.Instantiate(RenderSettings.skybox); //comment out when debugging
	}
	
    void Start(){
		//timeOfDay = Mathf.Repeat(Globals.time/Globals.time_resolution/timeresPerDegree + originalAngle.x, 360);
		changeSkyByTime(am4, 0);
    }

    void Update(){
		timeOfDay = Mathf.Repeat(Globals.time/Globals.time_resolution/timeresPerDegree + originalAngle.x, 360);
		updateSunMoon();
		//updateSky(); //disabled sky change. Setting changeSky with time of 0 will still instantly change the sky.
    }
	
	private void updateSunMoon(){
		transform.eulerAngles = new Vector3(timeOfDay, originalAngle.y, originalAngle.z); //update angle of sun/moon with timeofday
		transform.position = new Vector3(Player.transform.position.x, 0, Player.transform.position.z); //follow player
		
		// Modulate sun/moon intensity
		if(timeOfDay > horizonBufferAngle && timeOfDay <= 180 - horizonBufferAngle){ //day
			SunLight.GetComponent<Light>().intensity = sunMaxIntensity;
			MoonLight.GetComponent<Light>().intensity = 0.0f;
			if(timeOfDay <= 90){ //morning
				float ratio = ((timeOfDay - horizonBufferAngle) / (90 - horizonBufferAngle));
				setSky(am8 * (1 - ratio) + pm12 * ratio);
			}else{ //afternoon
				float ratio = ((timeOfDay - 90) / (90 - horizonBufferAngle));
				setSky(pm12 * (1 - ratio) + pm4 * ratio);
			}
		}else if(timeOfDay > 180 - horizonBufferAngle && timeOfDay <= 180 + horizonBufferAngle){ //sunset
			SunLight.GetComponent<Light>().intensity = sunMaxIntensity * (1.0f - (timeOfDay - (180 - horizonBufferAngle)) / (horizonBufferAngle * 2));
			MoonLight.GetComponent<Light>().intensity = moonMaxIntensity * ((timeOfDay - (180 - horizonBufferAngle)) / (horizonBufferAngle * 2));
			float ratio = ((timeOfDay - (180 - horizonBufferAngle)) / (horizonBufferAngle * 2));
			setSky(pm4 * (1 - ratio) + pm8 * ratio);
		}else if(timeOfDay > 180 + horizonBufferAngle && timeOfDay <= 360 - horizonBufferAngle){ //night
			SunLight.GetComponent<Light>().intensity = 0.0f;
			MoonLight.GetComponent<Light>().intensity = moonMaxIntensity;
			if(timeOfDay <= 270){ //before midnight
				float ratio = ((timeOfDay - (180 + horizonBufferAngle)) / (90 - horizonBufferAngle));
				setSky(pm8 * (1 - ratio) + am12 * ratio);
			}else{ //after midnight
				float ratio = ((timeOfDay - 270) / (90 - horizonBufferAngle));
				setSky(am12 * (1 - ratio) + am4 * ratio);
			}
		}else{ //sunrise
			float ratio;
			if(timeOfDay <= 30){ //above horizon
				SunLight.GetComponent<Light>().intensity = sunMaxIntensity * (0.5f + (timeOfDay / (horizonBufferAngle * 2)));
				MoonLight.GetComponent<Light>().intensity = moonMaxIntensity * (0.5f - (timeOfDay / (horizonBufferAngle * 2)));
				ratio = (0.5f + (timeOfDay - 0) / (horizonBufferAngle * 2));
			}else{ //below horizon
				SunLight.GetComponent<Light>().intensity = sunMaxIntensity * ((timeOfDay - (360 - horizonBufferAngle)) / (horizonBufferAngle * 2));
				MoonLight.GetComponent<Light>().intensity = moonMaxIntensity * (1.0f - (timeOfDay - (360 - horizonBufferAngle)) / (horizonBufferAngle * 2));
				ratio = ((timeOfDay - (360 - horizonBufferAngle)) / (horizonBufferAngle * 2));
			}
			setSky(am4 * (1 - ratio) + am8 * ratio);
		}
	}
	
	
	//unused ... FOR NOW (dun dun DUUUUUUUUN)
	private void updateSky(){
		if(timeRemain == 0) return;
		timeRemain -=  Globals.time_scale;
		
		if(timeRemain <= 0){ //catch overshooting
			timeRemain = 0;
			setSky(skyGoal);
			return;
		}
		
		setSky(getSky() + skyDelta * Globals.time_scale);
	}
	
	//"time" is number of degrees it takes to finish changing to new sky
	//if time is 0, change sky immediately; time should not be less than 0
	void changeSkyByTime(Skybox s, float time){
		skyGoal = s;
		
		if(time <= 0){
			timeRemain = 0;
			setSky(skyGoal);
			return;
		}
		
		timeRemain = time * timeresPerDegree;
		skyDelta = s - getSky() / timeRemain;
	}

	private void setSky(Skybox s){
		RenderSettings.skybox.SetColor("_Color1", s.color1);
		RenderSettings.skybox.SetColor("_Color2", s.color2);
		RenderSettings.skybox.SetColor("_Color3", s.color3);
		RenderSettings.skybox.SetFloat("_Exponent1", s.exponent1);
		RenderSettings.skybox.SetFloat("_Exponent2", s.exponent2);
		RenderSettings.skybox.SetFloat("_Intensity", s.intensity);
	}
	
	private Skybox getSky(){
		return new Skybox(
			RenderSettings.skybox.GetColor("_Color1"),
			RenderSettings.skybox.GetColor("_Color2"),
			RenderSettings.skybox.GetColor("_Color3"),
			RenderSettings.skybox.GetFloat("_Exponent1"),
			RenderSettings.skybox.GetFloat("_Exponent2"),
			RenderSettings.skybox.GetFloat("_Intensity")
		);
	}
}

//time of day, seasons, weather, biome