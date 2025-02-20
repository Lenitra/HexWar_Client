using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;



public class Tile : MonoBehaviour
{
    [Header("Tile Attributes")]
    private int x = int.MaxValue;
    private int y = int.MaxValue;
    [SerializeField] private int units = 0;
    [SerializeField] private string owner = "";
    [SerializeField] private int lvl = 0;
    [SerializeField] private string type = "";
    [SerializeField] private string color = "0|0|0";

    [Header("Prefabs des types de tiles")]
    [SerializeField] private GameObject[] typesPrefabs;


    [Header("Elements enfants")]
    [SerializeField] private MeshRenderer meshDeRendu;
    [SerializeField] private GameObject glow;



    #region Getters & Setters
    public int X
    {
        get { return x; }
        set { x = value; }
    }

    public int Y
    {
        get { return y; }
        set { y = value; }
    }

    public int Units
    {
        get { return units; }
        set { units = value; }
    }

    public string Owner
    {
        get { return owner; }
        set { owner = value; }
    }

    public int Lvl
    {
        get { return lvl; }
        set { lvl = value; }
    }

    public string Type
    {
        get { return type; }
        set
        {
            type = value;
            SetupType();
        }
    }

    public string Color
    {
        get { return color; }
        set
        {
            color = value;
            SetupColor();
        }
    }

    #endregion




    public void SetupTile(Hex hexData)
    {
        if (this.X == int.MaxValue || this.Y == int.MaxValue)
        {
            this.X = hexData.x;
            this.Y = hexData.y;
            this.Lvl = hexData.lvl;
            this.Units = hexData.units;
            this.Owner = hexData.owner;
            this.Type = hexData.type;
            this.Color = hexData.color;
            return;
        }

        if (this.X != hexData.x || this.Y != hexData.y)
        {
            Debug.LogError("Erreur: Tentative de modification d'une tile avec des coordonnées différentes");
            return;
        }

        if (this.Lvl != hexData.lvl)
        {
            this.Lvl = hexData.lvl;
        }
        if (this.Units != hexData.units)
        {
            this.Units = hexData.units;
        }
        if (this.Owner != hexData.owner)
        {
            this.Owner = hexData.owner;
        }
        if (this.Type != hexData.type)
        {
            this.Type = hexData.type;
        }
        if (this.Color != hexData.color)
        {
            this.Color = hexData.color;
        }
    }



    // Défini la couleur du mesh de rendu du dessus de l'objet
    // Appellé dans le setter de la couleur
    private void SetupColor()
    {
        string[] rgb = this.Color.Split('|');
        Color color = new Color(float.Parse(rgb[0]) / 255, float.Parse(rgb[1]) / 255, float.Parse(rgb[2]) / 255);
        meshDeRendu.material.SetColor("_Color", color);
        meshDeRendu.material.SetColor("_EmissionColor", color);
        // intencité de l'émission
        meshDeRendu.material.SetFloat("_EmissionIntensity", 0.5f);

        // Actualise le material
        meshDeRendu.material.EnableKeyword("_EMISSION");
    }


    private void SetupType()
    {
        int index = -1;
        switch (this.type.ToLower())
        {
            case "hq":
                index = 0;
                break;
            case "miner":
                index = 1;
                break;
            case "barrack":
                index = 2;
                break;
            case "radar":
                index = 3;
                break;
            case "node":
                index = 4;
                break;
            default:
                return;
        }

        // instancier le prefab avec l'index
        if (index != -1)
        {
            GameObject typePrefab = Instantiate(typesPrefabs[index], this.transform);
            typePrefab.transform.position = this.transform.position;
        }

    }


    public void HighlightTile()
    {
        glow.SetActive(true);
    }

    public void UnHighlightTile()
    {
        glow.SetActive(false);
    }


}
