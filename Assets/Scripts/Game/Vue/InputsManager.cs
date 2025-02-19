using UnityEngine;
using System.Collections;


public class InputsManager : MonoBehaviour
{   
    private PresenteurInputs presenteurInputs;
    [SerializeField] CamController camController;


    private void Start()
    {
        presenteurInputs = GetComponent<PresenteurInputs>();
    }
    



    private void Update()
    {

        
        // Vérification supplémentaire pour éviter les conflits avec le drag de la caméra
        if (Input.GetMouseButtonUp(0) && camController.isDragging == false) 
        {
            presenteurInputs.TraiterClick(Input.mousePosition);
        }
    }


}
