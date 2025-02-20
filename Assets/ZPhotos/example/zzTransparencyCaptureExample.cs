using System.Collections;
using UnityEngine;

public class zzTransparencyCaptureExample:MonoBehaviour
{
    public Texture2D capturedImage;



    public IEnumerator capture()
    {
        //capture whole screen
        Rect lRect = new Rect(0f,0f,Screen.width,Screen.height);


        yield return new WaitForEndOfFrame();
        //After Unity4,you have to do this function after WaitForEndOfFrame in Coroutine
        //Or you will get the error:"ReadPixels was called to read pixels from system frame buffer, while not inside drawing frame"
        capturedImage = zzTransparencyCapture.capture(lRect);
    
        // save the image
        byte[] bytes = capturedImage.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.dataPath + "/capturedImage.png", bytes);
    }


    public void Awake()
    {
            StartCoroutine(capture());

    }


}