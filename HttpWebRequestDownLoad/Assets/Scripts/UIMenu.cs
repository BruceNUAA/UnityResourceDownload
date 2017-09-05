using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class UIMenu : MonoBehaviour
{
    public Button btn_www;
    public Button btn_http;

    // Use this for initialization
    void Start()
    {
        btn_www.onClick.AddListener(OnWWWClick);
        btn_http.onClick.AddListener(OnHttpClick);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    private void OnWWWClick()
    {
        SceneManager.LoadScene("WWWDownLoad");
    }

    private void OnHttpClick()
    {
        SceneManager.LoadScene("HttpDownLoad");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }


}
