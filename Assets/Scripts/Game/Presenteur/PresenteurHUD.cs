using UnityEngine;

public class PresenteurHUD : MonoBehaviour
{
    private HUDVue hudVue;

    private void Awake()
    {
        hudVue = GetComponent<HUDVue>();
    }

    public void UpdateMoney(int money)
    {
        hudVue.UpdateMoney(money);
    }

}
