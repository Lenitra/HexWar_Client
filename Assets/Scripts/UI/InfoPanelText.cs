using UnityEngine;
using TMPro;

public class InfoPanelText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coorinates;
    [SerializeField] private TextMeshProUGUI owner;
    [SerializeField] private TextMeshProUGUI units;

    public void SetText(string text)
    {
        if (text == "")
        {
            coorinates.text = "";
            owner.text = "";
            units.text = "";
            return;
        }
        coorinates.text = text.Split('\n')[0];
        owner.text = "\n" + text.Split('\n')[1];
        units.text = "\n\n" + text.Split('\n')[2];
    }

}
