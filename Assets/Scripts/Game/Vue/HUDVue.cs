using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HUDVue : MonoBehaviour
{

    private PresenteurHUD presenteurHUD;

    [SerializeField] private TextMeshProUGUI moneyText;
    
    // Boutons du HUD statique
    [Header("Boutons du HUD statique")]
    [SerializeField] private Button optionsBtn;
    [SerializeField] private Button locateHQBtn;
    [SerializeField] private Button dispatchBtn;

    // 
    [SerializeField] private GameObject dispatchPanel;


    

    void Start(){
        presenteurHUD = GetComponent<PresenteurHUD>();

        optionsBtn.onClick.AddListener(optionsBtnClick);
        locateHQBtn.onClick.AddListener(locateHQBtnClick);
        dispatchBtn.onClick.AddListener (activateDispatchPanel);
    }

    private void activateDispatchPanel(){
        dispatchPanel.SetActive(true);
    }

    private void locateHQBtnClick(){
        presenteurHUD.OnBtnLocate();
    }

    private void optionsBtnClick(){
        Debug.Log("Bouton options cliqué");
    }

    public void dispatchBtnClick(){
        presenteurHUD.OnBtnDispatch();
    }




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


    public void nopePanel(string message){
        Debug.Log("Nope panel");
    }

}
