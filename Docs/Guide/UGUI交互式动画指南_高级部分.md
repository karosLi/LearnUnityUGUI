# UGUI 交互式动画指南 - 高级部分

## 高级交互动画技术

### 基于事件系统的高级交互动画

UGUI的事件系统允许我们创建丰富的交互式动画效果：

```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class AdvancedButtonAnimation : MonoBehaviour, 
    IPointerEnterHandler, IPointerExitHandler, 
    IPointerDownHandler, IPointerUpHandler,
    IPointerClickHandler
{
    [SerializeField] private RectTransform buttonRect;
    [SerializeField] private Image buttonImage;
    [SerializeField] private RectTransform iconRect;
    [SerializeField] private ParticleSystem clickParticles;
    
    [Header("交互动画设置")]
    [SerializeField] private float hoverGrowScale = 1.05f;
    [SerializeField] private float clickShrinkScale = 0.95f;
    [SerializeField] private float animationDuration = 0.2f;
    [SerializeField] private Vector2 iconHoverOffset = new Vector2(0, 5);
    [SerializeField] private float iconHoverRotation = 10f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("颜色变化")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(0.9f, 0.9f, 1f);
    [SerializeField] private Color pressedColor = new Color(0.7f, 0.7f, 0.9f);
    
    private Vector2 iconDefaultPos;
    private Quaternion iconDefaultRotation;
    private Coroutine currentAnimation;
    
    private void Awake()
    {
        if (buttonRect == null)
            buttonRect = GetComponent<RectTransform>();
        
        if (buttonImage == null)
            buttonImage = GetComponent<Image>();
        
        if (iconRect != null)
        {
            iconDefaultPos = iconRect.anchoredPosition;
            iconDefaultRotation = iconRect.rotation;
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        CancelCurrentAnimation();
        currentAnimation = StartCoroutine(AnimateButton(
            hoverGrowScale, 
            iconDefaultPos + iconHoverOffset, 
            Quaternion.Euler(0, 0, iconHoverRotation),
            hoverColor
        ));
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        CancelCurrentAnimation();
        currentAnimation = StartCoroutine(AnimateButton(
            1f, 
            iconDefaultPos, 
            iconDefaultRotation,
            normalColor
        ));
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        CancelCurrentAnimation();
        currentAnimation = StartCoroutine(AnimateButton(
            clickShrinkScale, 
            iconDefaultPos - iconHoverOffset * 0.5f, 
            Quaternion.Euler(0, 0, -iconHoverRotation * 0.5f),
            pressedColor
        ));
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (RectTransformUtility.RectangleContainsScreenPoint(buttonRect, eventData.position, eventData.pressEventCamera))
        {
            OnPointerEnter(eventData);
        }
        else
        {
            OnPointerExit(eventData);
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // 触发点击特效
        if (clickParticles != null)
        {
            clickParticles.Stop();
            clickParticles.Play();
        }
        
        // 添加点击波纹效果
        StartCoroutine(CreateClickRipple(eventData.position));
    }
    
    private void CancelCurrentAnimation()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }
    }
    
    private IEnumerator AnimateButton(float targetScale, Vector2 targetIconPos, Quaternion targetIconRot, Color targetColor)
    {
        float startScale = buttonRect.localScale.x;
        Vector2 startIconPos = iconRect != null ? iconRect.anchoredPosition : Vector2.zero;
        Quaternion startIconRot = iconRect != null ? iconRect.rotation : Quaternion.identity;
        Color startColor = buttonImage != null ? buttonImage.color : Color.white;
        
        float startTime = Time.unscaledTime;
        
        while (Time.unscaledTime < startTime + animationDuration)
        {
            float t = (Time.unscaledTime - startTime) / animationDuration;
            float curvedT = animationCurve.Evaluate(t);
            
            // 应用缩放
            float currentScale = Mathf.Lerp(startScale, targetScale, curvedT);
            buttonRect.localScale = new Vector3(currentScale, currentScale, 1f);
            
            // 应用图标动画
            if (iconRect != null)
            {
                iconRect.anchoredPosition = Vector2.Lerp(startIconPos, targetIconPos, curvedT);
                iconRect.rotation = Quaternion.Slerp(startIconRot, targetIconRot, curvedT);
            }
            
            // 应用颜色变化
            if (buttonImage != null)
            {
                buttonImage.color = Color.Lerp(startColor, targetColor, curvedT);
            }
            
            yield return null;
        }
        
        // 确保最终状态
        buttonRect.localScale = new Vector3(targetScale, targetScale, 1f);
        if (iconRect != null)
        {
            iconRect.anchoredPosition = targetIconPos;
            iconRect.rotation = targetIconRot;
        }
        if (buttonImage != null)
        {
            buttonImage.color = targetColor;
        }
        
        currentAnimation = null;
    }
    
    private IEnumerator CreateClickRipple(Vector2 clickPosition)
    {
        // 创建一个圆形图像作为涟漪效果
        GameObject rippleObj = new GameObject("ClickRipple");
        rippleObj.transform.SetParent(transform);
        rippleObj.transform.position = clickPosition;
        rippleObj.transform.localScale = Vector3.zero;
        
        Image rippleImage = rippleObj.AddComponent<Image>();
        rippleImage.sprite = Resources.Load<Sprite>("UI/CircleMask");
        rippleImage.color = new Color(1f, 1f, 1f, 0.3f);
        
        RectTransform rippleRect = rippleObj.GetComponent<RectTransform>();
        rippleRect.sizeDelta = new Vector2(buttonRect.rect.width * 2, buttonRect.rect.width * 2);
        
        float rippleTime = 0.6f;
        float startTime = Time.unscaledTime;
        
        while (Time.unscaledTime < startTime + rippleTime)
        {
            float t = (Time.unscaledTime - startTime) / rippleTime;
            
            // 缩放和淡出效果
            float scale = Mathf.Lerp(0f, 1f, t);
            float alpha = Mathf.Lerp(0.3f, 0f, t);
            
            rippleRect.localScale = new Vector3(scale, scale, 1f);
            rippleImage.color = new Color(1f, 1f, 1f, alpha);
            
            yield return null;
        }
        
        Destroy(rippleObj);
    }
}
```

### 序列动画系统

创建UI序列动画系统，用于控制多个UI元素按顺序动画：

```csharp
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UISequenceAnimator : MonoBehaviour
{
    [System.Serializable]
    public class AnimationItem
    {
        public GameObject target;
        public AnimationType animationType = AnimationType.Scale;
        public float delay = 0.1f;
        public float duration = 0.3f;
        public Vector3 fromScale = new Vector3(0.5f, 0.5f, 1f);
        public Vector3 toScale = Vector3.one;
        public Vector2 fromPosition = new Vector2(50f, 0f);
        public Vector2 toPosition = Vector2.zero;
        public float fromAlpha = 0f;
        public float toAlpha = 1f;
        public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [HideInInspector] public RectTransform rectTransform;
        [HideInInspector] public CanvasGroup canvasGroup;
    }
    
    public enum AnimationType
    {
        Scale,
        Position,
        Alpha,
        Combined
    }
    
    [SerializeField] private List<AnimationItem> animationSequence = new List<AnimationItem>();
    [SerializeField] private bool playOnStart = false;
    [SerializeField] private bool reverseOnHide = true;
    
    private List<Coroutine> activeAnimations = new List<Coroutine>();
    
    private void Start()
    {
        // 初始化引用
        foreach (var item in animationSequence)
        {
            if (item.target != null)
            {
                item.rectTransform = item.target.GetComponent<RectTransform>();
                item.canvasGroup = item.target.GetComponent<CanvasGroup>();
                
                // 如果需要Alpha动画但没有CanvasGroup，则添加
                if ((item.animationType == AnimationType.Alpha || 
                     item.animationType == AnimationType.Combined) && 
                    item.canvasGroup == null)
                {
                    item.canvasGroup = item.target.AddComponent<CanvasGroup>();
                }
            }
        }
        
        // 设置初始状态
        foreach (var item in animationSequence)
        {
            SetInitialState(item);
        }
        
        if (playOnStart)
        {
            Show();
        }
    }
    
    public void Show()
    {
        StopAllAnimations();
        
        float totalDelay = 0f;
        
        foreach (var item in animationSequence)
        {
            if (item.target != null)
            {
                item.target.SetActive(true);
                
                Coroutine animCoroutine = StartCoroutine(AnimateItem(item, totalDelay, false));
                activeAnimations.Add(animCoroutine);
                
                totalDelay += item.delay;
            }
        }
    }
    
    public void Hide()
    {
        StopAllAnimations();
        
        float totalDelay = 0f;
        List<AnimationItem> sequence = new List<AnimationItem>(animationSequence);
        
        if (reverseOnHide)
        {
            sequence.Reverse();
        }
        
        foreach (var item in sequence)
        {
            if (item.target != null)
            {
                Coroutine animCoroutine = StartCoroutine(AnimateItem(item, totalDelay, true, 
                    () => item.target.SetActive(false)));
                activeAnimations.Add(animCoroutine);
                
                totalDelay += item.delay;
            }
        }
    }
    
    private void StopAllAnimations()
    {
        foreach (var coroutine in activeAnimations)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }
        
        activeAnimations.Clear();
    }
    
    private void SetInitialState(AnimationItem item)
    {
        if (item.target == null || item.rectTransform == null)
            return;
        
        switch (item.animationType)
        {
            case AnimationType.Scale:
                item.rectTransform.localScale = item.fromScale;
                break;
                
            case AnimationType.Position:
                item.rectTransform.anchoredPosition = item.fromPosition;
                break;
                
            case AnimationType.Alpha:
                if (item.canvasGroup != null)
                    item.canvasGroup.alpha = item.fromAlpha;
                break;
                
            case AnimationType.Combined:
                item.rectTransform.localScale = item.fromScale;
                item.rectTransform.anchoredPosition = item.fromPosition;
                if (item.canvasGroup != null)
                    item.canvasGroup.alpha = item.fromAlpha;
                break;
        }
    }
    
    private IEnumerator AnimateItem(AnimationItem item, float delay, bool reverse, System.Action onComplete = null)
    {
        if (item.target == null || item.rectTransform == null)
            yield break;
        
        if (delay > 0)
            yield return new WaitForSecondsRealtime(delay);
        
        float startTime = Time.unscaledTime;
        
        // 设置初始值和目标值
        Vector3 startScale = reverse ? item.toScale : item.fromScale;
        Vector3 targetScale = reverse ? item.fromScale : item.toScale;
        
        Vector2 startPosition = reverse ? item.toPosition : item.fromPosition;
        Vector2 targetPosition = reverse ? item.fromPosition : item.toPosition;
        
        float startAlpha = reverse ? item.toAlpha : item.fromAlpha;
        float targetAlpha = reverse ? item.fromAlpha : item.toAlpha;
        
        while (Time.unscaledTime < startTime + item.duration)
        {
            float t = (Time.unscaledTime - startTime) / item.duration;
            float curvedT = item.animationCurve.Evaluate(t);
            
            switch (item.animationType)
            {
                case AnimationType.Scale:
                    item.rectTransform.localScale = Vector3.Lerp(startScale, targetScale, curvedT);
                    break;
                    
                case AnimationType.Position:
                    item.rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, curvedT);
                    break;
                    
                case AnimationType.Alpha:
                    if (item.canvasGroup != null)
                        item.canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, curvedT);
                    break;
                    
                case AnimationType.Combined:
                    item.rectTransform.localScale = Vector3.Lerp(startScale, targetScale, curvedT);
                    item.rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, curvedT);
                    if (item.canvasGroup != null)
                        item.canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, curvedT);
                    break;
            }
            
            yield return null;
        }
        
        // 确保最终状态准确
        switch (item.animationType)
        {
            case AnimationType.Scale:
                item.rectTransform.localScale = targetScale;
                break;
                
            case AnimationType.Position:
                item.rectTransform.anchoredPosition = targetPosition;
                break;
                
            case AnimationType.Alpha:
                if (item.canvasGroup != null)
                    item.canvasGroup.alpha = targetAlpha;
                break;
                
            case AnimationType.Combined:
                item.rectTransform.localScale = targetScale;
                item.rectTransform.anchoredPosition = targetPosition;
                if (item.canvasGroup != null)
                    item.canvasGroup.alpha = targetAlpha;
                break;
        }
        
        if (onComplete != null)
            onComplete();
    }
}
```

### 基于ShaderGraph的高级UI动画效果

使用Shader实现高级UI视觉效果和动画：

```csharp
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class UIShaderAnimationController : MonoBehaviour
{
    [SerializeField] private Material customMaterial;
    [SerializeField] private float animationSpeed = 1f;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float glowIntensity = 1.5f;
    [SerializeField] private Color glowColor = new Color(0f, 0.7f, 1f);
    
    private Image targetImage;
    private Material instanceMaterial;
    private float animationTime = 0f;
    
    private void Awake()
    {
        targetImage = GetComponent<Image>();
        
        if (customMaterial != null)
        {
            // 创建材质实例以避免共享材质的修改影响其他对象
            instanceMaterial = new Material(customMaterial);
            targetImage.material = instanceMaterial;
            
            // 设置初始属性
            instanceMaterial.SetColor("_GlowColor", glowColor);
            instanceMaterial.SetFloat("_GlowIntensity", 0f);
        }
    }
    
    private void Update()
    {
        if (instanceMaterial != null)
        {
            // 更新材质动画时间
            animationTime += Time.deltaTime * animationSpeed;
            instanceMaterial.SetFloat("_AnimationTime", animationTime);
            
            // 脉冲光晕效果
            float pulseValue = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f * glowIntensity;
            instanceMaterial.SetFloat("_GlowIntensity", pulseValue);
        }
    }
    
    public void SetHighlight(bool isHighlighted)
    {
        if (instanceMaterial != null)
        {
            StartCoroutine(AnimateHighlight(isHighlighted));
        }
    }
    
    private IEnumerator AnimateHighlight(bool highlightOn)
    {
        float startValue = instanceMaterial.GetFloat("_HighlightAmount");
        float targetValue = highlightOn ? 1f : 0f;
        float duration = 0.25f;
        float startTime = Time.time;
        
        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            float smoothT = t * t * (3f - 2f * t); // 平滑过渡
            
            float currentValue = Mathf.Lerp(startValue, targetValue, smoothT);
            instanceMaterial.SetFloat("_HighlightAmount", currentValue);
            
            yield return null;
        }
        
        instanceMaterial.SetFloat("_HighlightAmount", targetValue);
    }
}
```

### 动画状态机管理器

创建一个用于复杂UI动画状态管理的状态机系统：

```csharp
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class UIAnimationStateMachine : MonoBehaviour
{
    [System.Serializable]
    public class AnimationState
    {
        public string stateName;
        public List<AnimationStep> animationSteps = new List<AnimationStep>();
        public UnityEvent onStateEnter;
        public UnityEvent onStateExit;
    }
    
    [System.Serializable]
    public class AnimationStep
    {
        public GameObject target;
        [Tooltip("动画持续时间")]
        public float duration = 0.3f;
        [Tooltip("此步骤开始前的延迟")]
        public float delay = 0f;
        [Tooltip("动画曲线")]
        public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("变换属性")]
        public bool animatePosition = false;
        public Vector2 targetPosition;
        
        public bool animateScale = false;
        public Vector3 targetScale = Vector3.one;
        
        public bool animateRotation = false;
        public Vector3 targetRotation;
        
        public bool animateAlpha = false;
        public float targetAlpha = 1f;
    }
    
    [SerializeField] private List<AnimationState> states = new List<AnimationState>();
    [SerializeField] private string initialState;
    [SerializeField] private bool playInitialStateOnStart = true;
    
    private Dictionary<string, AnimationState> stateMap = new Dictionary<string, AnimationState>();
    private string currentState;
    private Coroutine currentAnimation;
    
    private void Awake()
    {
        // 构建状态映射
        foreach (var state in states)
        {
            if (!string.IsNullOrEmpty(state.stateName))
            {
                stateMap[state.stateName] = state;
            }
        }
    }
    
    private void Start()
    {
        if (playInitialStateOnStart && !string.IsNullOrEmpty(initialState))
        {
            PlayState(initialState);
        }
    }
    
    public void PlayState(string stateName)
    {
        if (string.IsNullOrEmpty(stateName) || !stateMap.ContainsKey(stateName))
        {
            Debug.LogWarning($"尝试播放不存在的状态: {stateName}");
            return;
        }
        
        // 停止当前动画
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }
        
        // 调用当前状态的退出事件
        if (!string.IsNullOrEmpty(currentState) && stateMap.ContainsKey(currentState))
        {
            stateMap[currentState].onStateExit?.Invoke();
        }
        
        // 更新当前状态
        string previousState = currentState;
        currentState = stateName;
        
        // 调用新状态的进入事件
        stateMap[currentState].onStateEnter?.Invoke();
        
        // 开始播放新状态的动画
        currentAnimation = StartCoroutine(PlayStateAnimation(stateMap[currentState]));
    }
    
    private IEnumerator PlayStateAnimation(AnimationState state)
    {
        // 存储每个对象的初始状态
        Dictionary<GameObject, Vector2> initialPositions = new Dictionary<GameObject, Vector2>();
        Dictionary<GameObject, Vector3> initialScales = new Dictionary<GameObject, Vector3>();
        Dictionary<GameObject, Quaternion> initialRotations = new Dictionary<GameObject, Quaternion>();
        Dictionary<GameObject, float> initialAlphas = new Dictionary<GameObject, float>();
        
        // 收集所有需要动画的目标对象
        HashSet<GameObject> allTargets = new HashSet<GameObject>();
        foreach (var step in state.animationSteps)
        {
            if (step.target != null)
            {
                allTargets.Add(step.target);
            }
        }
        
        // 存储初始状态
        foreach (GameObject target in allTargets)
        {
            RectTransform rectTransform = target.GetComponent<RectTransform>();
            CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
            
            if (rectTransform != null)
            {
                initialPositions[target] = rectTransform.anchoredPosition;
                initialScales[target] = rectTransform.localScale;
                initialRotations[target] = rectTransform.rotation;
            }
            
            if (canvasGroup != null)
            {
                initialAlphas[target] = canvasGroup.alpha;
            }
        }
        
        // 按顺序执行每个动画步骤
        foreach (var step in state.animationSteps)
        {
            if (step.target == null)
                continue;
            
            // 添加延迟
            if (step.delay > 0)
                yield return new WaitForSecondsRealtime(step.delay);
            
            RectTransform rectTransform = step.target.GetComponent<RectTransform>();
            CanvasGroup canvasGroup = step.target.GetComponent<CanvasGroup>();
            
            // 确保我们有需要的组件
            if (step.animateAlpha && canvasGroup == null)
            {
                canvasGroup = step.target.AddComponent<CanvasGroup>();
                initialAlphas[step.target] = canvasGroup.alpha;
            }
            
            float startTime = Time.unscaledTime;
            
            // 获取初始值
            Vector2 startPos = rectTransform.anchoredPosition;
            Vector3 startScale = rectTransform.localScale;
            Quaternion startRot = rectTransform.rotation;
            float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;
            
            // 动画循环
            while (Time.unscaledTime < startTime + step.duration)
            {
                float t = (Time.unscaledTime - startTime) / step.duration;
                float curvedT = step.curve.Evaluate(t);
                
                // 应用各种动画属性
                if (step.animatePosition)
                {
                    rectTransform.anchoredPosition = Vector2.Lerp(startPos, step.targetPosition, curvedT);
                }
                
                if (step.animateScale)
                {
                    rectTransform.localScale = Vector3.Lerp(startScale, step.targetScale, curvedT);
                }
                
                if (step.animateRotation)
                {
                    rectTransform.rotation = Quaternion.Slerp(
                        startRot, 
                        Quaternion.Euler(step.targetRotation), 
                        curvedT
                    );
                }
                
                if (step.animateAlpha && canvasGroup != null)
                {
                    canvasGroup.alpha = Mathf.Lerp(startAlpha, step.targetAlpha, curvedT);
                }
                
                yield return null;
            }
            
            // 确保最终状态是准确的
            if (step.animatePosition)
            {
                rectTransform.anchoredPosition = step.targetPosition;
            }
            
            if (step.animateScale)
            {
                rectTransform.localScale = step.targetScale;
            }
            
            if (step.animateRotation)
            {
                rectTransform.rotation = Quaternion.Euler(step.targetRotation);
            }
            
            if (step.animateAlpha && canvasGroup != null)
            {
                canvasGroup.alpha = step.targetAlpha;
            }
        }
        
        currentAnimation = null;
    }
    
    public string GetCurrentState()
    {
        return currentState;
    }
    
    public bool IsAnimating()
    {
        return currentAnimation != null;
    }
} 