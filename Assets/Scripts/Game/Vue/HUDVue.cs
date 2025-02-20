using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HUDVue : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;
    
    // Boutons du HUD statique
    [Header("Boutons du HUD statique")]
    [SerializeField] private Button optionsBtn;
    [SerializeField] private Button locateHQBtn;
    [SerializeField] private Button infosTilesBtn;


    

    




    #region Gestion de l'actualisation de l'argent
    public void UpdateMoney(int money)
    {
        StartCoroutine(UpdateMoneyAnimation(money));
    }


    private IEnumerator UpdateMoneyAnimation(int newMoney)
    {
        int oldMoney = int.Parse(moneyText.text.Substring(2));
        float duration = 0.5f;
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            moneyText.text = $"¤ {Mathf.RoundToInt(Mathf.Lerp(oldMoney, newMoney, t / duration))}";
            yield return null;
        }
        moneyText.text = $"¤ {newMoney}";
    }
    #endregion

}
