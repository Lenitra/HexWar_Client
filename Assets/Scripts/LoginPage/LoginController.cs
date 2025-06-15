using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Core;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoginController : MonoBehaviour
{
    public event Action<PlayerProfile> OnSignedInUnity;
    public event Action<PlayerProfile> OnSignedInBackend;
    public event Action<string> OnConnectingError;
    public PlayerProfile PlayerProfile => playerProfile;

    private PlayerProfile playerProfile = new PlayerProfile();
    private PlayerInfo playerInfo;

    private string idToken;

    private async void Awake()
    {
        await UnityServices.InitializeAsync();
        PlayerAccountService.Instance.SignedIn += SignedIn;
    }


    // Valide la connexion avec le service d'authentification de Unity
    private async void SignedIn()
    {
        try
        {
            // Connexion avec le service d'authentification de Unity
            string sessionToken = PlayerAccountService.Instance.AccessToken;
            await SignInWithUnityAsync(sessionToken);
            SignedInUnity(playerProfile);

            // Une fois connecté, on récupère le token du backend à patir du token d'authentification de Unity
            idToken = AuthenticationService.Instance.AccessToken;

            // Envoi du token d'authentification de Unity au backend pour obtenir un token backend
            string backendUrl = DataManager.Instance.GetData("serverIP") + "/api/auth/unity-idtoken";
            await SendIdTokenToBackend(backendUrl);

        }
        catch (Exception ex)
        {
            Debug.LogError("Error during sign-in: " + ex.Message);
            ConnectingError($"Error during sign-in: {ex.Message}");
        }
    }

    // Lien entre le token d'authentification de Unity et le backend pour récupérer un token backend
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
                string token = request.downloadHandler.text;
                token = token.Trim(); // enlève les espaces, \n, etc.
                token = token.Replace("\"", ""); // enlève des guillemets éventuels


                Debug.Log("idToken validated by backend. Response: " + token);
                PlayerPrefs.SetString("auth_token", token);
                PlayerPrefs.Save();
                SignedInBackend(playerProfile);
            }
            else
            {
                ConnectingError($"Error sending idToken to backend: {request.error}");
                Debug.LogError($"Error sending idToken to backend: {request.error}");
            }
        }
    }


    private void ConnectingError(string error)
    {
        AuthenticationService.Instance.SignOut();
        PlayerPrefs.DeleteKey("auth_token");
        PlayerPrefs.Save();
    }

    private void SignedInUnity(PlayerProfile profile)
    {
        playerProfile = profile;
        OnSignedInUnity?.Invoke(playerProfile);
    }

    private void SignedInBackend(PlayerProfile profile)
    {
        playerProfile = profile;
        OnSignedInBackend?.Invoke(playerProfile);
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

        OnSignedInUnity?.Invoke(playerProfile);
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
