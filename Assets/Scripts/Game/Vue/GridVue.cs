using UnityEngine;
using System.Collections;

public class GridVue : MonoBehaviour
{

    private const float gridGap = 0.1f;
    private const float hexSize = 0.5f;

    [SerializeField] private GameObject tilePrefab;


    #region Génération de la grille

    // Placement d'un hex sur la grille et ajout de ses attributs
    public void CreerTile(Hex hexData)
    {
        float[] coords = GetHexCoordinates(hexData.x, hexData.y);
        GameObject tile = Instantiate(tilePrefab, new Vector3(coords[0], -5, coords[1]), Quaternion.identity);
        // set le nom de l'objet
        tile.name = $"HexObj {hexData.x}:{hexData.y}";
        tile.GetComponent<Tile>().SetupTile(hexData);
        StartCoroutine(CreerTileAnim(tile));
    }


    public void SupprimerTile(GameObject tile)
    {
        StartCoroutine(DestroyTileAnim(tile));
    }


    public void ModifierTile(Hex hexData, Tile tile)
    {
        tile.SetupTile(hexData);
    }

    #endregion



    #region utils

    // Convertit les coordonnées d'un hex en coordonnées pixel
    public float[] GetHexCoordinates(int x, int z)
    {
        // Axial -> Pixel (flat top)
        float px = (1.5f * hexSize + gridGap) * x;
        float pz = (Mathf.Sqrt(3f) * hexSize + gridGap) * (z + x / 2f);

        return new float[] { px, pz };
    }


    #endregion










    #region Coroutine d'animations de tiles
    // Créé un nouvel hexagone avec une animation de pop
    private IEnumerator CreerTileAnim(GameObject tile)
    {
        float duration = 0.5f;
        float t = 0;
        Vector3 startPos = tile.transform.position;
        Vector3 endPos = new Vector3(tile.transform.position.x, 0, tile.transform.position.z);
        while (t < duration)
        {
            t += Time.deltaTime;
            tile.transform.position = Vector3.Lerp(startPos, endPos, t / duration);
            yield return null;
        }
        tile.transform.position = endPos;
    }


    // Détruit un hexagone avec une animation de pop
    private IEnumerator DestroyTileAnim(GameObject tile)
    {
        if (tile == null){yield break;}
        float duration = 0.5f;
        float t = 0;
        Vector3 startPos = tile.transform.position;
        Vector3 endPos = new Vector3(tile.transform.position.x, -5, tile.transform.position.z);
        while (t < duration)
        {
            t += Time.deltaTime;
            tile.transform.position = Vector3.Lerp(startPos, endPos, t / duration);
            yield return null;
        }
        Destroy(tile);
    }


    #endregion




    #region Coroutine d'animations de selection
    
    public IEnumerator SelectTileAnim(Tile tile)
    {
        float duration = 0.25f;
        float t = 0;
        Vector3 startPos = tile.transform.position;
        Vector3 endPos = new Vector3(tile.transform.position.x, 0.5f, tile.transform.position.z);
        while (t < duration)
        {
            t += Time.deltaTime;
            tile.transform.position = Vector3.Lerp(startPos, endPos, t / duration);
            yield return null;
        }
        tile.transform.position = endPos;
    }
    
    
    public IEnumerator DeselectTileAnim(Tile tile)
    {
        float duration = 0.25f;
        float t = 0;
        Vector3 startPos = tile.transform.position;
        Vector3 endPos = new Vector3(tile.transform.position.x, 0, tile.transform.position.z);
        while (t < duration)
        {
            t += Time.deltaTime;
            tile.transform.position = Vector3.Lerp(startPos, endPos, t / duration);
            yield return null;
        }
        tile.transform.position = endPos;
    }
    
    
    #endregion






}
