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
    
    private Image buttonImage;
    private bool isPressed = false;
    private bool wasInRange = false;
    private Transform[] fingerTips;
    
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
        GameObject[] hands = new GameObject[] 
        {
            GameObject.Find("/[CameraRig]/SGHand Left"),
            GameObject.Find("/[CameraRig]/SGHand Right")
        };
        
        System.Collections.Generic.List<Transform> tips = new System.Collections.Generic.List<Transform>();
        
        foreach (GameObject hand in hands)
        {
            if (hand != null)
            {
                Transform feedbackLayer = hand.transform.Find("Feedback Layer");
                if (feedbackLayer != null)
                {
                    Transform indexFFB = feedbackLayer.Find("Index_FFB");
                    if (indexFFB != null)
                    {
                        tips.Add(indexFFB);
                    }
                }
            }
        }
        
        fingerTips = tips.ToArray();
        
        if (fingerTips.Length == 0)
        {
            Debug.LogWarning("[VRButtonInteraction] No finger tips found. Make sure SGHand objects exist in scene.");
        }
    }
    
    void Update()
    {
        if (targetButton == null || fingerTips == null || fingerTips.Length == 0)
            return;
        
        bool inRange = IsFingerNearButton(out float closestDistance);
        
        if (inRange && !wasInRange)
        {
            OnHoverEnter();
        }
        else if (!inRange && wasInRange)
        {
            OnHoverExit();
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
        
        Debug.Log($"[VRButtonInteraction] Hovering over {gameObject.name}");
    }
    
    void OnHoverExit()
    {
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
