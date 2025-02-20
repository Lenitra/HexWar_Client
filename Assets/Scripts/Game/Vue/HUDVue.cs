using UnityEngine;
using System.Collections;

public class HUDVue : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI moneyText;

    public void UpdateMoney(int money)
    {
        moneyText.text = "¤ " + money.ToString();
    }


    private IEnumerator UpdateMoneyAnimation(int newMoney)
    {
        int oldMoney = int.Parse(moneyText.text.Substring(2));
        int diff = newMoney - oldMoney;
        int step = diff / 10;
        int currentMoney = oldMoney;

        for (int i = 0; i < 10; i++)
        {
            currentMoney += step;
            moneyText.text = "¤ " + currentMoney.ToString();
            yield return new WaitForSeconds(0.1f);
        }

        moneyText.text = "¤ " + newMoney.ToString();
    }

}
