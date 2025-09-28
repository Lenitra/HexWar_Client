using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class LoginController : MonoBehaviour
{
    public event Action OnSignedInUnity;
    public event Action OnSignedInBackend;
    public event Action OnConnectingError;
    public PlayerProfile PlayerProfile => playerProfile;

    private PlayerProfile playerProfile = new PlayerProfile();
    private PlayerInfo playerInfo;

    private string idToken;

    private async void Start()
    {
        AutoConnect();
        await UnityServices.InitializeAsync();
        PlayerAccountService.Instance.SignedIn += SignedIn;

        OnConnectingError += () =>
        {
            PlayerAccountService.Instance.SignedIn -= SignedIn;
            PlayerPrefs.DeleteKey("auth_token");
            PlayerPrefs.Save();
            SceneManager.LoadScene("Login");
        };
    }


    #region Connexion basique avec unity


    // Valide la connexion avec le service d'authentification de Unity
    private async void SignedIn()
    {
        try
        {
            // Connexion avec le service d'authentification de Unity
            string sessionToken = PlayerAccountService.Instance.AccessToken;
            await SignInWithUnityAsync(sessionToken);
            SignedInUnity();

            // Une fois connecté, on récupère le token du backend à patir du token d'authentification de Unity
            idToken = AuthenticationService.Instance.AccessToken;

            // Envoi du token d'authentification de Unity au backend pour obtenir un token backend
            string backendUrl = DataManager.Instance.GetData("serverIP") + "/auth/unity-connexion";
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
                SaveJWTInPlayerPrefs(request.downloadHandler.text);
                SignedInBackend();
            }
            else
            {
                ConnectingError($"Error sending idToken to backend: {request.error}");
                Debug.LogError($"Error sending idToken to backend: {request.error}");
            }
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

        OnSignedInUnity?.Invoke();
    }



    [Serializable]
    public class IdTokenPayload
    {
        public string idToken;
    }

    #endregion














    #region Connexion automatique avec le token du backend
    private void AutoConnect()
    {
        // Vérifie si le joueur est déjà connecté
        if (CheckAlreadyConnectedInfos())
        {
            Debug.Log("Le joueur est déjà connecté avec un token d'authentification, on tente de se connecter automatiquement.");
            SignedInUnity();
            // Si oui, on se connecte avec le token du backend
            CheckAndRefreshToken();
        }
    }


    private bool CheckAlreadyConnectedInfos()
    {
        // Vérifie si les PlayerPrefs contiennent un token d'authentification, l'id et le username
        // et si le token est valide
        if (!PlayerPrefs.HasKey("auth_token") ||
            string.IsNullOrEmpty(PlayerPrefs.GetString("auth_token")) ||
            !PlayerPrefs.HasKey("user_id") ||
            string.IsNullOrEmpty(PlayerPrefs.GetString("user_id")) ||
            !PlayerPrefs.HasKey("username") ||
            string.IsNullOrEmpty(PlayerPrefs.GetString("username")))
        {
            Debug.Log("Aucun token d'authentification trouvé, le joueur n'est pas connecté.");
            return false;
        }

        return true;

    }

    private async void CheckAndRefreshToken()
    {
        try
        {
            string backendUrl = DataManager.Instance.GetData("serverIP") + "/auth/token-refresh";
            UnityWebRequest request = UnityWebRequest.Get(backendUrl);

            request.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("auth_token"));
            request.SetRequestHeader("Content-Type", "application/json");

            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                SaveJWTInPlayerPrefs(request.downloadHandler.text);
                SignedInBackend();
            }
            else
            {
                Debug.LogError($"Error during token connexion: {request.error}");
                ConnectingError($"Error during token connexion: {request.error}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error during sign-in: " + ex.Message);
            ConnectingError($"Error during sign-in: {ex.Message}");
        }
    }


    #endregion




    // return JSONResponse(content=backend_token, status_code=200)
    private void SaveJWTInPlayerPrefs(string response)
    {
        Debug.Log("Response from server authentication: " + response);
        // Enleve les guillemets autour du token si présents
        response = response.Trim('"');

        // Enregistre les données dans PlayerPrefs
        PlayerPrefs.SetString("auth_token", response);
        PlayerPrefs.Save();
    }




    private void ConnectingError(string error)
    {
        AuthenticationService.Instance.SignOut();
        PlayerPrefs.DeleteKey("auth_token");
        PlayerPrefs.Save();
    }

    private void SignedInUnity()
    {
        OnSignedInUnity?.Invoke();
    }

    private void SignedInBackend()
    {
        OnSignedInBackend?.Invoke();
    }







}

[Serializable]
public struct PlayerProfile
{
    public PlayerInfo playerInfo;
    public string Name;
}
