using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using System.Text;
using System.Linq;
public class ExportManager : OdinEditorWindow
{
    [MenuItem("Tools/ExportManager")]
    private static void OpenWindow()
    {
        var window = GetWindow<ExportManager>();

        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(700, 700);
    }


    public List<UnityEngine.Object> Targets;
    public ExportPackageOptions Option=ExportPackageOptions.Recurse;
    [FolderPath]
    public string Path= "Assets";
    public string FileName= "NewPackage";
    [HideInInspector]
    public bool CombineFolder = false;

    [ShowIf("CombineFolder")][FolderPath]
    public string CombineFolderPath;

    [Button("ExportPackage")]
    public void Export()
    {
        var exportedPackageAssetList = new List<string>();
        var length = Targets.Count;

        Dictionary<UnityEngine.Object, string> d=default;

        if (CombineFolder)
        {
            d = Targets.ToDictionary(x => x, x => AssetDatabase.GetAssetPath(x));

            if (!AssetDatabase.IsValidFolder(CombineFolderPath))
            {
                //var folders = CombineFolderPath.Split('/');
                EditorUtility.DisplayDialog("Error", CombineFolderPath + " is not vaild folder","Okay");
                return;
            }

            foreach (var target in d)
            {
                AssetDatabase.MoveAsset(target.Value, CombineFolderPath+"/"+target.Key.name+".asset");
            }
            exportedPackageAssetList.Add(CombineFolderPath);
            AssetDatabase.Refresh();
        }
        else
        {
            for (int i = 0; i < length; i++)
            {
                EditorUtility.DisplayProgressBar("ExportPackage", string.Format("Packing {0}", Targets[i].name), (float)i / length);
                var guid = AssetDatabase.GetAssetPath(Targets[i]);
                exportedPackageAssetList.Add(guid);
            }

        }


        EditorUtility.ClearProgressBar();

        AssetDatabase.ExportPackage(exportedPackageAssetList.ToArray(), Path+"/"+ FileName+".unitypackage", Option);


        if (CombineFolder)
        {
            var obs = AssetDatabase.LoadAllAssetsAtPath(CombineFolderPath);
            foreach (var item in obs)
            {
                if(d.TryGetValue(item,out var oldPath))
                {
                    var newPath = AssetDatabase.GetAssetPath(item);
                    AssetDatabase.MoveAsset(newPath, oldPath);
                }
                else
                {
                    Debug.LogError(item+" missing old path !");
                }
            }
        }

        //AssetDatabase.Refresh();
    }
}

