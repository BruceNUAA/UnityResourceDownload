using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class TestScene : MonoBehaviour
{
    public Button btn_login;
    public Button btn_register;
    public InputField input_name;
    public InputField input_passwd;
    public Text tips;

    private void Start()
    {
        btn_login.onClick.AddListener(OnLoginClick);
        btn_register.onClick.AddListener(OnRegisterClick);

    }

    private void OnLoginClick()
    {
        if (input_name.text == "" || input_passwd.text == "")
            return;
        Debug.Log("服务器校验信息中...");
        StartCoroutine(ShowTips("服务器校验信息中"));
    }

    private void OnRegisterClick()
    {
        if (input_name.text == "" || input_passwd.text == "")
            return;
        Debug.Log("注册信息中...");
        StartCoroutine(ShowTips("注册信息中"));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("Menu");
        }
    }
    IEnumerator ShowTips(string tip)
    {
        tips.text = tip;
        yield return new WaitForSeconds(2f);
        tips.text = "";
    }
}
