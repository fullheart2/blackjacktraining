using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    // static settings
    public static int[] settings = new int[4];

    // UI elements
    public Button startBtn;
    public Toggle countMode;
    public Dropdown decks;
    public Dropdown players;
    public Toggle deviations;



    // Start is called before the first frame update
    void Start()
    {
        startBtn.onClick.AddListener(() => StartGame());
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void StartGame()
    {
        SetSettings();
        SceneManager.LoadScene("Game");
    }
    public static int[] GetSettings() 
    {
        return settings;
    }

    public void SetSettings()
    {
        if (!countMode.isOn) settings[0] = 0; else settings[0] = 1;
        settings[1] = decks.value*2 + 4;
        settings[2] = 1;
        // settings[2] = players.value + 1;
        if (!deviations.isOn) settings[3] = 0; else settings[3] = 1;
    }
        
}
