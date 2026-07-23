using System.Collections;
using UnityEngine;

public class BombZoomTest : MonoBehaviour
{
    public RectTransform bomb;

    public float zoomScale = 3f;
    public float duration = 0.3f;

    Vector2 originalPosition;
    Vector3 originalScale;
    RectTransform targetModule;

    void Awake()
    {
        originalPosition = bomb.anchoredPosition;
        originalScale = bomb.localScale;
    }
    public void ZoomTo(RectTransform module) {
        targetModule=module;
        StartCoroutine(nameof(ZoomToCor));
    }
    /* public void ZoomTo(RectTransform module)
    {
        Vector2 modulePos = module.anchoredPosition;

        bomb.localScale = Vector3.one * zoomScale;

        bomb.anchoredPosition = -modulePos * zoomScale;
    } */
    IEnumerator ZoomToCor() {
        Vector2 modulePos = targetModule.anchoredPosition;
        float timer = 0;
        while(timer<duration){
            timer += Time.deltaTime;
            float progress = timer/duration;
            bomb.localScale = Vector3.Lerp(Vector3.one,Vector3.one * zoomScale,progress);
            bomb.anchoredPosition = Vector2.Lerp(originalPosition,-modulePos * zoomScale,progress);
            yield return new WaitForEndOfFrame();
        }
    }
    public void ResetView() {
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
        while(timer<duration){
            timer += Time.deltaTime;
            float progress = timer/duration;
            bomb.localScale = Vector3.Lerp(currentScale,Vector3.one,progress);
            bomb.anchoredPosition = Vector2.Lerp(currentPos,originalPosition,progress);
            yield return new WaitForEndOfFrame();
        }
    }
}
