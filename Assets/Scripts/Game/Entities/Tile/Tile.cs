using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




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
    [SerializeField] private GameObject glowObject;
    [SerializeField] private GameObject shieldObject;
    [SerializeField] private LineRenderer nodeRadiusObject;



    private GameObject building;
    private readonly float gridGap = 0.05f;
    private readonly float selectElevation = 1f;



}
