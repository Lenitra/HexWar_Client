using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ErrorPanel : MonoBehaviour
{
    [Header("Elements communs")]
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button validateBtn;

    [Header("Informations visuelles")]
    [SerializeField] private TextMeshProUGUI description;


    void Start(){
        closeBtn.onClick.AddListener(closeBtnClick);
        validateBtn.onClick.AddListener(closeBtnClick);
    }

    private void closeBtnClick(){
        gameObject.SetActive(false);
    }

    public void init(string desc){
        gameObject.SetActive(true);
        description.text = desc;
    }

    void Update(){
        if (Input.GetKeyDown(KeyCode.Escape)){
            closeBtnClick();
        }
        // on press enter 
        if (Input.GetKeyDown(KeyCode.Return)){
            closeBtnClick();
        }
        if (Input.GetKeyDown(KeyCode.KeypadEnter)){
            closeBtnClick();
        }
    }

}
