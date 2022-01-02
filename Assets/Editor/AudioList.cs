using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = AudioList.ID, menuName ="CreateAudioArchives")]
[System.Serializable]
public class AudioList : ScriptableObject
{
    [ShowInInspector]
    public List<AudioClip> Clips=new List<AudioClip>();
    public const string ID= "AudioArchives";
    public void Archive(AudioClip[] clips)
    {
        foreach (var clip in clips)
        {
            if (!Clips.Contains(clip))
                Clips.Add(clip);
        }

        EditorUtility.SetDirty(this);

    }

    [Button("ExportPackage")]
    public void Export()
    {

        var exportedPackageAssetList = new List<string>();
        var length = Clips.Count;

        for (int i = 0; i < length; i++)
        {
            EditorUtility.DisplayProgressBar("ExportPackage", string.Format("Packing {0}", Clips[i].name),(float)i/length);
            var guid = AssetDatabase.GetAssetPath(Clips[i]);
            exportedPackageAssetList.Add(guid);
        }

        EditorUtility.ClearProgressBar();

        AssetDatabase.ExportPackage(exportedPackageAssetList.ToArray(), "Assets/AudioPackage.unitypackage", ExportPackageOptions.Recurse);
    }
}
