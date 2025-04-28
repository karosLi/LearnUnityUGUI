# UGUI 交互式动画示例

## 示例案例

### 1. 主菜单按钮过渡动画

```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class MenuButtonAnimation : MonoBehaviour, 
    IPointerEnterHandler, IPointerExitHandler, 
    IPointerClickHandler
{
    [SerializeField] private RectTransform buttonRect;
    [SerializeField] private Image buttonImage;
    [SerializeField] private Text buttonText;
    [SerializeField] private RectTransform iconRect;
    
    [Header("动画参数")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float clickScale = 0.9f;
    [SerializeField] private float animDuration = 0.2f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(0.8f, 0.9f, 1f);
    [SerializeField] private Vector2 iconOffset = new Vector2(5f, 0f);
    
    private Coroutine currentAnimation;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
        
        currentAnimation = StartCoroutine(AnimateButton(hoverScale, hoverColor, iconOffset));
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
        
        currentAnimation = StartCoroutine(AnimateButton(1f, normalColor, Vector2.zero));
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
        
        StartCoroutine(ClickAnimation());
    }
    
    private IEnumerator AnimateButton(float targetScale, Color targetColor, Vector2 iconTargetOffset)
    {
        float startTime = Time.unscaledTime;
        float startScale = buttonRect.localScale.x;
        Color startColor = buttonImage.color;
        Vector2 startIconPos = iconRect.anchoredPosition;
        Vector2 targetIconPos = iconRect.anchoredPosition + iconTargetOffset;
        
        while (Time.unscaledTime < startTime + animDuration)
        {
            float t = (Time.unscaledTime - startTime) / animDuration;
            t = t * t * (3f - 2f * t); // 平滑插值
            
            buttonRect.localScale = Vector3.one * Mathf.Lerp(startScale, targetScale, t);
            buttonImage.color = Color.Lerp(startColor, targetColor, t);
            
            if (iconRect != null)
                iconRect.anchoredPosition = Vector2.Lerp(startIconPos, targetIconPos, t);
            
            yield return null;
        }
        
        buttonRect.localScale = Vector3.one * targetScale;
        buttonImage.color = targetColor;
        
        if (iconRect != null)
            iconRect.anchoredPosition = targetIconPos;
        
        currentAnimation = null;
    }
    
    private IEnumerator ClickAnimation()
    {
        // 点击时的快速缩小
        float startTime = Time.unscaledTime;
        float startScale = buttonRect.localScale.x;
        
        while (Time.unscaledTime < startTime + animDuration * 0.3f)
        {
            float t = (Time.unscaledTime - startTime) / (animDuration * 0.3f);
            buttonRect.localScale = Vector3.one * Mathf.Lerp(startScale, clickScale, t);
            yield return null;
        }
        
        // 恢复到悬停状态
        startTime = Time.unscaledTime;
        startScale = buttonRect.localScale.x;
        
        while (Time.unscaledTime < startTime + animDuration * 0.5f)
        {
            float t = (Time.unscaledTime - startTime) / (animDuration * 0.5f);
            buttonRect.localScale = Vector3.one * Mathf.Lerp(startScale, hoverScale, t);
            yield return null;
        }
        
        buttonRect.localScale = Vector3.one * hoverScale;
        currentAnimation = null;
    }
}
```

### 2. 弹出窗口动画

```csharp
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PopupWindowAnimation : MonoBehaviour
{
    [SerializeField] private CanvasGroup backgroundOverlay;
    [SerializeField] private RectTransform windowRect;
    [SerializeField] private float animDuration = 0.3f;
    [SerializeField] private AnimationCurve animCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    public void Show()
    {
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(ShowAnimation());
    }
    
    public void Hide()
    {
        StopAllCoroutines();
        StartCoroutine(HideAnimation());
    }
    
    private IEnumerator ShowAnimation()
    {
        // 设置初始状态
        backgroundOverlay.alpha = 0f;
        windowRect.localScale = Vector3.zero;
        
        float startTime = Time.unscaledTime;
        
        while (Time.unscaledTime < startTime + animDuration)
        {
            float t = (Time.unscaledTime - startTime) / animDuration;
            float curvedT = animCurve.Evaluate(t);
            
            // 应用动画
            backgroundOverlay.alpha = Mathf.Lerp(0f, 0.5f, curvedT);
            windowRect.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, curvedT);
            
            yield return null;
        }
        
        // 确保最终状态
        backgroundOverlay.alpha = 0.5f;
        windowRect.localScale = Vector3.one;
    }
    
    private IEnumerator HideAnimation()
    {
        float startTime = Time.unscaledTime;
        
        while (Time.unscaledTime < startTime + animDuration)
        {
            float t = (Time.unscaledTime - startTime) / animDuration;
            float curvedT = animCurve.Evaluate(t);
            
            // 应用动画
            backgroundOverlay.alpha = Mathf.Lerp(0.5f, 0f, curvedT);
            windowRect.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, curvedT);
            
            yield return null;
        }
        
        // 确保最终状态
        backgroundOverlay.alpha = 0f;
        windowRect.localScale = Vector3.zero;
        
        gameObject.SetActive(false);
    }
}
```

### 3. 进度条填充动画

```csharp
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AnimatedProgressBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Text percentText;
    [SerializeField] private float animDuration = 1f;
    [SerializeField] private AnimationCurve fillCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    private float currentValue = 0f;
    private Coroutine fillAnimation;
    
    public void SetProgress(float newValue)
    {
        newValue = Mathf.Clamp01(newValue);
        
        if (fillAnimation != null)
            StopCoroutine(fillAnimation);
        
        fillAnimation = StartCoroutine(AnimateFill(currentValue, newValue));
        currentValue = newValue;
    }
    
    private IEnumerator AnimateFill(float fromValue, float toValue)
    {
        float startTime = Time.time;
        
        while (Time.time < startTime + animDuration)
        {
            float t = (Time.time - startTime) / animDuration;
            float curvedT = fillCurve.Evaluate(t);
            
            float currentFill = Mathf.Lerp(fromValue, toValue, curvedT);
            
            // 更新填充图像
            fillImage.fillAmount = currentFill;
            
            // 更新百分比文本
            if (percentText != null)
                percentText.text = Mathf.RoundToInt(currentFill * 100) + "%";
            
            yield return null;
        }
        
        // 确保最终值准确
        fillImage.fillAmount = toValue;
        if (percentText != null)
            percentText.text = Mathf.RoundToInt(toValue * 100) + "%";
        
        fillAnimation = null;
    }
}
```

### 4. 列表项滚动动画

```csharp
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AnimatedScrollList : MonoBehaviour
{
    [SerializeField] private RectTransform contentRect;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private float itemHeight = 60f;
    [SerializeField] private float itemSpacing = 10f;
    [SerializeField] private float animDelay = 0.05f;
    [SerializeField] private float animDuration = 0.3f;
    
    private List<RectTransform> items = new List<RectTransform>();
    
    public void PopulateList(int itemCount)
    {
        // 清除现有项
        ClearList();
        
        // 设置内容区域大小
        float contentHeight = itemCount * (itemHeight + itemSpacing) - itemSpacing;
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, contentHeight);
        
        // 创建新项
        for (int i = 0; i < itemCount; i++)
        {
            GameObject newItem = Instantiate(itemPrefab, contentRect);
            RectTransform itemRect = newItem.GetComponent<RectTransform>();
            items.Add(itemRect);
            
            // 设置位置
            Vector2 position = new Vector2(0f, -i * (itemHeight + itemSpacing) - itemHeight / 2f);
            itemRect.anchoredPosition = position;
            
            // 准备动画
            CanvasGroup canvasGroup = newItem.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = newItem.AddComponent<CanvasGroup>();
            
            // 初始透明度为0，缩放为0.5
            canvasGroup.alpha = 0f;
            itemRect.localScale = new Vector3(0.5f, 0.5f, 1f);
            
            // 开始动画
            StartCoroutine(AnimateItemIn(itemRect, canvasGroup, i * animDelay));
        }
    }
    
    private void ClearList()
    {
        foreach (var item in items)
        {
            if (item != null)
                Destroy(item.gameObject);
        }
        
        items.Clear();
    }
    
    private IEnumerator AnimateItemIn(RectTransform itemRect, CanvasGroup canvasGroup, float delay)
    {
        if (delay > 0)
            yield return new WaitForSeconds(delay);
        
        float startTime = Time.time;
        
        while (Time.time < startTime + animDuration)
        {
            float t = (Time.time - startTime) / animDuration;
            float smoothT = t * t * (3f - 2f * t); // 平滑插值
            
            // 应用动画
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, smoothT);
            itemRect.localScale = Vector3.Lerp(
                new Vector3(0.5f, 0.5f, 1f),
                Vector3.one,
                smoothT
            );
            
            yield return null;
        }
        
        // 确保最终状态
        canvasGroup.alpha = 1f;
        itemRect.localScale = Vector3.one;
    }
}
```

### 5. 循环呼吸动画按钮

```csharp
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PulsingButton : MonoBehaviour
{
    [SerializeField] private RectTransform buttonRect;
    [SerializeField] private Image buttonImage;
    [SerializeField] private Image glowImage;
    
    [Header("呼吸效果")]
    [SerializeField] private float minScale = 0.95f;
    [SerializeField] private float maxScale = 1.05f;
    [SerializeField] private float pulseDuration = 2f;
    [SerializeField] private Color normalColor = new Color(0.8f, 0.8f, 1f);
    [SerializeField] private Color pulseColor = new Color(1f, 1f, 1f);
    [SerializeField] private float minGlowAlpha = 0.3f;
    [SerializeField] private float maxGlowAlpha = 0.8f;
    
    private void Start()
    {
        // 启动呼吸动画
        StartCoroutine(PulseAnimation());
    }
    
    private IEnumerator PulseAnimation()
    {
        while (true)
        {
            // 膨胀阶段
            yield return AnimatePulse(minScale, maxScale, normalColor, pulseColor, minGlowAlpha, maxGlowAlpha);
            
            // 收缩阶段
            yield return AnimatePulse(maxScale, minScale, pulseColor, normalColor, maxGlowAlpha, minGlowAlpha);
        }
    }
    
    private IEnumerator AnimatePulse(
        float fromScale, float toScale, 
        Color fromColor, Color toColor,
        float fromGlowAlpha, float toGlowAlpha)
    {
        float startTime = Time.time;
        float halfDuration = pulseDuration * 0.5f;
        
        while (Time.time < startTime + halfDuration)
        {
            float t = (Time.time - startTime) / halfDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            // 应用缩放
            float currentScale = Mathf.Lerp(fromScale, toScale, smoothT);
            buttonRect.localScale = new Vector3(currentScale, currentScale, 1f);
            
            // 应用颜色变化
            buttonImage.color = Color.Lerp(fromColor, toColor, smoothT);
            
            // 应用光晕效果
            if (glowImage != null)
            {
                Color glowColor = glowImage.color;
                glowColor.a = Mathf.Lerp(fromGlowAlpha, toGlowAlpha, smoothT);
                glowImage.color = glowColor;
            }
            
            yield return null;
        }
        
        // 确保最终状态
        buttonRect.localScale = new Vector3(toScale, toScale, 1f);
        buttonImage.color = toColor;
        
        if (glowImage != null)
        {
            Color glowColor = glowImage.color;
            glowColor.a = toGlowAlpha;
            glowImage.color = glowColor;
        }
    }
}
``` 