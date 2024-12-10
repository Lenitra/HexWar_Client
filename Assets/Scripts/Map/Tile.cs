using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;



public class Tile : MonoBehaviour
{
    // private with getters and setters
    public int units = 0;
    public string owner = "";
    public int[] position = new int[2];
    public string type = "";
    public TextMeshPro tileInfos;
    public string color;

    private float originalBaseY;
    private float selectionOffset = 0.8f;

    // textmeshpro for info on the tile
    [SerializeField] private GameObject toShowOnSelected;
    [SerializeField] private GameObject hoverOwner;
    [SerializeField] private GameObject glow;
    public GameObject moreInfo;

    [Header("Prefabs")]
    [SerializeField] private GameObject hqPrefab;
    [SerializeField] private GameObject moneyPrefab;
    [SerializeField] private GameObject radarPrefab;
    [SerializeField] private GameObject barrackPrefab;

    private Coroutine activeCoroutine; // Référence à la coroutine active
    

    private GameObject infrastrucutre;



    void Start()
    {
        originalBaseY = transform.position.y;

        // name of the object is "Hexagon x, z"
        position[0] = int.Parse(gameObject.name.Split("Hexagon ")[1].Split(":")[0]);
        position[1] = int.Parse(gameObject.name.Split("Hexagon ")[1].Split(":")[1]);

        
        toShowOnSelected.gameObject.SetActive(false);

        if (owner != ""){
            glow.SetActive(true);
            // add a material to the hoverOwner
            // set renderinmode to transparent
            Material material = glow.GetComponent<Renderer>().material;
            material.SetFloat("_Mode", 2);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            material.color = new Color(1,1,1,1); 
            material.EnableKeyword("_EMISSION");


            if (color != "")
            {
                float r = float.Parse(color.Split('|')[0])/255;
                float g = float.Parse(color.Split('|')[1])/255;
                float b = float.Parse(color.Split('|')[2])/255;
                float a = 1;
                material.SetColor("_EmissionColor", new Color(r,g,b,a));
                material.color = new Color(r,g,b,a);
                transform.position = new Vector3(transform.position.x, transform.position.y + 0.025f, transform.position.z);
            }
            else
            {
                material.SetColor("_EmissionColor", new Color(1,1,1,1));
            }
            // setup intensity of the emission
            material.SetFloat("_EmissionScaleUI", 2f);
        } else {
            glow.SetActive(false);
        }


        
        





        hoverOwner.SetActive(false);



        // switch pour configurer l'objet à instancier en fonction du type en minuscule
        Destroy(infrastrucutre);
        infrastrucutre = null;

        switch (type.ToLower().Split(":")[0])
        {
            case "hq":
                infrastrucutre = Instantiate(hqPrefab, Vector3.zero, Quaternion.identity, transform);
                infrastrucutre.transform.localPosition = Vector3.zero;
                break;

            case "miner":
                infrastrucutre = Instantiate(moneyPrefab, Vector3.zero, Quaternion.identity, transform);
                infrastrucutre.transform.localPosition = Vector3.zero;
                break;

            case "radar":
                infrastrucutre = Instantiate(radarPrefab, Vector3.zero, Quaternion.identity, transform);
                infrastrucutre.transform.localPosition = Vector3.zero;
                break;

            case "barrack":
                infrastrucutre = Instantiate(barrackPrefab, Vector3.zero, Quaternion.identity, transform);
                infrastrucutre.transform.localPosition = Vector3.zero;
                break;

            case "defensive":
                // Ajouter des actions spécifiques pour "defensive" si nécessaire
                break;

            case "offensive":
                // Ajouter des actions spécifiques pour "offensive" si nécessaire
                break;

            case "":
                break;

            default:
                Debug.LogWarning("Type non reconnu : " + type);
                break;
        }



    }



    public void setupTile(int units, string owner, string type, string color) {
        this.units = units;
        this.owner = owner;
        this.type = type;
        this.color = color;
        tileInfos.text = "";
        if (owner != ""){
            tileInfos.text += "<sprite=112>" + owner;
        } 
        if (owner == PlayerPrefs.GetString("username"))
        {
            tileInfos.text += "\n" + "<sprite=91>" + units;
        }

        Start();
    }

    

    public void select()
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }
        activeCoroutine = StartCoroutine(SelectTile());
    }

    public void unselect()
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }
        activeCoroutine = StartCoroutine(UnselectTile());
    }

    private IEnumerator SelectTile()
    {
        toShowOnSelected.gameObject.SetActive(true);

        float animationDuration = 0.25f;
        float elapsedTime = 0f;

        float startY = transform.position.y;
        float endY = originalBaseY + selectionOffset;

        while (elapsedTime < animationDuration)
        {
            float t = elapsedTime / animationDuration;
            float newY = Mathf.Lerp(startY, endY, t);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Assure la position finale
        transform.position = new Vector3(transform.position.x, endY, transform.position.z);
        yield return new WaitForSeconds(0.1f);
    }

    private IEnumerator UnselectTile()
    {
        toShowOnSelected.gameObject.SetActive(false);

        float animationDuration = 0.25f;
        float elapsedTime = 0f;

        float startY = transform.position.y;
        float endY = originalBaseY; // On revient à la base

        while (elapsedTime < animationDuration)
        {
            float t = elapsedTime / animationDuration;
            float newY = Mathf.Lerp(startY, endY, t);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Assure la position finale
        transform.position = new Vector3(transform.position.x, endY, transform.position.z);
        yield return new WaitForSeconds(0.1f);
    }






}
