using UnityEngine;
using UnityEngine.UI;


public class OptionPanel : MonoBehaviour
{
    [SerializeField] private PlayerControler playerControler;
    [SerializeField] private Button cancelBtn;
    [SerializeField] private Button confirmBtn;
    [SerializeField] private Slider gapSlider;
    [SerializeField] private Slider bloomSlider;
    [SerializeField] private Toggle hexesColorToggle;


    void Start(){
        
        // add event listener to cancelBtn
        cancelBtn.onClick.AddListener(cancelBtnClic);
        confirmBtn.onClick.AddListener(cancelBtnClic);

        // Slider du gap entre les tiles
        gapSlider.onValueChanged.AddListener(value => PlayerPrefs.SetFloat("opt_gridGap", value));
        // Couleur des hexagones
        hexesColorToggle.onValueChanged.AddListener(value => PlayerPrefs.SetInt("opt_hexesColor", value ? 1 : 0));
        // Flou lumineux
        bloomSlider.onValueChanged.AddListener(value => PlayerPrefs.SetFloat("opt_bloom", value));


        // Set les valeurs des inputs depuis les playerPrefs
        gapSlider.value = PlayerPrefs.GetFloat("opt_gridGap");
        hexesColorToggle.isOn = PlayerPrefs.GetInt("opt_hexesColor", 1) == 1;
        bloomSlider.value = PlayerPrefs.GetFloat("opt_bloom");
    }


    public void cancelBtnClic(){
        gameObject.SetActive(false);
        playerControler.setupOptions();
    }




}
