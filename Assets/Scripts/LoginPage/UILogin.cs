using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UILogin : MonoBehaviour
{
    [SerializeField] private Button loginButton;
    [SerializeField] private GameObject connectingPanel;
    [SerializeField] private LoginController loginController;


    private void OnEnable()
    {
        loginButton.onClick.AddListener(LoginButtonPressed);
        loginController.OnSignedInUnity += LoginController_OnSignedInUnity;
        loginController.OnSignedInBackend += LoginController_OnSignedInBackend;
        loginController.OnConnectingError += () =>
        {
            connectingPanel.SetActive(false);
            Debug.LogError($"Login error resetting login controller");
        };
    }

    private void OnDisable()
    {
        loginButton.onClick.RemoveListener(LoginButtonPressed);
        loginController.OnSignedInUnity -= LoginController_OnSignedInUnity;
        loginController.OnSignedInBackend -= LoginController_OnSignedInBackend;
    }

    private async void LoginButtonPressed()
    {
        await loginController.InitSignIn();
    }

    private void LoginController_OnSignedInUnity()
    {
        connectingPanel.SetActive(true);
    }


    private void LoginController_OnSignedInBackend()
    {
        connectingPanel.SetActive(false);
        string msg = $"Welcome {PlayerPrefs.GetString("username")}!";
        msg += $"\nPlayer ID: {PlayerPrefs.GetString("user_id")}";
        msg += $"\nAuth Token: {PlayerPrefs.GetString("auth_token")}";
        Debug.Log(msg);
        SceneManager.LoadScene("Game");
    }



}