using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraObstruction : MonoBehaviour {
    public Transform[] targets; // Personagens que precisam ficar visíveis
    public float fadeSpeed = 3f;
    
    [Header("Alpha Settings")]
    public float minAlpha = 0.1f;
    public float minAlphaLight = 0.3f;

    [Header("Screen Detection")]
    [Tooltip("Raio do círculo na tela. 0.2 significa 20% da altura da tela.")]
    [Range(0.01f, 1f)]
    public float screenFadeRadius = 0.15f; 

    // Estrutura para cachear o Renderer e o alpha que ele deve atingir
    private struct ObstructorData {
        public Renderer renderer;
        public float targetAlpha;
    }

    private Camera cam;
    private MaterialPropertyBlock propBlock;
    private Dictionary<Renderer, float> fadingObjects = new Dictionary<Renderer, float>();
    
    // Agora temos apenas um array que guarda tudo!
    private ObstructorData[] potentialObstructors;

    void Start() {
        cam = GetComponent<Camera>();
        propBlock = new MaterialPropertyBlock();
        
        List<ObstructorData> obstructorsList = new List<ObstructorData>();

        // 1. Pega os obstáculos densos (minAlpha)
        GameObject[] obstructionObjects = GameObject.FindGameObjectsWithTag("Obstruction");
        foreach (var obj in obstructionObjects) {
            Renderer rend = obj.GetComponent<Renderer>();
            if (rend != null) {
                obstructorsList.Add(new ObstructorData { renderer = rend, targetAlpha = minAlpha });
            }
        }

        // 2. Pega os obstáculos leves (minAlphaLight)
        GameObject[] obstructionLightObjects = GameObject.FindGameObjectsWithTag("ObstructionLight");
        foreach (var obj in obstructionLightObjects) {
            Renderer rend = obj.GetComponent<Renderer>();
            if (rend != null) {
                obstructorsList.Add(new ObstructorData { renderer = rend, targetAlpha = minAlphaLight });
            }
        }

        // Converte para array para máxima performance no Update
        potentialObstructors = obstructorsList.ToArray();
    }

    public void AddTarget(Transform target) {
        List<Transform> targetList = new List<Transform>(targets);
        if (!targetList.Contains(target)) {
            targetList.Add(target);
            targets = targetList.ToArray();
        }
    }

    void Update() {
        if (targets == null || targets.Length == 0) return;

        HashSet<Renderer> obstructingThisFrame = new HashSet<Renderer>();

        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(cam);
        float aspectRatio = (float)Screen.width / Screen.height;

        List<Vector3> targetsViewportPos = new List<Vector3>();
        foreach (var target in targets) {
            if (target != null) {
                targetsViewportPos.Add(cam.WorldToViewportPoint(target.position));
            }
        }

        // Loop único passando por TODOS os obstáculos (tanto normais quanto lights)
        foreach (var obs in potentialObstructors) {
            Renderer rend = obs.renderer;
            if (rend == null) continue;

            Bounds b = rend.bounds;

            if (GeometryUtility.TestPlanesAABB(frustumPlanes, b)) {
                
                Vector3 objViewportPos = cam.WorldToViewportPoint(b.center);
                float closestObjDepth = objViewportPos.z - Mathf.Max(b.extents.x, b.extents.z);

                bool isObstructing = false;

                foreach (Vector3 targetVP in targetsViewportPos) {
                    if (closestObjDepth < targetVP.z) {
                        float diffX = (objViewportPos.x - targetVP.x) * aspectRatio; 
                        float diffY = (objViewportPos.y - targetVP.y);
                        
                        float distanceOnScreen = Mathf.Sqrt((diffX * diffX) + (diffY * diffY));

                        if (distanceOnScreen < screenFadeRadius) {
                            isObstructing = true;
                            break; 
                        }
                    }
                }

                if (isObstructing) {
                    // Aqui está o segredo: Passamos o alpha salvo no cache especificamente para este objeto!
                    FadeRenderer(rend, obs.targetAlpha);
                    obstructingThisFrame.Add(rend);
                }
            }
        }

        // Restaurar a opacidade
        var toRestore = new List<Renderer>(fadingObjects.Keys);
        foreach (var rend in toRestore) {
            if (rend == null) continue; 

            if (!obstructingThisFrame.Contains(rend)) {
                FadeRenderer(rend, 1f);
                
                if (Mathf.Approximately(fadingObjects[rend], 1f)) {
                    fadingObjects.Remove(rend);
                }
            }
        }
    }

    void FadeRenderer(Renderer rend, float targetAlpha) {
        if (!fadingObjects.ContainsKey(rend))
            fadingObjects[rend] = 1f;

        float currentAlpha = fadingObjects[rend];
        float newAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);
        fadingObjects[rend] = newAlpha;

        rend.GetPropertyBlock(propBlock);
        propBlock.SetFloat("_Alpha", newAlpha);
        rend.SetPropertyBlock(propBlock);
    }
}