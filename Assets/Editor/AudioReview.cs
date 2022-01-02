using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using System.Linq;
public class AudioReview : OdinEditorWindow
{
    const string EditorDataKey= "AudioReview";

    [MenuItem("Tools/AudioReview")]
    private static void OpenWindow()
    {
        var window = GetWindow<AudioReview>();

        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(700, 700);
    }

    

    [HorizontalGroup("Data")]
    [FolderPath][OnValueChanged("LoadFiles")]
    public string FolderPath;



    [HorizontalGroup("Data")]
    [Button("PingFolder")]
    public void Ping()
    {
        UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(FolderPath, typeof(UnityEngine.Object));
        EditorGUIUtility.PingObject(obj); 
    }

    [BoxGroup("Files")][EnumPaging]
    public AudioClip[] Clips;


    string[] GUIDs;
    public int PreviewNumbers = 20;


    int _index;
    int NextIndex
    {
        get
        {
            _index=GUIDs.Loop(_index+1);

            return _index;
        }
    }
    int LastIndex
    {
        get
        {
            _index = GUIDs.Loop(_index - 1);

            return _index;
        }
    }
    [PropertySpace]
    [HorizontalGroup("LoadFile")]
    [PropertyOrder(1.1f)]
    [Button("ArchiveFile",ButtonSizes.Large)]
    public void ArchiveData()
    {
        var list = Resources.Load<AudioList>(AudioList.ID);
        list.Archive(Archives.ToArray());
        Archives.Clear();
    }

    [PropertySpace]
    [HorizontalGroup("LoadFile")]
    [Button("UpdateFiles", ButtonSizes.Large)]
    [PropertyOrder(1.5f)]
    public void LoadFiles()
    {
        var folders= AssetDatabase.GetSubFolders("Assets");
        GUIDs = AssetDatabase.FindAssets("t:AudioClip", folders);
        var lastKey = Record;
        var length = GUIDs.Length;


        for (int i = 0; i < length; i++)
        {
            EditorUtility.DisplayProgressBar("Loading", "Checking last record", (float)i / length);
            if (GUIDs[i].Equals(lastKey))
            {
                if (EditorUtility.DisplayDialog("Load", "Whether loading data by last record ?", "Yes, contiune last record. ", "No, create new search. "))
                    _index = i;
                else
                    _index = GUIDs.Length;
            }
        }

        EditorUtility.ClearProgressBar();

        NextPage();
    }
    [PropertySpace]
    [HorizontalGroup("LoadFile")]
    [Button("NextPage",ButtonSizes.Large)]
    [PropertyOrder(2)]
    public void NextPage()
    {
        var audios = new Queue<AudioClip>(PreviewNumbers);
        var key = NextIndex;
        var first = key;


        for (int i = 0; i < PreviewNumbers; i++)
        {
            EditorUtility.DisplayProgressBar("Loading..", "Loading audio files", (float)i / PreviewNumbers);
            var path = AssetDatabase.GUIDToAssetPath(GUIDs[key]);
            var audio = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            audios.Enqueue(audio);
            key = NextIndex;

            if (key == first)
                break;

        }
        EditorUtility.ClearProgressBar();
        Clips = audios.ToArray();
        _select = Clips.Length;
        Record = GUIDs[first];
        PreviewNext();
    }
    [PropertySpace]
    [HorizontalGroup("LoadFile")]
    [Button("LastPage", ButtonSizes.Large)]
    [PropertyOrder(1)]
    public void LastPage()
    {
        var audios = new Queue<AudioClip>(PreviewNumbers);
        var key = LastIndex;
        var first = key;


        for (int i = 0; i < PreviewNumbers; i++)
        {
            EditorUtility.DisplayProgressBar("Loading..", "Loading audio files", (float)i / PreviewNumbers);
            var path = AssetDatabase.GUIDToAssetPath(GUIDs[key]);
            var audio = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            audios.Enqueue(audio);
            key = LastIndex;

            if (key == first)
                break;

        }
        EditorUtility.ClearProgressBar();
        Clips = audios.ToArray();
        _select = Clips.Length;
        Record = GUIDs[first];
        PreviewNext();
    }

    int _select;


    [PropertySpace]
    [HorizontalGroup("Control")]
    [Button("PreviewNext", ButtonSizes.Large)]
    [PropertyOrder(4)]
    public void PreviewNext()
    {
        _select = Clips.Loop(_select+1);
        PreviewAudio();
    }
    [PropertySpace]
    [HorizontalGroup("Control")]
    [Button("PreviewLast", ButtonSizes.Large)]
    [PropertyOrder(3)]
    public void PreviewLast()
    {
        _select = Clips.Loop(_select-1);
        PreviewAudio();
    }

    void PreviewAudio()
    {
        current = Clips[_select];
        AssetPreview.GetAssetPreview(current);
        Selection.activeObject = current;
    }
    AudioClip current;
    protected override void DrawEditors()
    {
        base.DrawEditors();
        if (!current)
            return;


        GUILayout.Label(current.name);
        GUILayout.Label(current.length.ToString());
        GUILayout.Label(current.frequency.ToString());


        EditorGUI.BeginDisabledGroup(Archives.Contains(current));
        if (GUILayout.Button("Archive"))
            Archives.Add(current);
        EditorGUI.EndDisabledGroup();


    }

    public HashSet<AudioClip> Archives=new HashSet<AudioClip>();

    public string Record
    {
        get => EditorPrefs.GetString(EditorDataKey);
        set => EditorPrefs.SetString(EditorDataKey,value);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

    }

    protected override void OnDestroy()
    {
        ArchiveData();

        base.OnDestroy();

    }
}

public static class m
{
    public static int Loop<T>(this T[] list, int sn)
    {
        var length = list.Length;
        if (length == 0)
            return -1;

        var remainder = sn % length;
        if (remainder < 0)
            return remainder + length;
        else
            return remainder;
    }
}