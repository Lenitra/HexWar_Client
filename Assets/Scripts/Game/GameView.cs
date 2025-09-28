using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameView : MonoBehaviour
{

    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI money; // Argent du joueur
    [SerializeField] private TextMeshProUGUI power; // Puissance du joueur
    [SerializeField] private TextMeshProUGUI playerName; // Nom du joueur


    // Durée des animations
    [SerializeField] private float animationDuration = 1f;

    [Header("Game effects")]
    [SerializeField] private GameObject sphereScanner; // Sphere scanner prefab
    [SerializeField] private GameObject dronePrefab; // Drone prefab
    [SerializeField] private LineRenderer prefabLine; // Prefab de la ligne de déplacement des drones






    public void SetMoney(int value)
    {
        StartCoroutine(AnimateMoney(value));
    }

    public void SetPower(int value)
    {
        StartCoroutine(AnimatePower(value));
    }

    public void SetPlayerName(string name)
    {
        StartCoroutine(AnimatePlayerName(name));
    }

    #region Animations des changements de valeurs HUD
    private IEnumerator AnimateMoney(int targetValue)
    {
        int currentMoney = int.Parse(money.text);
        int startValue = currentMoney;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / animationDuration;

            // Interpolation avec courbe d'ease-out pour un effet plus naturel
            progress = 1f - Mathf.Pow(1f - progress, 2f);

            int currentDisplayValue = Mathf.RoundToInt(Mathf.Lerp(startValue, targetValue, progress));
            money.text = currentDisplayValue.ToString();

            yield return null;
        }
        money.text = targetValue.ToString();
    }

    private IEnumerator AnimatePower(int targetValue)
    {
        int currentPower = int.Parse(power.text);
        int startValue = currentPower;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / animationDuration;

            // Interpolation avec courbe d'ease-out pour un effet plus naturel
            progress = 1f - Mathf.Pow(1f - progress, 2f);

            int currentDisplayValue = Mathf.RoundToInt(Mathf.Lerp(startValue, targetValue, progress));
            power.text = currentDisplayValue.ToString();

            yield return null;
        }
        power.text = targetValue.ToString();
    }



    private IEnumerator AnimatePlayerName(string targetName)
    {
        playerName.text = "";
        float delayPerCharacter = animationDuration / targetName.Length;

        for (int i = 0; i <= targetName.Length; i++)
        {
            playerName.text = targetName.Substring(0, i);
            yield return new WaitForSeconds(delayPerCharacter);
        }
    }
    #endregion





    #region Effets visuels de la grille de jeu



    #region Effets de scan
    public void ScanEffect(Tile point, bool expand = true)
    {
        StartCoroutine(BubleAnim(point, expand));
    }

    private IEnumerator BubleAnim(Tile tile, bool expand = true)
    {
        GameObject sphere = Instantiate(sphereScanner, tile.transform.position, Quaternion.identity);
        if (expand)
        {
            sphere.GetComponent<Scanner>().StartScanOutward();
        }
        else
        {
            sphere.GetComponent<Scanner>().StartScanInward();
        }
        yield return null;
    }

    #endregion




    #region Dessin d'une ligne de déplacement de drones
    public void DrawPathLine(Tile[] move)
    {
        StartCoroutine(AnimationMoveUnits(move));
    }


    private IEnumerator AnimationMoveUnits(Tile[] move)
    {
        yield return null;
    }


    #endregion



    #endregion



}
