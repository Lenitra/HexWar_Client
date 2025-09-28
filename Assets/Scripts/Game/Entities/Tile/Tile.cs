using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



[Serializable]
public class Tile : MonoBehaviour
{


    [Header("Data from server")]

    [SerializeField] private int id = int.MinValue;
    [SerializeField] private int user_id = int.MinValue;
    [SerializeField] private int x = int.MinValue;
    [SerializeField] private int y = int.MinValue;
    [SerializeField] private int lvl = 0;
    [SerializeField] private string build = "";
    [SerializeField] private int drone = 0;
    [SerializeField] private DateTime shield = DateTime.MinValue;






    [Header("Attributs calculés")]
    [SerializeField] private string color = "";
    [SerializeField] private string owner = "";

    [SerializeField] private bool hasShield = false;


    [Header("Prefabs des builds")]
    [SerializeField] private GameObject[] buildPrefabs;



    [Header("Elements enfants")]
    [SerializeField] private MeshRenderer meshDeRenduObject;
    [SerializeField] private GameObject borderParentObject;
    [SerializeField] private GameObject shieldObject;
    [SerializeField] private LineRenderer nodeRadiusObject;



    private GameObject building;
    private readonly float gridGap = 0.05f;
    private readonly float selectElevation = 1f;



    // Getters
    public int GetId() { return id; }
    public int GetUserId() { return user_id; }
    public int GetX() { return x; }
    public int GetY() { return y; }
    public int GetLevel() { return lvl; }
    public string GetBuild() { return build; }
    public int GetDrone() { return drone; }
    public DateTime GetShield() { return shield; }
    public string GetColor() { return color; }
    public string GetOwner() { return owner; }


    // Setters

    public void SetOwner(string newOwner)
    {
        owner = newOwner;
    }

    public void SetBuild(string newBuild)
    {
        build = newBuild;
    }



    public void SetShield(DateTime newShield)
    {
        if (newShield != shield)
        {
            shield = newShield;
        }

        if (shield < DateTime.Now)
        {
            hasShield = false;
            shieldObject.SetActive(false);
        }

        else if (newShield > DateTime.Now)
        {
            hasShield = true;
            shieldObject.SetActive(true);
        }
        shield = newShield;
    }


    public void SetColor(string newColor)
    {
        color = newColor;
        // Appliquer la couleur à la tile
        if (ColorUtility.TryParseHtmlString(color, out Color unityColor))
        {
            meshDeRenduObject.material.color = unityColor;
        }
        else
        {
            Debug.LogWarning("Invalid color format for tile ID " + id + ": " + color);
        }
    }

    public void SetUserData(User user)
    {
        if (user == null)
        {
            if (user_id != int.MinValue) user_id = int.MinValue;
            if (owner != "") owner = "";
            if (color != "#000000") SetColor("#000000");
            return;
        }

        if (user_id != user.id) user_id = user.id;
        if (color != user.color) SetColor(user.color);
        if (owner != user.username) owner = user.username;
    }


    public void SetData(TileApi tileApi)
    {
        if (tileApi == null) return;

        // Définir la position seulement lors de la première initialisation
        bool isFirstInit = this.id == int.MinValue;
        if (isFirstInit)
        {
            transform.position = new Vector3(tileApi.x * (1 + gridGap), 0, tileApi.y * (1 + gridGap));
            // Valeurs fixes 
            id = tileApi.id;
            x = tileApi.x;
            y = tileApi.y;
        }

        // Mise à jour des données (seulement si changement)
        if (user_id != tileApi.user_id) user_id = tileApi.user_id;
        if (lvl != tileApi.lvl) lvl = tileApi.lvl;
        if (build != tileApi.build) SetBuild(tileApi.build);
        if (drone != tileApi.drone) drone = tileApi.drone;
        SetShield(tileApi.shield);
    }



}
