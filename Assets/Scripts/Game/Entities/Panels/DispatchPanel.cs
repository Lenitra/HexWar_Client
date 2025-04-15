using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DispatchPanel : MonoBehaviour
{
    // [SerializeField] private GridVue gridVue;
    [SerializeField] private Controller controller;

    [Header("Elements communs")]

    [SerializeField] private Button closeButton;
    [SerializeField] private Button validateBtn;




    void Start()
    {
        // Setup des boutons généraux
        closeButton.onClick.AddListener(ClosePanel);

        // Setup des boutons de déplacement
        validateBtn.onClick.AddListener(ValidateDispatch);
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    public void OpenPanel()
    {
        gameObject.SetActive(true);
    }

    public void ValidateDispatch()
    {
        // TODO: Envoyer la commande de dispatch au serveur
        // hudVue.dispatchBtnClick();
        ClosePanel();
    }


}
