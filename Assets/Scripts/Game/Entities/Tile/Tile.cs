using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private string color = "#000000";
    [SerializeField] private bool hasShield = false;
    [SerializeField] private string owner = "";


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


    // Initialisation de la tile avec les données du serveur
    internal void SetData(TileApi tileApi)
    {
        if (tileApi == null)
        {
            Debug.LogError("TileApi est null");
            return;
        }

        if (this.id == int.MinValue || this.x == int.MinValue || this.y == int.MinValue)
        {
            this.transform.position = new Vector3(tileApi.x * (1 + gridGap), 0, tileApi.y * (1 + gridGap));
        }

        this.id = tileApi.id;
        this.user_id = tileApi.user_id;
        this.x = tileApi.x;
        this.y = tileApi.y;
        this.lvl = tileApi.lvl;
        this.build = tileApi.build;
        this.drone = tileApi.drone;
        this.shield = tileApi.shield;
    }

}
