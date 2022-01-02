using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(IconMaker))]
public class IconMakerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        IconMaker maker = (IconMaker)target;

        if (DrawDefaultInspector())
        {
            if (maker.CamUpdate)
            {
                maker.SettingCam();
            }
        }

        if(GUILayout.Button("Generate Icon"))
        {
            if (maker.IconSource)
            {
               File.WriteAllBytes("Assets/OutputCenter/" + "Icon_" + maker.IconSource.name + ".png", maker.MakeIcon());
                // AssetDatabase.LoadAssetAtPath<Sprite>("Assets/OutputCenter/" + "Icon_" + maker.IconSource.name + ".png");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                TextureImporter importer = AssetImporter.GetAtPath("Assets/OutputCenter/" + "Icon_" + maker.IconSource.name + ".png") as TextureImporter;

               if (importer)
               {
                    Debug.Log("Found");
                    AssetDatabase.LoadAssetAtPath<Texture>("Assets/OutputCenter/" + "Icon_" + maker.IconSource.name + ".png");
                    //importer.spritePixelsPerUnit = sp.pixelsPerUnit;
                    importer.mipmapEnabled = false;
                    EditorUtility.SetDirty(importer);
                    importer.textureType = TextureImporterType.Sprite;
                    importer.SaveAndReimport();

               }
                Debug.Log("Create");

                maker.AttachIcon(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/OutputCenter/" + "Icon_" + maker.IconSource.name + ".png"));
            }

            
        }

        if (GUILayout.Button("ResetPosition"))
        {
            maker.ResetPosition();
        }
    }
}



