using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class OptionsPanel : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Controller controller;

    [Header("Boutons")]
    [SerializeField] private Button closeButton;

    [Header("Partie compte")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField colorInput;
    // [SerializeField] private TMP_InputField discrodIdInput;

    [Header("Partie jeu")]
    [SerializeField] private Slider cameraSpeedSlider;
    [SerializeField] private Slider cameraZoomSlider;

    private void Start()
    {
        //TODO: Initialisation des valeurs par défaut
        usernameInput.text = PlayerPrefs.GetString("username", "Player");
        colorInput.text = PlayerPrefs.GetString("color", "#FFFFFF");
        cameraSpeedSlider.value = PlayerPrefs.GetFloat("opt_cam_speed", 1.0f);
        cameraSpeedSlider.value = PlayerPrefs.GetFloat("opt_cam_zoom", 1.0f);

        //TODO: Listeners
        closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        cameraSpeedSlider.onValueChanged.AddListener(controller.SetCameraSpeed);
        cameraZoomSlider.onValueChanged.AddListener(controller.SetCameraZoom);
        usernameInput.onEndEdit.AddListener(controller.SetUsername);
    }




}
