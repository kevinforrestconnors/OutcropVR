using UnityEngine;
using UnityEditor;
using System.Collections;

public class OnImport : AssetPostprocessor
{
    void OnPostprocessModel(GameObject g)
    {
        // Get the object name from the filename.  e.g. /Assets/PhotogrammetryModels/Rocas_100K.obj -> Rocas_100K
        string landscapeModel = MakeLandscape.modelName.Substring(MakeLandscape.modelName.LastIndexOf("/") + 1, MakeLandscape.modelName.IndexOf("."));
        string landscapeModel2 = LandscapePhotogrammetryModel.modelName.Substring(LandscapePhotogrammetryModel.modelName.LastIndexOf("/") + 1, LandscapePhotogrammetryModel.modelName.IndexOf("."));
        string photogrammetryModel = ConvertPhotogrammetryModel.photogrammetryModelName.Substring(ConvertPhotogrammetryModel.photogrammetryModelName.LastIndexOf("/") + 1, ConvertPhotogrammetryModel.photogrammetryModelName.IndexOf("."));
        string photogrammetryModel2 = LandscapePhotogrammetryModel.photogrammetryModelName.Substring(LandscapePhotogrammetryModel.photogrammetryModelName.LastIndexOf("/") + 1, LandscapePhotogrammetryModel.photogrammetryModelName.IndexOf("."));

        if (g.name.Equals(landscapeModel))
        {
            RotateModel(g);
            ApplyMeshColliderToMeshParts(g);
            MapTexturesToMeshParts(g, MakeLandscape.mapName);
        }

        if (g.name.Equals(landscapeModel2))
        {
            RotateModel(g);
            ApplyMeshColliderToMeshParts(g);
            MapTexturesToMeshParts(g, LandscapePhotogrammetryModel.mapName);
        }

        if (g.name.Equals(photogrammetryModel + "Scaled"))
        {
            RotateModel(g);
            ApplyMeshColliderToMeshParts(g);
            MapTexturesToMeshParts(g, ConvertPhotogrammetryModel.textureName);
        } 

        if (g.name.Equals(photogrammetryModel2 + "Scaled"))
        {
            RotateModel(g);
            ApplyMeshColliderToMeshParts(g);
            MapTexturesToMeshParts(g, LandscapePhotogrammetryModel.textureName);
        }
    }

    void RotateModel(GameObject g)
    {
        g.transform.Rotate(-90, 0, 180);
    }

    void ApplyMeshColliderToMeshParts(GameObject g)
    {
        
        Transform def = g.transform.GetChild(0);

        Transform hasChildren;
        try
        {
            hasChildren = def.GetChild(0);
        }
        catch
        {
            hasChildren = null;
        }

        if (!hasChildren)
        {
            def.gameObject.AddComponent<MeshCollider>();
        }
        else
        {
            foreach (Transform meshPart in def)
            {
                meshPart.gameObject.AddComponent<MeshCollider>();
            }
        }
        
    }

    void MapTexturesToMeshParts(GameObject g, string textureName)
    {
        Transform def = g.transform.GetChild(0);

        Material landscapeMaterial = new Material(Shader.Find("Standard"));
        Texture landscapeTex = (Texture)AssetDatabase.LoadAssetAtPath("Assets/" + textureName, typeof(Texture));
        landscapeMaterial.mainTexture = landscapeTex;
        UnityEngine.Debug.Log(textureName);
        AssetDatabase.CreateAsset(landscapeMaterial, "Assets/Materials/" + textureName.Substring(textureName.LastIndexOf("/") + 1, textureName.IndexOf("."))  + ".mat");
        AssetDatabase.Refresh();

        Transform hasChildren;
        try
        {
            hasChildren = def.GetChild(0);
        }
        catch
        {
            hasChildren = null;
        }

        if (!hasChildren)
        {
            Renderer rend = def.GetComponent<Renderer>();
            rend.material = landscapeMaterial;
        }
        else
        {
            foreach (Transform meshPart in def)
            {
                Renderer rend = meshPart.GetComponent<Renderer>();
                rend.material = landscapeMaterial;
            }
        }
    }
}