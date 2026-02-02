using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class VRButtonInteraction : MonoBehaviour
{
    [Header("Button Settings")]
    public Button targetButton;
    public float triggerDistance = 0.05f;
    
    [Header("Visual Feedback")]
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(0.96f, 0.96f, 0.96f);
    public Color pressColor = new Color(0.78f, 0.78f, 0.78f);
    
    [Header("Haptic Feedback")]
    public bool useHapticFeedback = true;
    
    [Header("Hover Hold Settings")]
    public float hoverHoldDuration = 5f;
    
    private Image buttonImage;
    private bool isPressed = false;
    private bool wasInRange = false;
    private Transform[] fingerTips;
    private float hoverTimer = 0f;
    private bool hasTriggeredOnHover = false;
    
    void Start()
    {
        if (targetButton == null)
        {
            targetButton = GetComponent<Button>();
        }
        
        if (targetButton != null)
        {
            buttonImage = targetButton.GetComponent<Image>();
        }
        
        FindFingerTips();
    }
    
    void FindFingerTips()
    {
        System.Collections.Generic.List<Transform> tips = new System.Collections.Generic.List<Transform>();
        
        // Find hands by searching for SG_TrackedHand components
        SG.SG_TrackedHand[] trackedHands = Object.FindObjectsOfType<SG.SG_TrackedHand>();
        
        Debug.Log($"[VRButtonInteraction] Found {trackedHands.Length} SG_TrackedHand components in scene");
        
        foreach (SG.SG_TrackedHand trackedHand in trackedHands)
        {
            GameObject hand = trackedHand.gameObject;
            Debug.Log($"[VRButtonInteraction] Processing hand: {hand.name} (active: {hand.activeInHierarchy})");
            Debug.Log($"[VRButtonInteraction] Full path: {GetGameObjectPath(hand)}");
            
            // Try multiple possible structures
            Transform fingerTip = null;
            
            // Method 1: Look for Feedback Layer > Index_FFB
            Transform feedbackLayer = hand.transform.Find("Feedback Layer");
            if (feedbackLayer != null)
            {
                fingerTip = feedbackLayer.Find("Index_FFB");
                if (fingerTip != null)
                {
                    Debug.Log($"[VRButtonInteraction] ✓ Found Index_FFB via Feedback Layer in {hand.name}");
                }
                else
                {
                    Debug.Log($"[VRButtonInteraction] Children of Feedback Layer: {ListChildren(feedbackLayer)}");
                }
            }
            
            // Method 2: Look directly for Index_FFB child
            if (fingerTip == null)
            {
                fingerTip = hand.transform.Find("Index_FFB");
                if (fingerTip != null)
                {
                    Debug.Log($"[VRButtonInteraction] ✓ Found Index_FFB directly under {hand.name}");
                }
            }
            
            // Method 3: Search recursively for Index_FFB
            if (fingerTip == null)
            {
                fingerTip = FindChildRecursive(hand.transform, "Index_FFB");
                if (fingerTip != null)
                {
                    Debug.Log($"[VRButtonInteraction] ✓ Found Index_FFB recursively in {hand.name}");
                }
            }
            
            // Method 4: Look for any finger tip (Index finger)
            if (fingerTip == null)
            {
                fingerTip = FindChildRecursive(hand.transform, "Index");
                if (fingerTip != null)
                {
                    Debug.Log($"[VRButtonInteraction] ✓ Found Index finger in {hand.name}: {fingerTip.name}");
                }
            }
            
            if (fingerTip != null)
            {
                tips.Add(fingerTip);
                Debug.Log($"[VRButtonInteraction] ✓ Added {fingerTip.name} from {hand.name} at world position {fingerTip.position}");
            }
            else
            {
                Debug.LogWarning($"[VRButtonInteraction] Could not find finger tip in {hand.name}");
                LogHierarchy(hand.transform, 0);
            }
        }
        
        fingerTips = tips.ToArray();
        
        if (fingerTips.Length == 0)
        {
            Debug.LogWarning("[VRButtonInteraction] No finger tips found. Will retry in 2 seconds.");
        }
        else
        {
            Debug.Log($"[VRButtonInteraction] ✓ Successfully found {fingerTips.Length} finger tip(s) for button: {gameObject.name}");
        }
    }
    
    Transform FindChildRecursive(Transform parent, string childName)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name.Contains(childName))
            {
                return child;
            }
            Transform result = FindChildRecursive(child, childName);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }
    
    void LogHierarchy(Transform parent, int indent)
    {
        string indentStr = new string(' ', indent * 2);
        Debug.Log($"{indentStr}{parent.name}");
        for (int i = 0; i < parent.childCount; i++)
        {
            LogHierarchy(parent.GetChild(i), indent + 1);
        }
    }
    
    string ListChildren(Transform parent)
    {
        string result = "";
        for (int i = 0; i < parent.childCount; i++)
        {
            if (i > 0) result += ", ";
            result += parent.GetChild(i).name;
        }
        return result;
    }
    
    string GetGameObjectPath(GameObject obj)
    {
        string path = "/" + obj.name;
        Transform current = obj.transform.parent;
        while (current != null)
        {
            path = "/" + current.name + path;
            current = current.parent;
        }
        return path;
    }
    
    void Update()
    {
        if (targetButton == null || fingerTips == null || fingerTips.Length == 0)
        {
            // Only retry finding finger tips every 2 seconds to avoid spam
            if (Time.frameCount % 120 == 0)
            {
                FindFingerTips();
            }
            return;
        }
        
        bool inRange = IsFingerNearButton(out float closestDistance);
        
        if (inRange && !wasInRange)
        {
            OnHoverEnter();
        }
        else if (!inRange && wasInRange)
        {
            OnHoverExit();
        }
        
        // Update hover timer
        if (inRange && !hasTriggeredOnHover)
        {
            hoverTimer += Time.deltaTime;
            if (hoverTimer >= hoverHoldDuration)
            {
                OnHoverHoldComplete();
            }
        }
        else if (!inRange)
        {
            hoverTimer = 0f;
        }
        
        if (inRange && closestDistance < triggerDistance && !isPressed)
        {
            OnButtonPress();
        }
        else if ((!inRange || closestDistance >= triggerDistance) && isPressed)
        {
            OnButtonRelease();
        }
        
        wasInRange = inRange;
    }
    
    bool IsFingerNearButton(out float closestDistance)
    {
        closestDistance = float.MaxValue;
        Vector3 buttonPosition = transform.position;
        float checkDistance = triggerDistance * 3f;
        
        foreach (Transform fingerTip in fingerTips)
        {
            if (fingerTip == null) continue;
            
            float distance = Vector3.Distance(fingerTip.position, buttonPosition);
            
            if (distance < closestDistance)
            {
                closestDistance = distance;
            }
        }
        
        return closestDistance < checkDistance;
    }
    
    void OnHoverEnter()
    {
        if (buttonImage != null && targetButton.interactable)
        {
            buttonImage.color = hoverColor;
        }
        
        hoverTimer = 0f;
        hasTriggeredOnHover = false;
        Debug.Log($"[VRButtonInteraction] Hovering over {gameObject.name}. Hold for {hoverHoldDuration} seconds to activate.");
    }
    
    void OnHoverHoldComplete()
    {
        if (!targetButton.interactable) return;
        
        hasTriggeredOnHover = true;
        targetButton.onClick.Invoke();
        Debug.Log($"[VRButtonInteraction] Button {gameObject.name} triggered after {hoverHoldDuration}s hold!");
    }
    
    void OnHoverExit()
    {
        hoverTimer = 0f;
        hasTriggeredOnHover = false;
        
        if (buttonImage != null && !isPressed)
        {
            buttonImage.color = normalColor;
        }
    }
    
    void OnButtonPress()
    {
        if (!targetButton.interactable) return;
        
        isPressed = true;
        
        if (buttonImage != null)
        {
            buttonImage.color = pressColor;
        }
        
        targetButton.onClick.Invoke();
        
        Debug.Log($"[VRButtonInteraction] Button {gameObject.name} pressed!");
    }
    
    void OnButtonRelease()
    {
        isPressed = false;
        
        if (buttonImage != null)
        {
            buttonImage.color = wasInRange ? hoverColor : normalColor;
        }
    }
}
