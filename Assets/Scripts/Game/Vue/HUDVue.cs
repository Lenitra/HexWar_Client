using UnityEngine;
using System.Collections;

public class HUDVue : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI moneyText;

    public void UpdateMoney(int money)
    {
        moneyText.text = "¤ " + money.ToString();
    }



}
