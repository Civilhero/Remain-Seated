using System.Collections.Generic;
using UnityEngine;

public class BoundaryBoxBehavior : MonoBehaviour
{
    // Inspector Fields
    public GameObject boundaryContainer;

    public List<GameObject> boundaryObjects = new List<GameObject>();
    public List<Collider> boundaries = new List<Collider>();

    // MonoBehaviour Methods
    void Awake()
    {
        if (boundaryContainer == null) { return; }
        foreach (Transform child in boundaryContainer.transform)
        {
            boundaryObjects.Add(child.gameObject);
            Collider collider = child.GetComponent<Collider>();
            if (collider != null)
            {
                boundaries.Add(collider);
            }
        }
    }


    // Functions
    public List<GameObject> getOverlappingObjects(LayerMask? layerMask, string tag)
    {
        
        List<GameObject> overlappingObjects = new List<GameObject>();
        foreach (Collider collider in boundaries)
        {
            Collider[] overlaps = Physics.OverlapBox(
                collider.bounds.center,
                collider.bounds.extents,
                Quaternion.identity,
                layerMask ?? Physics.AllLayers
            );

            foreach (Collider overlap in overlaps)
            {
                if (!boundaries.Contains(overlap) && !overlappingObjects.Contains(overlap.gameObject))
                {
                    if (string.IsNullOrEmpty(tag) || overlap.CompareTag(tag))
                    {
                        overlappingObjects.Add(overlap.gameObject);
                    }
                }
            }
        }

        return overlappingObjects;
    }

    public void DisableBoundaries()
    {
        foreach (Collider collider in boundaries)
        {
            collider.enabled = false;
        }
    }
}