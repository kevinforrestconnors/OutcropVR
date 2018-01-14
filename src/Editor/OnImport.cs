using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

public class OnImport : AssetPostprocessor
{
    void OnPostprocessModel(GameObject g)
    {

		// Get the object name from the filename.  e.g. /PhotogrammetryModels/Rocas_100K.obj -> Rocas_100K
		int land1sp = MakeLandscape.modelName.LastIndexOf ("/") + 1;
		int land2sp = LandscapePhotogrammetryModel.modelName.LastIndexOf ("/") + 1;
		int photogrammetry1sp = ConvertPhotogrammetryModel.photogrammetryModelName.LastIndexOf ("/") + 1;
		int photogrammetry2sp = LandscapePhotogrammetryModel.photogrammetryModelName.LastIndexOf ("/") + 1;

		string landscapeModel, landscapeModel2, photogrammetryModel, photogrammetryModel2;

		try {
			landscapeModel = MakeLandscape.modelName.Substring(land1sp, MakeLandscape.modelName.IndexOf(".") - land1sp);
			landscapeModel2 = LandscapePhotogrammetryModel.modelName.Substring(land2sp, LandscapePhotogrammetryModel.modelName.IndexOf(".") - land2sp);
			photogrammetryModel = ConvertPhotogrammetryModel.photogrammetryModelName.Substring(photogrammetry1sp, ConvertPhotogrammetryModel.photogrammetryModelName.IndexOf(".") - photogrammetry1sp);
			photogrammetryModel2 = LandscapePhotogrammetryModel.photogrammetryModelName.Substring(photogrammetry2sp, LandscapePhotogrammetryModel.photogrammetryModelName.IndexOf(".") - photogrammetry2sp);
		} catch (ArgumentOutOfRangeException) {
			throw new Exception ("OnImportError: Invalid model name.");
		}


        if (g.name.Equals(landscapeModel))
        {
            RotateModel(g);
            ApplyMeshColliderToMeshParts(g);
            MapTexturesToMeshParts(g, MakeLandscape.mapName);
            g.tag = "Landscape";
        }

        if (g.name.Equals(landscapeModel2))
        {
            RotateModel(g);
            ApplyMeshColliderToMeshParts(g);
            MapTexturesToMeshParts(g, LandscapePhotogrammetryModel.mapName);
            g.tag = "Landscape";
        }

        if (g.name.Equals(photogrammetryModel + "Scaled"))
        {
            RotateModel(g);
            ApplyMeshColliderToMeshParts(g);
            MapTexturesToMeshParts(g, ConvertPhotogrammetryModel.textureName);
            g.tag = "Photogrammetry Model";
        } 

        if (g.name.Equals(photogrammetryModel2 + "Scaled"))
        {
            RotateModel(g);
            ApplyMeshColliderToMeshParts(g);
            MapTexturesToMeshParts(g, LandscapePhotogrammetryModel.textureName);
            g.tag = "Photogrammetry Model";
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
		int startPos = textureName.LastIndexOf ("/") + 1;
		int elems = textureName.LastIndexOf (".") - startPos;

		try {
			AssetDatabase.CreateAsset(landscapeMaterial, "Assets/Materials/" + textureName.Substring(startPos, elems)  + ".mat");
		} catch (ArgumentOutOfRangeException) {
			throw new Exception ("OnImportError: Invalid texture name.");
		}

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