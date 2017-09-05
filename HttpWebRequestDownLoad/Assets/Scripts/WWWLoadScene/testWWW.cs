using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System;
using UnityEngine.SceneManagement;

public class testWWW : MonoBehaviour {
    public Slider slider1;
    public Image fillImage;
    public Text tips;
    public Text path;
    public RawImage httpImage;
    public GameObject canves;

    private static string fileName1 = "ClickEffect.apk";
    private static string fileName2 = "DL_MU.apk";
    private static string fileName3 = "UnityCallAndroidApi.apk";
    private static string fileName4 = "1.jpeg";

    private static string LocalHost = "192.168.3.203";//"192.168.3.203"
    private static string RemoteIP = "180.173.46.38";
   

    public static float process;
    string savePath = string.Empty;
    private static string RemoteIP1 = @"http://www.aladdingame.online/wuzhang/Resources/ClickEffect.apk";
    private static string RemoteIP2 = @"http://www.aladdingame.online/wuzhang/Resources/UnityCallAndroidApi1.apk";//UnityCallAndroidApi1.apk   DL_MU.apk 1.jpeg
    private static string RemoteIP3 = @"http://www.aladdingame.online/wuzhang/Resources/1.jpeg";

    private void Awake()
    {
        //canves = GameObject.Find("Canvas");
        //slider1 = canves.transform.FindChild("Slider1").GetComponent<Slider>();
        //fillImage = canves.transform.FindChild("Slider1/Fill Area/Fill").GetComponent<Image>();
        //tips = canves.transform.FindChild("tips").GetComponent<Text>();
        //path = canves.transform.FindChild("path").GetComponent<Text>();
        //httpImage = canves.transform.FindChild("RawImage").GetComponent<RawImage>();
    }


    void Start()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        savePath = Application.streamingAssetsPath;
#elif UNITY_ANDROID
        savePath = Application.persistentDataPath;;
#endif
        WWWLoad wwwLoad = new WWWLoad();
        savePath = savePath + "/" + fileName4;
        //StartCoroutine(wwwLoad.DownFile(RemoteIP1, savePath, ShowProcess));//clickEffect
        StartCoroutine(wwwLoad.DownFile(RemoteIP3, savePath, ShowProcess));

    }


    void ShowProcess(WWW www)
    {
        slider1.value = www.progress;
        if (1 == www.progress)
        {
            tips.text = string.Format("下载完成");
            path.text = "Path:"+savePath;
            httpImage.texture = www.texture;
        }
        else
        {
            fillImage.color = new Color(1 - www.progress, www.progress, 1- www.progress, 1f);
            tips.text = string.Format("下载进度:{0:F}%", www.progress * 100.0);
            path.text = "";
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("Menu");
        }
    }

    private void OnDestroy()
    {
    }


}
