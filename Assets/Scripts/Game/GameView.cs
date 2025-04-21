using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameView : MonoBehaviour
{
    
    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI money; // Argent du joueur   

    [Header("Game effects")]
    [SerializeField] private GameObject sphereScanner; // Sphere scanner prefab
    [SerializeField] private GameObject dronePrefab; // Drone prefab
    [SerializeField] private LineRenderer prefabLine; // Prefab de la ligne de déplacement des drones




    public void SetMoney(int value)
    {
        money.text = "¤ " + value.ToString();
    }




    #region Effets visuels de la grille de jeu



    #region Effets de scan
    public void ScanEffect(Tile point, bool expand = true)
    {
        StartCoroutine(BubleAnim(point, expand));
    }

    private IEnumerator BubleAnim(Tile tile, bool expand = true)
    {
        GameObject sphere = Instantiate(sphereScanner, tile.transform.position, Quaternion.identity);
        if (expand)
        {
            sphere.GetComponent<Scanner>().StartScanOutward();
        }
        else
        {
            sphere.GetComponent<Scanner>().StartScanInward();
        }
        yield return null;
    }

    #endregion




    #region Dessin d'une ligne de déplacement de drones
        public void DrawPathLine(Tile[] move)
        {
            StartCoroutine(AnimationMoveUnits(move));
        }


        private IEnumerator AnimationMoveUnits(Tile[] move)
        {
            string debugMsg = "";
            // Définir la durée totale de l'animation aller et retour
            float durationGo = 0.001f; // Durée totale pour dessiner la ligne
            float durationOg = 2f; // Durée totale pour effacer la ligne

            // 1. Convertir les données en liste de positions
            List<Vector3> positions = new List<Vector3>();
            for (int i = 0; i < move.Length; i++)
            {
                float[] pos = move[i].GetHexCoordinates();
                positions.Add(new Vector3(pos[0], -4f, pos[1]));
                debugMsg += $"{move[i].X}:{move[i].Y} -> ";
            }

            Debug.Log(debugMsg);

            if (positions.Count == 0)
                yield break;

            LineRenderer moveUnitsLine = Instantiate(prefabLine);
            // instancier le drone prefab
            GameObject drone = Instantiate(dronePrefab, positions[0], Quaternion.identity);
            // drone.transform.SetParent(moveUnitsLine.transform);

            // 2. Calculer la distance totale du chemin
            float totalDistance = 0f;
            for (int i = 0; i < positions.Count - 1; i++)
            {
                totalDistance += Vector3.Distance(positions[i], positions[i + 1]);
            }

            // 3. Animation de création (aller) sur l'ensemble du chemin
            float t = 0f;
            while (t < durationGo)
            {
                t += Time.deltaTime;
                float fraction = Mathf.Clamp01(t / durationGo);
                float distanceCovered = fraction * totalDistance;

                // Déterminer la position courante le long du chemin
                float d = 0f;
                Vector3 currentPoint = positions[0];
                int lastFullIndex = 0;
                for (int i = 0; i < positions.Count - 1; i++)
                {
                    float segmentLength = Vector3.Distance(positions[i], positions[i + 1]);
                    if (d + segmentLength >= distanceCovered)
                    {
                        float segFraction = (distanceCovered - d) / segmentLength;
                        currentPoint = Vector3.Lerp(positions[i], positions[i + 1], segFraction);
                        lastFullIndex = i;
                        break;
                    }
                    d += segmentLength;
                }



                // Construire la ligne actuelle :
                // - Tous les points déjà atteints (de 0 à lastFullIndex)
                // - Le point courant interpolé sur le segment en cours
                List<Vector3> currentLine = new List<Vector3>();
                for (int i = 0; i <= lastFullIndex; i++)
                {
                    currentLine.Add(positions[i]);
                }
                currentLine.Add(currentPoint);

                moveUnitsLine.positionCount = currentLine.Count;
                for (int i = 0; i < currentLine.Count; i++)
                {
                    moveUnitsLine.SetPosition(i, currentLine[i]);
                }

                yield return null;
            }


            // Assurer que la ligne complète est affichée
            moveUnitsLine.positionCount = positions.Count;
            for (int i = 0; i < positions.Count; i++)
            {
                moveUnitsLine.SetPosition(i, positions[i]);
            }


            // 4. Animation d'effacement (retour) : faire disparaître la ligne depuis le début jusqu'à la fin
            // Et faire faire suivre le drone à la ligne
            t = 0f;
            while (t < durationOg)
            {
                t += Time.deltaTime;
                float fraction = Mathf.Clamp01(t / durationOg);
                float distanceErased = fraction * totalDistance;

                // Déterminer le nouveau point de départ le long du chemin
                float d = 0f;
                Vector3 newStart = positions[0];
                Vector3 nextDirection = Vector3.forward; // Valeur par défaut pour éviter les warnings
                int firstFullIndex = 0;
                for (int i = 0; i < positions.Count - 1; i++)
                {
                    float segmentLength = Vector3.Distance(positions[i], positions[i + 1]);
                    if (d + segmentLength >= distanceErased)
                    {
                        float segFraction = (distanceErased - d) / segmentLength;
                        newStart = Vector3.Lerp(positions[i], positions[i + 1], segFraction);
                        firstFullIndex = i + 1;

                        // Calculer la direction pour orienter le drone
                        Vector3 dir = positions[i + 1] - positions[i];
                        if (dir != Vector3.zero)
                        {
                            nextDirection = dir.normalized;
                        }

                        break;
                    }
                    d += segmentLength;
                }

                // Déplacer et orienter le drone
                drone.transform.position = newStart;
                if (nextDirection != Vector3.zero)
                {
                    drone.transform.rotation = Quaternion.LookRotation(nextDirection);
                }

                // Construire la nouvelle ligne à partir du nouveau point de départ jusqu'à la fin
                List<Vector3> newLine = new List<Vector3>();
                newLine.Add(newStart);
                for (int i = firstFullIndex; i < positions.Count; i++)
                {
                    newLine.Add(positions[i]);
                }

                moveUnitsLine.positionCount = newLine.Count;
                for (int i = 0; i < newLine.Count; i++)
                {
                    moveUnitsLine.SetPosition(i, newLine[i]);
                }

                yield return null;
            }



            // Détruire la ligne
            Destroy(moveUnitsLine.gameObject);

            // Animation de destruction du drone
            float destroyDuration = 0.5f;
            t = 0f;
            Vector3 startPosition = drone.transform.position;
            Vector3 endPosition = new Vector3(startPosition.x, startPosition.y - 1f, startPosition.z);
            Vector3 startScale = drone.transform.localScale;
            Vector3 endScale = Vector3.zero;
            while (t < destroyDuration)
            {
                t += Time.deltaTime;
                float fraction = Mathf.Clamp01(t / destroyDuration);
                drone.transform.localScale = Vector3.Lerp(startScale, endScale, fraction);
                drone.transform.position = Vector3.Lerp(startPosition, endPosition, fraction);
                yield return null;
            }
            // Assurer que le drone est complètement détruit
            drone.transform.localScale = endScale;

            Destroy(drone);
            yield return null;
        }



    #endregion
    
    
    
    #endregion



}
