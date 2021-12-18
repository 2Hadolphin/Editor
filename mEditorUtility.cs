#if UNITY_EDITOR

using UnityEngine;
using System.IO;
using UnityEditor;
using System;
using System.Collections;
using Object = UnityEngine.Object;
using System.Linq;
public static class mEditorUtility
{
    public const string RootPath = "Assets";
    public static bool SplitEditorTargetPath(string target, out string path, out string fileName)
    {
        var chars = target.Split('/');
        var length = chars.Length;
        var _path = chars[0];
        for (int i = 1; i < length; i++)
        {
            if (AssetDatabase.IsValidFolder(_path))
            {
                Debug.Log(_path);
                _path += chars[i];
            }
            else
            {
                fileName = chars[i - 1];
                path = _path.Replace(chars[i - 1], string.Empty);
                return true;
            }
        }

        path = target;
        fileName = string.Empty;
        return false;
    }

    public static T[] GetAssetsAtPath<T>(string path, bool inculdeChildFolder = false) where T : Object
    {

        EditorUtility.DisplayProgressBar("Loading", "Loading " + typeof(T) + " files..", 0);
        var fileEntries = GetSubFiles(path, true);
        EditorUtility.ClearProgressBar();
        ArrayList al = new ArrayList();

        foreach (string filePath in fileEntries)
        {
            Object t = AssetDatabase.LoadAssetAtPath<T>(filePath);
            if (t != null)
                al.Add(t);
        }
        T[] result = new T[al.Count];
        for (int i = 0; i < al.Count; i++)
            result[i] = (T)al[i];

        return result;
    }

    public static string[] GetSubFiles(string path, bool inculdeChildFolder)
    {
        var root = RootPath;
        path = path.Substring(path.IndexOf(root));

        if (path.Contains(root + "/"))
            path = path.Replace(root + "/", string.Empty);
        else
            path = path.Replace(root, string.Empty);


        path = Application.dataPath + "/" + path;

        var fileEntries = Directory.GetFiles(path).ToList();

        if (inculdeChildFolder)
        {
            var subFolders = Directory.GetDirectories(path);
            foreach (var folder in subFolders)
            {
                fileEntries.AddRange(GetSubFiles(folder, inculdeChildFolder));
            }
        }

        var length = fileEntries.Count;
        for (var i = 0; i < length; i++)
        {
            var filePath = fileEntries[i];
            int assetPathIndex = filePath.IndexOf(RootPath);
            fileEntries[i] = filePath.Substring(assetPathIndex);
        }

        return fileEntries.ToArray();
    }
    public static void Rename(UnityEngine.Object @object, string newID)
    {
        string assetPath = AssetDatabase.GetAssetPath(@object.GetInstanceID());
        AssetDatabase.RenameAsset(assetPath, newID);
        AssetDatabase.SaveAssets();
    }
    public static void PopMessage(string title,string msg,string okay)
    {
        EditorUtility.DisplayDialog(title, msg, okay);
    }
    public static void OpenFile<T>(ref string path, ref T field) where T : UnityEngine.Object
    {
        path = EditorUtility.OpenFilePanel("Assets path" + typeof(T).ToString(), "Assets", "");
        //path = EditorUtility.OpenFolderPanel("Assets path" + typeof(T).ToString(), "Assets", "");

        if (path.Contains(Application.dataPath))
        {
            path = "Assets" + path.Substring(Application.dataPath.Length);
            field = AssetDatabase.LoadAssetAtPath(path, typeof(T)) as T;
        }
    }

    public static string ChoseFolder(string path=null)
    {
        if(string.IsNullOrEmpty(path)||!AssetDatabase.IsValidFolder(path))
        path = EditorUtility.OpenFolderPanel("Selected Folder", "Assets", "");


        if (path.Contains(Application.dataPath))
        {
            path = "Assets" + path.Substring(Application.dataPath.Length);
        }

        return path;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="file"></param>
    /// <param name="path">Must inculde file name and assets type (etc example.asset)</param>
    /// <returns></returns>
    public static T WriteFile<T>(T file,string fileName=null, string path = null)where T: UnityEngine.Object
    {
#if UNITY_EDITOR
        var type = file.GetType();

        if (string.IsNullOrEmpty(fileName))
        {
            var originPath = AssetDatabase.GetAssetPath(file);
            var split = originPath.LastIndexOf('/');
            if (split > 0)
                fileName = originPath.Substring(split);

            if (string.IsNullOrEmpty(fileName))
                fileName = file.name;

            fileName += DateTime.Now+type.ToString();
        }

        if (string.IsNullOrEmpty(path))
        {
            SafeCreateDirectory("Assets/TempArchiveFolder");
            path = "Assets/TempArchiveFolder/";//string.Format({0}{1:yyyy_MM_dd_HH_mm_ss}_{2}", fileName, , type.ToString());
        }

        if(AssetDatabase.IsValidFolder(path))
            path = Path.Combine(path,fileName) + ".asset";
        else
        {
            throw new IOException(path);
        }

        var uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath(path);

        var log = string.Format("Archive one {0} file at {1}", type.ToString(), path);
        //EditorGUILayout.HelpBox(log, MessageType.Info, false);
        AssetDatabase.CreateAsset(file, uniqueAssetPath);
        AssetDatabase.Refresh();

        EditorGUIUtility.PingObject(file);

       return  AssetDatabase.LoadAssetAtPath<T>(uniqueAssetPath);
#endif
    }

    public static void WriteFile(UnityEngine.Object file,Type type=null,string path=null)
    {
#if UNITY_EDITOR

        if (string.IsNullOrEmpty(path))
        {
            SafeCreateDirectory("Assets/TempArchiveFolder");
            path = string.Format("Assets/TempArchiveFolder/{0}{1:yyyy_MM_dd_HH_mm_ss}{2}.asset", file.ToString(), DateTime.Now, type.ToString());
        }
  

        var uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath(path);

        var log = string.Format("Archive one {0} file at {1}", type.ToString(), path);
        EditorGUILayout.HelpBox(log,MessageType.Info,false);
        AssetDatabase.CreateAsset(file, uniqueAssetPath);
        AssetDatabase.Refresh();

        EditorGUIUtility.PingObject(file);
#endif
    }

    public static void WriteAnimationFile(AnimationClip humanPose)
    {
#if UNITY_EDITOR
        SafeCreateDirectory("Assets/Resources");

        var path = string.Format("Assets/Resources/RecordMotion_{0}{1:yyyy_MM_dd_HH_mm_ss}.asset", humanPose.name, DateTime.Now);

        var uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath(path);


        AssetDatabase.CreateAsset(humanPose, uniqueAssetPath);
        AssetDatabase.Refresh();
#endif
    }
    public static DirectoryInfo SafeCreateDirectory(string path)
    {
        return Directory.Exists(path) ? null : Directory.CreateDirectory(path);
    }

    public static T EnumToButton<T>(this T @enum,GUILayoutOption GUILayoutOption)where T:Enum
    {
        var type = @enum.GetType();
        var enums = Enum.GetValues(type);
        foreach (var value in enums)
        {
            if (@enum.Equals(value))
            {
                var style = new GUIStyle(GUI.skin.button);
                //style.normal.textColor = Color.black;
                style.fontStyle = FontStyle.Bold;
                GUILayout.Button(value.ToString(), style, GUILayoutOption);
            }
            else
            {
                if (GUILayout.Button(value.ToString(), GUILayoutOption))
                {
                    return (T)value;
                }
            }

        }
        return @enum;
    }

    public static GUILayoutOption MiddleLittleButton(int numbers)
    {
        var width = EditorGUIUtility.currentViewWidth/numbers;
        width = width < EditorGUIUtility.labelWidth ? EditorGUIUtility.labelWidth : width;
        return GUILayout.Width(width);
    }


}
#endif