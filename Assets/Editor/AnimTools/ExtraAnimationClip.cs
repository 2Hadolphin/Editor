using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization.Editor;
using Sirenix.OdinInspector.Editor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class ExtraAnimationClip : OdinEditorWindow
{
    [AssetsOnly]
    public List<UnityEngine.Object> clipdata;
    public bool Filter_Humanoid;
    public bool KeepFolder = true;
    ExtraAnimationClip()
    {
        this.titleContent = new GUIContent("Extract HumanoidClip");
    }

    [MenuItem("Tools/Animation/ExtractClips")]
    static void Init()
    {
        var window = (ExtraAnimationClip)EditorWindow.GetWindow(typeof(ExtraAnimationClip));
        window.Show();


    }

    [FolderPath]
    public string mPath;

    [Button("Archive")]
    public void SaveFiles()
    {
        foreach (var data in clipdata)
        {
            var path = AssetDatabase.GetAssetPath(data);
            if (AssetDatabase.IsValidFolder(path))
            {
                SearchClipsInPack(data.name,path);
                continue;
            }


            var clips = AssetDatabase.LoadAllAssetsAtPath(path).Where(x=>x.GetType()==typeof(AnimationClip)).Where(x=>!x.name.Contains("preview")).Select(x=>x as AnimationClip).ToArray();
            
            foreach (var source in clips)
            {
                if (Filter_Humanoid)
                    if (!source.isHumanMotion)
                        continue;

                Debug.Log(source.name);
                var bindings = AnimationUtility.GetCurveBindings(source);
                var clip = new AnimationClip();

                foreach (var binding in bindings)
                {
                    var curve = AnimationUtility.GetEditorCurve(source, binding);
                    clip.SetCurve(binding.path, typeof(Animator), binding.propertyName, curve);
                }

                mEditorUtility.WriteFile(clip, string.IsNullOrEmpty(clip.name)?data.name:clip.name, mPath);
            }
        }

    }



    public void SearchClipsInPack(string folderName,string path)
    {
        var fbxs = mEditorUtility.GetAssetsAtPath<GameObject>(path);


        foreach (var data in fbxs)
        {
            path = AssetDatabase.GetAssetPath(data);
            var targetFolder = mPath;
            if (KeepFolder && fbxs.Length > 0)
            {
                targetFolder = Path.Combine(mPath, folderName);

                if (AssetDatabase.GetSubFolders(mPath).Where(x=>x.Contains(folderName)).Count()==0)
                    AssetDatabase.CreateFolder(mPath, folderName);
            }

            var clips = AssetDatabase.LoadAllAssetRepresentationsAtPath(path).Where(x => x.GetType() == typeof(AnimationClip)).Where(x => !x.name.Contains("preview")).Select(x => x as AnimationClip).ToArray();

            foreach (var source in clips)
            {
                if (Filter_Humanoid)
                    if (!source.isHumanMotion)
                        continue;

                var bindings = AnimationUtility.GetCurveBindings(source);
                var clip = new AnimationClip();

                foreach (var binding in bindings)
                {
                    var curve = AnimationUtility.GetEditorCurve(source, binding);
                    clip.SetCurve(binding.path, typeof(Animator), binding.propertyName, curve);
                }

                mEditorUtility.WriteFile(clip, string.IsNullOrEmpty(clip.name) ? data.name : clip.name, targetFolder);
            }
        }

    }



}
