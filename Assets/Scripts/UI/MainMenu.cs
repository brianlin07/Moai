﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour {
    public List<MMBackground> titleScreens;
    public int loadingScreen;
    public UnityEngine.UI.Image loadingBackdrop;
    public UnityEngine.UI.Image loadingIcon;
    public List<Sprite> loadingWallpapers;
    public RectTransform title;
    public RectTransform buttonsParent;

    private MMBackground mmback;
    private UnityStandardAssets.Characters.FirstPerson.FirstPersonController firstPersonCont;
    private int loadStep = -1;
    private Vector3 origCamPos;
    private Vector3 origCamRot;
    private GameObject scene;

    void Awake() {
        firstPersonCont = Globals.Player.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>();
        Globals.settings["Screenmode"] = (Screen.fullScreen ? 1 : 0);
    }

    // Update is called once per frame
    void Start () {
        if(Globals.mode != -1 && loadStep == -1) return;
        setupMain();
    }

    void Update() {
        if(Globals.mode != -1 && loadStep == -1) return;

        if(loadStep == 0) {
            prepLoad();
            loadStep = 1;
            StartCoroutine(loadingDelay());
            return;
        }
        else if(loadStep == 1)
        {
            float rot = Mathf.PingPong(Time.time,2) - 1;
            loadingIcon.rectTransform.localScale = new Vector3(rot,
                loadingIcon.rectTransform.localScale.y, loadingIcon.rectTransform.localScale.z);
        }
        else if(loadStep == 2) {
            loadGame();
            loadStep = -1;
            return;
        }
        
        Camera.main.transform.position = mmback.cameraPosition;
        Camera.main.transform.eulerAngles = mmback.cameraRotation;
        firstPersonCont.lookLock = true;
        firstPersonCont.getMouseLook().SetCursorLock(false);
        title.anchoredPosition = new Vector2(mmback.titlePosition.x * Screen.width, mmback.titlePosition.y * Screen.height) / 2;
        float aspectRatio = title.sizeDelta.x / title.sizeDelta.y;
        title.sizeDelta = new Vector2(Screen.height * mmback.titleScale * aspectRatio, Screen.height * mmback.titleScale);
        buttonsParent.anchoredPosition = new Vector2(mmback.buttonsPosition.x * Screen.width, mmback.buttonsPosition.y * Screen.height) / 2;
    }


    public void startGame() {
        loadStep = 0;
    }

    private IEnumerator loadingDelay()
    {
        Globals.mode = 0;
        Random.seed = Globals.SeedScript.randomizeSeed();
        yield return new WaitForSeconds(8f);
        loadStep = 2;
    }

    private void prepLoad() {
        Globals.MenusScript.switchTo(loadingScreen);
        Random.seed = (int)System.DateTime.Now.Ticks;
        loadingBackdrop.sprite = loadingWallpapers[Random.Range(0, loadingWallpapers.Count)];
        Globals.GenerationManagerScript.initiateWorld();
        Globals.WeatherManagerScript.initializeWeather();
        //Camera.main.GetComponent<MusicManager>().Stop(false);
        //AudioListener.volume = 0;
    }

    private void loadGame() {
        
        if(scene) Destroy(scene);

        Globals.MenusScript.switchTo(-1);
        //Camera.main.GetComponent<MusicManager>().Play();
        //AudioListener.volume = 1;

        Camera.main.transform.localPosition = origCamPos;
        Camera.main.transform.localEulerAngles = origCamRot;
        firstPersonCont.enabled = true;
        firstPersonCont.lookLock = false;
        firstPersonCont.getMouseLook().SetCursorLock(true);
        Globals.PlayerScript.warpToGround(3000, true);
    }

    public void setupMain() {
        Random.seed = (int)System.DateTime.Now.Ticks;
        setupMain(Random.Range(0, titleScreens.Count));
    }

    public void setupMain(int background) {
        mmback = titleScreens[background];

        scene = Instantiate(mmback.gameObject);
        Globals.cur_biome = mmback.biome;
        Globals.time = mmback.time * Globals.time_resolution;

        firstPersonCont.enabled = false;
        firstPersonCont.lookLock = true;
        origCamPos = Camera.main.transform.localPosition;
        origCamRot = Camera.main.transform.localEulerAngles;
        Camera.main.transform.position = mmback.cameraPosition;
        Camera.main.transform.eulerAngles = mmback.cameraRotation;
        firstPersonCont.getMouseLook().SetCursorLock(false);
    }
}
