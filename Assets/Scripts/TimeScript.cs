﻿using UnityEngine;
using System.Collections;

public class TimeScript : MonoBehaviour {
    public float initialWaitSpeed = 100;
    public float maxWaitSpeed = 657000;
    public AnimationCurve waitSpeedGrowth;
    public float timeToGetToMaxWait = 300; // 5 minutes
    public float sprintWaitMultiplier = 5;
    public float currentTimeReadOnly = 0;
    public float currentTimeScaleReadOnly = 1;
    public float ShaderTimeVar = 0;
    public float ShaderTimeVarMaxValue;

    private float waitingFor;
	
	// Update is called once per frame
	void Update () {
        //update time
        Globals.deltaTime = Globals.time_resolution * Globals.time_scale * Time.deltaTime;
        Globals.time += Globals.deltaTime;
        ShaderTimeVar += Globals.time_scale * Time.deltaTime;
        if(ShaderTimeVar > ShaderTimeVarMaxValue) ShaderTimeVar -= ShaderTimeVarMaxValue;
        currentTimeReadOnly = Globals.time / Globals.time_resolution;
        currentTimeScaleReadOnly = Globals.time_scale;
        Shader.SetGlobalFloat("_TimeVar", ShaderTimeVar);

        if(!StarEffect.isEffectPlaying && Input.GetButton("Patience") && Globals.mode == 0 && !Globals.MenusScript.GetComponent<CheatConsole>().isActive()) { //PATIENCE IS POWER
            if(waitingFor < timeToGetToMaxWait) Globals.time_scale = initialWaitSpeed + waitSpeedGrowth.Evaluate(waitingFor / timeToGetToMaxWait) * (maxWaitSpeed - initialWaitSpeed);
            else Globals.time_scale = maxWaitSpeed;
            if(Input.GetButton("Sprint")) waitingFor += Time.deltaTime * sprintWaitMultiplier;
            else waitingFor += Time.deltaTime;
        } else {
            Globals.time_scale = 1;
            waitingFor = 0;
        }

        if(Globals.mode == 1) { //paused
            Time.timeScale = 0;
            Globals.time_scale = 0;
        } else Time.timeScale = 1;
    }
}
