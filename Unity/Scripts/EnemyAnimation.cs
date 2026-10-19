using UnityEngine;
using Game.src.core.entities.Enemies;
using System.Collections;
using UnityEngine.UI;

public class EnemyAnimation : MonoBehaviour
{
    [Header("Animation params")]
    private Enemy enemy;
    private RectTransform rectTransform;
    private Coroutine moveCoroutine;
    
    [Header("Animation settings")]
    public float moveDuration = 0.5f;
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    public void Initialize(Enemy enemy)
    {
        this.enemy = enemy;
        rectTransform = GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);
    }
    
    public void MoveTo(Vector2 targetPosition)
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);
            
        moveCoroutine = StartCoroutine(MoveCoroutine(targetPosition));
    }
    
    private IEnumerator MoveCoroutine(Vector2 targetPosition)
    {
        Vector2 startPosition = rectTransform.anchoredPosition;
        float time = 0f;
        
        while (time < moveDuration)
        {
            time += Time.deltaTime;
            float t = moveCurve.Evaluate(time / moveDuration);
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            yield return null;
        }
        rectTransform.anchoredPosition = targetPosition;
    }

    public void GetDamageAnimation()
    {
        StartCoroutine(DamageCoroutine());
    }
    
    private IEnumerator DamageCoroutine()
    {
        Image image = GetComponent<Image>();
        Color originalColor = image.color;
        image.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        image.color = originalColor;
    }
}