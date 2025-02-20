using UnityEngine;
using UnityEngine.UI;

public class LayoutForPanel : MonoBehaviour
{
    [SerializeField] private Button closeBtn;


    private void Awake()
    {
        closeBtn.onClick.AddListener(ClosePanel);
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}
