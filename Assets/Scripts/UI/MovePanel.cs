using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MovePanel : MonoBehaviour
{
    [SerializeField] private Slider moveSlider;
    [SerializeField] private Button moveBtn;
    [SerializeField] private Button cancelBtn;
    [SerializeField] private Button maxBtn;
    [SerializeField] private InputField selectedUnitsInputField;

    [SerializeField] private PlayerControler playerControler;

    private string origin;
    private string destination;


    void Start(){
        // add event listener to cancelBtn
        cancelBtn.onClick.AddListener(cancelBtnClic);
        // on value change on moveSlider change the value of selectedUnitsInputField et inversement
        selectedUnitsInputField.onValueChanged.AddListener(inputChange);
        moveSlider.onValueChanged.AddListener(sliderChange);
        moveBtn.onClick.AddListener(() => playerControler.getFromMovePanel(int.Parse(moveSlider.value.ToString())));
        maxBtn.onClick.AddListener(() => selectedUnitsInputField.text = moveSlider.maxValue.ToString());
    }

    public void init(int maxUnits){
        gameObject.SetActive(true);
        selectedUnitsInputField.text = "0";
        moveSlider.value = 0;
        moveSlider.maxValue = maxUnits;
    }

    void cancelBtnClic(){
        playerControler.unselectTile();
        gameObject.SetActive(false);
    }


    void inputChange(string value){
        if (value == ""){
            moveSlider.value = 0;
        } else {
            if (int.Parse(value) < 0){
                selectedUnitsInputField.text = "0";
                moveSlider.value = 0;
            }
            
            if (int.Parse(value) > moveSlider.maxValue){
                selectedUnitsInputField.text = moveSlider.maxValue.ToString();
                moveSlider.value = moveSlider.maxValue;
            }

            moveSlider.value = int.Parse(selectedUnitsInputField.text);
        }
    }

    void sliderChange(float value){
        selectedUnitsInputField.text = value.ToString();
    }

}
