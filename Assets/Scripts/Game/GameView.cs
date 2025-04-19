using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class GameView : MonoBehaviour
{
    
    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI money; // Argent du joueur   





    public void SetMoney(int value)
    {
        money.text = "¤ " + value.ToString();
    }



}
