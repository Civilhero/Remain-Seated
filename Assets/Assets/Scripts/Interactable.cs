using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [Header("Interactable Event")]
    public UnityEvent onInteract;
    
    public void Interact()
    {
        onInteract.Invoke();
    }
    
}
