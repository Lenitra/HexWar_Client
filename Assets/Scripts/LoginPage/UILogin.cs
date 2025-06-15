using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UILogin : MonoBehaviour
{
    [SerializeField] private Button loginButton;
    [SerializeField] private GameObject connectingPanel;
    [SerializeField] private LoginController loginController;

    private PlayerProfile playerProfile;

    private void OnEnable()
    {
        loginButton.onClick.AddListener(LoginButtonPressed);
        loginController.OnSignedInUnity += LoginController_OnSignedInUnity;
        loginController.OnSignedInBackend += LoginController_OnSignedInBackend;
        loginController.OnConnectingError += (error) =>
        {
            connectingPanel.SetActive(false);
            Debug.LogError($"Login error: {error}");
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

    private void LoginController_OnSignedInUnity(PlayerProfile profile)
    {
        connectingPanel.SetActive(true);
    }


    private void LoginController_OnSignedInBackend(PlayerProfile profile)
    {
        connectingPanel.SetActive(false);
        string msg = $"Welcome {profile.Name}!";
        msg += $"\nPlayer ID: {profile.playerInfo.Id}";
        Debug.Log(msg);
        SceneManager.LoadScene("Game");
    }



}