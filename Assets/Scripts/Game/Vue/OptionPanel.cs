using UnityEngine;
using UnityEngine.UI;


public class OptionPanel : MonoBehaviour
{
    [SerializeField] private PlayerControler playerControler;
    [SerializeField] private Button cancelBtn;
    [Header("Navigation dans les menus")]
    [SerializeField] private Button generalOptionsBtn;
    [SerializeField] private GameObject generalOptionsPanel;

    [SerializeField] private Button keybindsBtn;
    [SerializeField] private GameObject keybindsPanel;


    [Header("Options générales")]
    
    [SerializeField] private Button confirmBtnGeneralOptions;
    [SerializeField] private Slider gapSlider;
    [SerializeField] private Slider bloomSlider;
    [SerializeField] private Toggle hexesColorToggle;
    [SerializeField] private Button accountBtn;

    [Header("Raccourcis clavier")]
    [SerializeField] private Button confirmBtnKeybinds;

    void Start(){
        


        #region Event listeners
        // gestion de la navigation entre les menus
        generalOptionsBtn.onClick.AddListener(() => {
            hideAllPanels();
            generalOptionsPanel.SetActive(true);
        });

        keybindsBtn.onClick.AddListener(() => {
            hideAllPanels();
            keybindsPanel.SetActive(true);
        });


        // add event listener to accountBtn, ouvre le dashboard sur le navigateur
        accountBtn.onClick.AddListener(() => Application.OpenURL(DataManager.Instance.GetData("serverIP") + "/dashboard"));
        
        // add event listener to cancel && confirm buttons
        cancelBtn.onClick.AddListener(cancelBtnClic);
        confirmBtnGeneralOptions.onClick.AddListener(confirmBtnClic);

        // Slider du gap entre les tiles
        gapSlider.onValueChanged.AddListener(value => PlayerPrefs.SetFloat("opt_gridGap", value));
        // Couleur des hexagones
        hexesColorToggle.onValueChanged.AddListener(value => PlayerPrefs.SetInt("opt_hexesColor", value ? 1 : 0));
        // Flou lumineux
        bloomSlider.onValueChanged.AddListener(value => PlayerPrefs.SetFloat("opt_bloom", value));

        #endregion

        // Set les valeurs des inputs depuis les playerPrefs
        gapSlider.value = PlayerPrefs.GetFloat("opt_gridGap");
        hexesColorToggle.isOn = PlayerPrefs.GetInt("opt_hexesColor", 1) == 1;
        bloomSlider.value = PlayerPrefs.GetFloat("opt_bloom");
    }

    private void hideAllPanels(){
        generalOptionsPanel.SetActive(false);
        keybindsPanel.SetActive(false);
    }

    public void cancelBtnClic(){
        gameObject.SetActive(false);
    }

    public void confirmBtnClic(){
        gameObject.SetActive(false);
        playerControler.setupOptions();
    }



}
