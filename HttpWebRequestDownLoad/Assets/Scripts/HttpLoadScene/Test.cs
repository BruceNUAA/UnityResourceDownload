using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Net;
using System.IO;

public class Test : MonoBehaviour
{
    bool isDone;
    Slider slider1;
    public Image fill1;
    Slider slider2;
    public Image fill2;

    Slider slider3;
    public Image fill3;

    Text text1;
    Text text2;
    Text text3;

    Text path;

    HttpDownLoad http1;
    HttpDownLoad http2;
    HttpDownLoad http3;

    bool http1Finish = false;
    bool http2Finish = false;
    bool http3Finish = false;


    public string ApplicationStreamingPath = "";


    void Awake()
    {
        slider1 = GameObject.Find("Slider1").GetComponent<Slider>();
        fill1 = GameObject.Find("Slider1/Fill Area/Fill").GetComponent<Image>();
        slider2 = GameObject.Find("Slider2").GetComponent<Slider>();
        fill2 = GameObject.Find("Slider2/Fill Area/Fill").GetComponent<Image>();
        slider3 = GameObject.Find("Slider3").GetComponent<Slider>();
        fill3 = GameObject.Find("Slider3/Fill Area/Fill").GetComponent<Image>();

        text1 = GameObject.Find("Text1").GetComponent<Text>();
        text2 = GameObject.Find("Text2").GetComponent<Text>();
        text3 = GameObject.Find("Text3").GetComponent<Text>();

        path = GameObject.Find("Path").GetComponent<Text>();
        //不同平台下StreamingAssets的路径是不同的，这里需要注意一下。  
        ApplicationStreamingPath =
#if UNITY_ANDROID   //安卓  
            "jar:file://" + Application.dataPath + "!/assets/";
#elif UNITY_IPHONE  //iPhone  
            Application.dataPath + "/Raw/";  
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR  //windows平台和web平台  
            "file://" + Application.dataPath + "/StreamingAssets/";  
#else
                string.Empty;  
#endif

    }

    //private static string fileName1 = "DL_MU.apk";
    private static string fileName1 = "ClickEffect.apk";
    private static string fileName2 = "UnityCallAndroidApi.apk";
    private static string fileName3 = "test.a";



    private static string RemoteIP = "http://www.aladdingame.online/wuzhang/Resources/";

    string savePath = string.Empty;
    private static string RemoteIP1 = @"http://www.aladdingame.online/wuzhang/Resources/ClickEffect.apk";
    private static string RemoteIP2 = @"http://www.aladdingame.online/wuzhang/Resources/UnityCallAndroidApi1.apk";//UnityCallAndroidApi1.apk   DL_MU.apk
    private static string RemoteIP3 = @"http://www.aladdingame.online/wuzhang/Resources/AssetBundle/test.a";

    void Start ()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        savePath = Application.persistentDataPath;
#elif UNITY_ANDROID
        savePath = Application.persistentDataPath;
#endif
        //开启两个http进行文件下载
        http1 = new HttpDownLoad();
        http1.DownLoad(RemoteIP1, savePath, fileName1, http1DownLoadState);

        http2 = new HttpDownLoad();
        http2.DownLoad(RemoteIP2, savePath, fileName2, http2DownLoadState);

        http3 = new HttpDownLoad();//下载bundle并LoadScene
        http3.DownLoad(RemoteIP3,savePath, fileName3, http3DownLoadState, System.Threading.ThreadPriority.Highest);//BUndle下载优先级最高

    }

	void OnDisable()
	{
		print ("OnDisable");
		http1.Close();
        http2.Close();
        http3.Close();
	}

    void http1DownLoadState()
    {
        http1Finish = true;
        Debug.Log("http1Finish:"+http1Finish);
    }

    void http2DownLoadState()
    {
        http2Finish = true;
    }

    void http3DownLoadState()
    {
        http3Finish = true;
        LoadLevel();
    }


    void LoadLevel()
	{
		isDone = true;
	}
    string url = "";
	void Update()
	{
        ShowProgress();
        //下载完成，加载Bundle场景
        if (isDone && http1.isDone && http2.isDone)
        {
            isDone = false;

            //string url = "file:///"+ savePath + "/test.a";//load名字为test的bundle
            url=
#if UNITY_ANDROID   //安卓  
            "jar:file://" + Application.persistentDataPath + "/test.a";
#elif UNITY_IPHONE  //iPhone  
            Application.dataPath + "/Raw"+ "/test.a";  
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR  //windows平台和web平台  
            "file:///" +Application.persistentDataPath+ "/test.a";  
#else
                string.Empty;  
#endif

            StartCoroutine(LoadScene(url));
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("Menu");
        }
    }

    /// <summary>
    /// 显示进度条
    /// </summary>
    void ShowProgress()
    {
        if (http1Finish)
        {
            slider1.value = 1;
            fill1.color = Color.green;
            text1.text = string.Format("{0}:<color=#0000ff>{1}</color>", fileName1, savePath + "/" + fileName1);
        }

        if (http2Finish)
        {
            slider2.value = 1;
            fill2.color = Color.green;
            text2.text = string.Format("{0}:<color=#0000ff>{1}</color>", fileName2, savePath + "/" + fileName2);
        }

        if (http3Finish)
        {
            slider3.value = 1;
            fill3.color = Color.green;
            text3.text = string.Format("{0}:<color=#0000ff>{1}</color>", fileName3, savePath + "/" + fileName3);
        }

        if (!http1.isDone)
        {
            slider1.value = http1.progress;
            fill1.color = new Color(0, http1.progress, 1 - http1.progress, http1.progress + 0.1f);
            text1.text = "DownLoading:\n" + fileName1 + (slider1.value * 100).ToString(" 0.00") + "%";
        }
        else
        {
            slider1.value = 1;
            text1.text = string.Format("{0}:<color=#0000ff>{1}</color>", fileName1, savePath + "/" + fileName1);
            path.text = "ResourcePath:" + savePath;
        }

        if (!http2.isDone)
        {
            slider2.value = http2.progress;
            fill2.color = new Color(0, http2.progress, 1 - http2.progress, http2.progress + 0.1f);
            text2.text = "DownLoading:\n" + fileName2 + (slider2.value * 100).ToString(" 0.00") + "%";
        }
        else
        {
            slider2.value = 1;
            text2.text = string.Format("{0}:<color=#0000ff>{1}</color>", fileName2, savePath + "/" + fileName2);
        }

        if (!http3.isDone)
        {
            slider3.value = http3.progress;
            fill3.color = new Color(0, http3.progress, 1 - http3.progress, http3.progress + 0.1f);
            text3.text = "DownLoading:\n" + fileName3 + (slider3.value * 100).ToString(" 0.00") + "%";
        }
        else
        {
            slider3.value = 1;
            text3.text =string.Format("AssetBundle:<color=#0000ff>{0}</color>", savePath+"/"+fileName3);
        }
    }

    /// <summary>
    /// load Scene
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    IEnumerator LoadScene(string url)
	{
        Debug.Log(url);
		WWW www = new WWW(url);
        while (!www.isDone)
        {
		    yield return www;
        }

		AssetBundle ab = www.assetBundle;
		SceneManager.LoadScene("testScene");

        // 中断正在加载过程中的WWW
        www.Dispose();
        //www.Dispose();
        isDone = false;
	}

    private void OnDestroy()
    {
        http1 = null;
        http2 = null;
    }
}
