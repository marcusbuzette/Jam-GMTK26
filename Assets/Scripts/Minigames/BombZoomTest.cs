using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BombZoomTest : MonoBehaviour
{
    public RectTransform bomb;

    public float zoomScale = 3f;
    public float durationZoom = 0.3f;
    public float durationShake = 0.5f;
    public float strengthShake = 10f;
    public float durationOpen = 2f;

    Vector2 originalPosition;
    RectTransform targetModule;
    Vector3 preShakePos;
    private Coroutine shakeCoroutine;
    private Coroutine openCloseCoroutine;
    bool isOpen = true;
    bool alreadyZoomed;
    MinigameBase openMinigame;
    /* void Awake()
    {
        originalPosition = bomb.anchoredPosition;
        originalScale = bomb.localScale;
    } */
    public void ZoomTo(RectTransform module) {
        //if(module==targetModule)return;//don't zoom if already zoomed
        if(alreadyZoomed)return;
        openMinigame=module.GetComponent<MinigameBase>();
        openMinigame.ToggleZoom();
        targetModule=module;
        originalPosition=bomb.anchoredPosition;
        alreadyZoomed=true;
        StartCoroutine(nameof(ZoomToCor));
    }
    /* public void ZoomTo(RectTransform module)
    {
        Vector2 modulePos = module.anchoredPosition;

        bomb.localScale = Vector3.one * zoomScale;

        bomb.anchoredPosition = -modulePos * zoomScale;
    } */
    IEnumerator ZoomToCor() {
        if(targetModule==null)yield break;
        Vector2 modulePos = targetModule.anchoredPosition;
        float timer = 0;
        while(timer<durationZoom){
            timer += Time.deltaTime;
            float progress = timer/durationZoom;
            bomb.localScale = Vector3.Lerp(Vector3.one,Vector3.one * zoomScale,progress);
            bomb.anchoredPosition = Vector2.Lerp(originalPosition,-modulePos * zoomScale,progress);
            yield return new WaitForEndOfFrame();
        }
    }
    public void ResetView() {
        targetModule=null;
        alreadyZoomed=false;
        openMinigame?.ToggleZoom();
        openMinigame=null;
        StartCoroutine(nameof(ResetViewCor));
    }
    /* public void ResetView()
    {
        bomb.localScale = originalScale;
        bomb.anchoredPosition = originalPosition;
    } */
    IEnumerator ResetViewCor() {
        Vector2 currentPos = bomb.anchoredPosition;
        Vector2 currentScale = bomb.localScale;
        float timer = 0;
        while(timer<durationZoom){
            timer += Time.deltaTime;
            float progress = timer/durationZoom;
            bomb.localScale = Vector3.Lerp(currentScale,Vector3.one,progress);
            bomb.anchoredPosition = Vector2.Lerp(currentPos,originalPosition,progress);
            yield return new WaitForEndOfFrame();
        }
    }
    public void TriggerShake()
    {
        preShakePos = bomb.anchoredPosition;
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        shakeCoroutine = StartCoroutine(ShakeRoutine(durationShake, strengthShake));
    }

    private IEnumerator ShakeRoutine(float duration, float strength)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            
            // Generate a random position offset within a 2D circle
            Vector2 randomOffset = Random.insideUnitCircle * strength;
            bomb.localPosition = preShakePos + new Vector3(randomOffset.x, randomOffset.y, 0f);
            
            yield return null;
        }

        // Return precisely to the original position once completed
        bomb.localPosition = preShakePos;
    }
    void Update() {
        if (Keyboard.current.oKey.wasPressedThisFrame) {
            OpenClose();
        }
    }
    float bombHeight;
    public void OpenClose()
    {
        bombHeight = bomb.rect.height;
        if (alreadyZoomed) {
            ResetView();
        }
        if(!isOpeningClosing)StartCoroutine(OpenCloseRoutine(durationOpen));
    }
    bool isOpeningClosing;
    private IEnumerator OpenCloseRoutine(float duration)
    {
        isOpeningClosing = true;
        float elapsedTime = 0f;
        float posY,finalPosY;
        if (isOpen) {
            posY=0;
            finalPosY=1080;
        } else {
            posY=1080;
            finalPosY=0;
        }
        Vector2 ini=new Vector2(0,posY);
        Vector2 fin=new Vector2(0,finalPosY);
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime/duration;
            
            bomb.anchoredPosition=Vector2.Lerp(ini,fin,progress);
            yield return null;
        }
        isOpen=!isOpen;
        isOpeningClosing = false;
    }

}
