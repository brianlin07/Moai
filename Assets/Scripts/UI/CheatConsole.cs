﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CheatConsole : MonoBehaviour
{

    public InputField console;

    private GameObject airStar;
    private GameObject earthStar;
    private GameObject fireStar;
    private GameObject waterStar;

    private GameObject puzzleFinisher;

    private bool active = true;
    void Start()
    {
        airStar = Resources.Load("Prefabs/Stars/star_air") as GameObject;
        earthStar = Resources.Load("Prefabs/Stars/star_earth") as GameObject;
        fireStar = Resources.Load("Prefabs/Stars/star_fire") as GameObject;
        waterStar = Resources.Load("Prefabs/Stars/star_water") as GameObject;
        puzzleFinisher = Resources.Load("Prefabs/puzzleFinisher") as GameObject;
        deactivateConsole();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            toggleConsole();
        }
        console.gameObject.SetActive(active);
    }

    void toggleConsole() {
        active = !active;

        if(!active) {
            console.DeactivateInputField();
        }
        console.gameObject.SetActive(active);
        if(active) {
            console.ActivateInputField();
        }
        console.text = "";
    }

    void deactivateConsole() {
        active = false;
        console.DeactivateInputField();
        console.gameObject.SetActive(false);
        console.text = "";
    }

    void parseCommand()
    {
        string text = console.text;

        if (text.Equals("stars"))
        {
            Debug.Log("Adding one of each elemental star");
            addStars();
        }
        else if (text.Equals("finish"))
        {
            Debug.Log("Creating puzzle finisher");
            finishShrine();
        }
        else
        {
            Debug.Log("Unrecognized Command");
        }

        toggleConsole();

    }

    // Cheats

    void addStars()
    {
        GameObject aStar = Instantiate(airStar, Globals.Player.transform.position, Quaternion.identity) as GameObject;
        GameObject eStar = Instantiate(earthStar, Globals.Player.transform.position, Quaternion.identity) as GameObject;
        GameObject fStar = Instantiate(fireStar, Globals.Player.transform.position, Quaternion.identity) as GameObject;
        GameObject wStar = Instantiate(waterStar, Globals.Player.transform.position, Quaternion.identity) as GameObject;

        Vector3 target = Globals.Player.transform.position + Vector3.up * 10000;

        aStar.GetComponent<StarEffect>().setTarget(target);
        eStar.GetComponent<StarEffect>().setTarget(target);
        fStar.GetComponent<StarEffect>().setTarget(target);
        wStar.GetComponent<StarEffect>().setTarget(target);
        Globals.airStars.Add(aStar);
        Globals.earthStars.Add(eStar);
        Globals.fireStars.Add(fStar);
        Globals.waterStars.Add(wStar);
    }

    // Create a shrine finisher at player
    void finishShrine()
    {
        Instantiate(puzzleFinisher, Globals.Player.transform.position, Quaternion.identity);
    }
}