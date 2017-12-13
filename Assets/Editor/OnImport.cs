using UnityEngine;
using UnityEditor;


public class OnImport : AssetPostprocessor
{
    void OnPostprocessModel(GameObject g)
    {
        if (g.name.Contains("Scaled")) {
            g.transform.Rotate(-90, 0, 180);
            Apply(g.transform);
        }
        
    }

    void Apply(Transform t)
    {
        g.AddComponent<MeshCollider>();
        // Recurse
        foreach (Transform child in t)
            Apply(child);
    }
}