using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Core;

public class LoginController : MonoBehaviour
{
    public event Action<PlayerProfile> OnSignedIn;
    public PlayerProfile PlayerProfile => playerProfile;

    private PlayerProfile playerProfile = new PlayerProfile();
    private PlayerInfo playerInfo;

    private string idToken;

    private async void Awake()
    {
        await UnityServices.InitializeAsync();
        PlayerAccountService.Instance.SignedIn += SignedIn;
    }


    private async void SignedIn()
    {
        try
        {
            string sessionToken = PlayerAccountService.Instance.AccessToken;

            await SignInWithUnityAsync(sessionToken);

            // Une fois connecté, on récupère le vrai JWT
            idToken = AuthenticationService.Instance.AccessToken;
            Debug.Log("Sending idToken to backend: " + idToken);

            string backendUrl = DataManager.Instance.GetData("serverIP") + "/api/auth/unity-idtoken";
            await SendIdTokenToBackend(backendUrl);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error during sign-in: " + ex.Message);
        }
    }

    public async Task InitSignIn()
    {
        await PlayerAccountService.Instance.StartSignInAsync();
    }

    private async Task SignInWithUnityAsync(string token)
    {
        await AuthenticationService.Instance.SignInWithUnityAsync(token);
        playerInfo = AuthenticationService.Instance.PlayerInfo;
        var name = await AuthenticationService.Instance.GetPlayerNameAsync();

        playerProfile.playerInfo = playerInfo;
        playerProfile.Name = name;

        OnSignedIn?.Invoke(playerProfile);
    }

    public async Task SendIdTokenToBackend(string url)
    {
        var payload = JsonUtility.ToJson(new IdTokenPayload { idToken = idToken });

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(payload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("idToken validated by backend. Response: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Backend token validation failed: " + request.error);
            }
        }
    }

    [Serializable]
    public class IdTokenPayload
    {
        public string idToken;
    }
}

[Serializable]
public struct PlayerProfile
{
    public PlayerInfo playerInfo;
    public string Name;
}
