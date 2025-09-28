using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameView : MonoBehaviour
{
    
    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI money; // Argent du joueur   
    [SerializeField] private TextMeshProUGUI power; // Puissance du joueur
    [SerializeField] private TextMeshProUGUI playerName; // Nom du joueur

    [Header("Game effects")]
    [SerializeField] private GameObject sphereScanner; // Sphere scanner prefab
    [SerializeField] private GameObject dronePrefab; // Drone prefab
    [SerializeField] private LineRenderer prefabLine; // Prefab de la ligne de déplacement des drones





    public void SetMoney(int value)
    {
        money.text = value.ToString();
    }

    public void SetPower(int value)
    {
        power.text = value.ToString();
    }

    public void SetPlayerName(string name)
    {
        playerName.text = name;
    }



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
