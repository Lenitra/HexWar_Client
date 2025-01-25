using UnityEngine;
using UnityEngine.UI;

public class UpdateBtn : MonoBehaviour
{
    
    // on start, add a listener to the component button

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => UpdateGame());
    }

    // Update the map
    public void UpdateGame()
    {
        // open the link :
        Application.OpenURL(DataManager.Instance.GetData("serverIP") + " /");
    }
}
