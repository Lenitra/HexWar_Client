using UnityEngine;

public class PresenteurHUD : MonoBehaviour
{
    private HUDVue hudVue;
    // private GridVue gridVue;
    private GameManager gameManager;

    
    
    private void Start()
    {
        hudVue = GetComponent<HUDVue>();
        gameManager = GetComponent<GameManager>();
        // gridVue = GetComponent<GridVue>();
    }



    public void UpdateMoney(int money)
    {
        hudVue.UpdateMoney(money);
    }




    #region Gestion des boutons en haut à droite
    // Afficher les infos sur les différentes tiles (Owner, Drones)
    public void OnBtnInfos()
    {
        
    }

    // Déplacer la caméra sur le HQ
    public void OnBtnLocate()
    {
        
    }

    // Demander au serveur de dispatcher les drones
    public void OnBtnDispatch()
    {
        
    }



    #endregion



}
