using UnityEngine;
using System.Collections.Generic;

public class DataManager : MonoBehaviour
{

    // Instance du singleton
    private static DataManager _instance;


    // Dictionnaire pour stocker les chaînes de caractères
    private Dictionary<string, string> dataDictionary;



    // Propriété pour obtenir l'instance unique
    public static DataManager Instance
    {
        get
        {
            // Si l'instance n'existe pas encore, on en crée une nouvelle
            if (_instance == null)
            {
                // Recherche d'une instance existante dans la scène
                _instance = FindObjectOfType<DataManager>();

                // Si aucune instance n'est trouvée, on en crée une
                if (_instance == null)
                {
                    GameObject dataManager = new GameObject("DataManager");
                    _instance = dataManager.AddComponent<DataManager>();
                    DontDestroyOnLoad(dataManager); // Empêche la destruction lors des changements de scènes
                }
            }
            return _instance;
        }
    }



    // Initialisation
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject); // Empêche la destruction lors des changements de scènes
            dataDictionary = new Dictionary<string, string>();
            initalise();
        }
        else
        {
            Destroy(gameObject); // Si une autre instance existe, on détruit l'objet en cours
        }
    }



    // Méthode pour récupérer une chaîne de caractères
    public string GetData(string key)
    {
        if (dataDictionary.TryGetValue(key, out string value))
        {
            return value;
        }
        return null; // Retourne null si la clé n'existe pas
    }

    // Méthode pour supprimer toutes les données
    public void ClearData()
    {
        dataDictionary.Clear();
    }


    // Méthode pour initialiser les données en fonction de la langue sélectionnée
    void initalise(string lang="fr"){
        dataDictionary.Clear();
        dataDictionary.Add("serverIP", "http://217.160.99.153:8080");
        if (lang == "fr"){
            dataDictionary.Add("build", "Construire");
            dataDictionary.Add("upgrade", "Améliorer");
            dataDictionary.Add("moveUnits", "Déplacer/Attaquer");
            
            dataDictionary.Add("miner", "Excavateur");
            dataDictionary.Add("minerDescription", "Permet de générer de l'argent.");
            dataDictionary.Add("minerInfos1", "Génère 1¤ par heure. Fonctionne jusqu'à 6h en AFK.");
            dataDictionary.Add("minerInfos2", "Génère 1¤ -> 2¤ par heure. Fonctionne jusqu'à 6h en AFK.");
            dataDictionary.Add("minerInfos3", "Génère 2¤ -> 3¤ par heure. Fonctionne jusqu'à 6h en AFK.");
            dataDictionary.Add("minerInfos4", "Génère 3¤ -> 4¤ par heure. Fonctionne jusqu'à 6h en AFK.");
            dataDictionary.Add("minerInfos5", "Génère 4¤ -> 5¤ par heure. Fonctionne jusqu'à 6h en AFK.");

            dataDictionary.Add("hq", "QG");
            dataDictionary.Add("hqDescription", "Permet de pouvoir construire et améliorer des bâtiments");
            dataDictionary.Add("hqInfos1", "Permet de pouvoir construire et améliorer des bâtiments au niveau 1.");
            dataDictionary.Add("hqInfos2", "Permet de pouvoir construire et améliorer des bâtiments au niveau 1 -> 2.");
            dataDictionary.Add("hqInfos3", "Permet de pouvoir construire et améliorer des bâtiments au niveau 2 -> 3.");
            dataDictionary.Add("hqInfos4", "Permet de pouvoir construire et améliorer des bâtiments au niveau 3 -> 4.");
            dataDictionary.Add("hqInfos5", "Permet de pouvoir construire et améliorer des bâtiments au niveau 4 -> 5.");

            dataDictionary.Add("barrack", "Usine de drones");
            dataDictionary.Add("barrackDescription", "Permet de générer des unités.");
            dataDictionary.Add("barrackInfos1", "Génère 1 unité par heure. Fonctionne jusqu'à 6h en AFK.");
            dataDictionary.Add("barrackInfos2", "Génère 1 -> 2 unités par heure. Fonctionne jusqu'à 6h en AFK.");
            dataDictionary.Add("barrackInfos3", "Génère 2 -> 3 unités par heure. Fonctionne jusqu'à 6h en AFK.");
            dataDictionary.Add("barrackInfos4", "Génère 3 -> 4 unités par heure. Fonctionne jusqu'à 6h en AFK.");
            dataDictionary.Add("barrackInfos5", "Génère 4 -> 5 unités par heure. Fonctionne jusqu'à 6h en AFK.");

            dataDictionary.Add("radar", "Radar");
            dataDictionary.Add("radarDescription", "Permet de voir les tuiles adjacentes.");
            dataDictionary.Add("radarInfos1", "Permet de voir les tuiles adjacentes dans un rayon de 3.");
            dataDictionary.Add("radarInfos2", "Permet de voir les tuiles adjacentes dans un rayon de 3 -> 4.");
            dataDictionary.Add("radarInfos3", "Permet de voir les tuiles adjacentes dans un rayon de 4 -> 5.");
            dataDictionary.Add("radarInfos4", "Permet de voir les tuiles adjacentes dans un rayon de 5 -> 6.");
            dataDictionary.Add("radarInfos5", "Permet de voir les tuiles adjacentes dans un rayon de 6 -> 7.");

        }



        // else if (lang == "en"){

        // }

        else {
            lang = "fr";
            initalise(lang);
        }

    }


    

}
