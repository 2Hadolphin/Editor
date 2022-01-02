using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Return.Humanoid;
using System.Linq;
using Return.Helper;
public class EditorWindow_AvatarMaker : EditorWindow
{
    protected Editor window;

    public EditorWindow_AvatarMaker()
    {
        this.titleContent = new GUIContent("AvatarMaker");

    }
    [MenuItem("Tools/Animation/Avatar/AvatarMaker")]
    public static void OpenWindow()
    {
        var window=CreateWindow<EditorWindow_AvatarMaker>();
        window.Show();
        
    }

    private void OnEnable()
    {
        window = Editor.CreateEditor(AvatarMaker.Instance);
    }

    private void OnDisable()
    {
        var avatarMaker=(AvatarMaker)window.target;
        DestroyImmediate(avatarMaker);
    }

    private void OnGUI()
    {
        window.OnInspectorGUI();
    }
}

public class AvatarMaker : ScriptableObject
{
    static Transform _Root;
    static string[] _AllBoneNames;
    static Transform[] _AllBoneTransform;
    public Transform Root 
    { 
        get { return _Root; }
        set
        { 
            if(_Root!=value)
            {
                BindingData.Clear();
                SkeletonMap = SkeletonMap.BuildSkeketonBase;
            }
            _Root = value;
        } 
    }
    public string[] AllBoneName 
    {
        get 
        {
            if (_AllBoneNames is null)
                _AllBoneNames = mAnimationUtility.AllHumanBodyBoneNamesInTitlecaseLetter;

            return _AllBoneNames;
        } 
    }

   public Transform[] AllBoneTransform
    {
        get
        {
            if(_AllBoneTransform is null)
                _AllBoneTransform=_Root.GetComponentsInChildren<Transform>();

            return _AllBoneTransform;
        }
    }

    public static AvatarMaker Instance
    {
        get
        {
            var _Instance = CreateInstance<AvatarMaker>();
            // Not visible to the user and not save
            //_Instance.hideFlags = HideFlags.HideAndDontSave;
            _Instance.SkeletonMap = SkeletonMap.BuildSkeketonBase;
            return _Instance;
        }
    }

    Dictionary<HumanBodyBones, MapData> BindingData = new Dictionary<HumanBodyBones, MapData>();
    struct MapData
    {
        public int Score;
        public Transform Bone;
    }
    public SkeletonMap SkeletonMap;
    public void ScanBone()
    {
       var sb = new System.Text.StringBuilder();

        var tfs = AllBoneTransform;
        var sn = 0;
        foreach (var tf in tfs)
        {
            EditorUtility.DisplayProgressBar("Scanning", "Searching for skeleton bones",(float)sn / tfs.Length);
            if (!TryBindBone(tf))
            {
                SkeletonMap.ExtraBoneMap.SafeAdd(tf, -1);
                sb.AppendLine(tf.name);
            }
            sn++;
        }


        EditorUtility.ClearProgressBar();

        if (!string.IsNullOrEmpty(sb.ToString()))
            Debug.LogError("The following transforms can't been reference to skeleton data via human name. \n"+sb.ToString());
    }

    static string[] LoadKeyWord(string name)
    {
        var tags = Text.Depart(name, Text.TitlecaseLetter).Select(x => x.ToUpper()).ToList();
        var length = tags.Count;

        for (int i = 0; i < length; i++)
        {
            switch (tags[i])
            {
                case "L":
                    tags[i] = "LEFT";
                    break;               
                case "R":
                    tags[i] = "RIGHT";
                    break;              
                case "U":
                    tags[i] = "UPPER";
                    break;            
                case "D":
                    tags[i] = "DOWN";
                    break;
                case "LOW":
                    tags[i] = "LOWER";
                    break;
                default:
                    if (tags[i].Length == 1)
                        tags[i] = string.Empty;
                    break;
            }
        }



        return tags.Where(x=>!string.Empty.Equals(x)).ToArray();
    }

    protected bool IsFinger(Transform tf)
    {
        var parent = tf.parent;
        while (parent != null)
        {
            if (parent.name.ToUpper().Contains("HAND"))
                return true;

            parent = parent.parent;
        }
        return false;
    }


    protected bool SetFinger(Transform tf)
    {
        int HumanSN = 0;
        var parent = tf.parent;
        var index = 0;
        while (parent != null)
        {
            if (parent.name.ToUpper().Contains("HAND"))
            {
                var name = parent.name.ToUpper();

                var dot = Vector3.Dot(Root.right, parent.position - Root.position);
                if (dot > 0)
                    HumanSN = (int)HumanBodyBones.RightThumbProximal;
                else
                    HumanSN = (int)HumanBodyBones.LeftThumbProximal;

                break;
            }
            index++;
            if (index > 3)
            {
                Debug.Log(tf.name);
                return false;
            }

            parent = parent.parent;
        }

  

        if (HumanSN==0)
            return false;


        var fingerName = tf.name.ToLower();

        if (fingerName.Contains("thumb"))
            HumanSN += 0;
        else if (fingerName.Contains("index"))
            HumanSN += 3;
        else if (fingerName.Contains("middle"))
            HumanSN += 6;
        else if (fingerName.Contains("ring"))
            HumanSN += 9;
        else if (fingerName.Contains("little"))
            HumanSN += 12;

        HumanSN += index;
        Debug.Log(tf.name + " : " + (HumanBodyBones)HumanSN);
        SkeletonMap.BoneTransform.Add((HumanBodyBones)HumanSN, tf);

        return true;
    }

    public bool TryBindBone(Transform tf)
    {
        //Check is finger

        if (tf.name.ToUpper().Contains("END"))
            return false;

        if (IsFinger(tf))
            return SetFinger(tf);

        //departstring
        var tags = LoadKeyWord(tf.name);
        var bonesName = AllBoneName;
        var count = tags.Length;

        var score = 0;
        var index = 0;
        var length = bonesName.Length;
        var results = new List<possibleResult>();

        for (int i = 0; i < length; i++)
        {
            var mScore = 0;
            var bone = bonesName[i];
            for (int w = 0; w < count; w++)
            {
                if (tags[w].Length < 3)
                    continue;
                if (bone.Contains(tags[w])) 
                    mScore++;
                //mScore +=bone.CompareTo(tags[w]);
            }

            if (mScore > score)
            {
                index = i;
                score = mScore;
                results.Add(new possibleResult() { HumanBodyBone = (HumanBodyBones)i, Score = score }) ;
            }
        }


            var sb = new System.Text.StringBuilder();

        foreach (var result in results)
        {
            sb.AppendLine("Match Bone : "+result.HumanBodyBone + "=>" + result.Score);
        }
        sb.AppendLine("Tags : ");
            foreach (var text in tags)
            {
                sb.Append(text);
                sb.Append('-');
            }
            Debug.Log(sb.ToString());



        if(score==0)
            return false;

        results.Reverse();
        BindBone(tf,results.ToArray());
        return true;
    }
    public struct possibleResult
    {
        public HumanBodyBones HumanBodyBone;
        public int Score;
    }
    public void BindBone(Transform tf, possibleResult[] results)
    {

        foreach (var result in results)
        {
            if(!BindingData.TryGetValue(result.HumanBodyBone,out var targetData))
            {
                SkeletonMap.BoneTransform.Add(result.HumanBodyBone, tf); 
                BindingData.SafeAdd(result.HumanBodyBone, new MapData() { Bone = tf, Score = result.Score });
                return;
            }
            else
            {
                if (targetData.Bone.Equals(tf))
                    return;

                if (targetData.Score > result.Score)
                    continue;
                else
                {
                    Debug.LogError(string.Format(" Overwrite  {0} -- {1} with {2}",result.HumanBodyBone,targetData.Bone.name,tf.name ));
                    SkeletonMap.ExtraBoneMap.SafeAdd(targetData.Bone, -1);
                    SkeletonMap.BoneTransform.Add(result.HumanBodyBone, tf);
                    BindingData.SafeAdd(result.HumanBodyBone, new MapData() { Bone = tf, Score = result.Score });

                    return;
                }
            }
        }
        var sb = new System.Text.StringBuilder();
        foreach (var result in results)
        {
            sb.Append(result.HumanBodyBone.ToString() + " : " + result.Score + " lower than " + BindingData[result.HumanBodyBone].Score+'\n');
        }
        Debug.LogError("Reference bone fail : " + tf+sb.ToString());
        //Bind To Extra

        SkeletonMap.ExtraBoneMap.SafeAdd(tf, -1);

    }
    public static void MakeAvatarMask()
    {
        GameObject activeGameObject = Selection.activeGameObject;

        if (activeGameObject != null)
        {
            AvatarMask avatarMask = new AvatarMask();

            avatarMask.AddTransformPath(activeGameObject.transform);

            var path = string.Format("Assets/{0}.mask", activeGameObject.name.Replace(':', '_'));
            AssetDatabase.CreateAsset(avatarMask, path);
        }
    }

    public static void MakeAvatar()
    {
        GameObject activeGameObject = Selection.activeGameObject;

        if (activeGameObject != null)
        {
            Avatar avatar = AvatarBuilder.BuildHumanAvatar(activeGameObject, new HumanDescription() { });
            avatar.name = activeGameObject.name;
            Debug.Log(avatar.isHuman ? "is human" : "is generic");

            var path = string.Format("Assets/{0}.ht", avatar.name.Replace(':', '_'));
            AssetDatabase.CreateAsset(avatar, path);
        }
    }
}
[CustomEditor(typeof(AvatarMaker))]
public class Editor_Avatar:Editor
{
    public const string DataPath = "Assets/HumanoidAnimation/Skeleton";
    public enum BuildFuncion { KeyIn,AutoDetect,OfficalForm}
    static BuildFuncion buildFuncion=BuildFuncion.AutoDetect;
    //static Transform root;
    static string path=null;
    static mHumanoidDescription mDescription;
    static Vector2 Scroll;
    static string KeyWord;
    
    public override void OnInspectorGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.Space();

        ChoseFunction();



        EditorGUILayout.Space();

        switch (buildFuncion)
        {
            case BuildFuncion.KeyIn:
                EditorGUILayout.Space();
                GUILayout.Label(buildFuncion.ToString()+" : Mapping data via bone name");
                EditorGUILayout.Space();
                KeyIn();
                return;
            case BuildFuncion.AutoDetect:
                AutoDetect();
                return;
            case BuildFuncion.OfficalForm:
                OfficalForm();
                return;
        }
 
    }

    void ChoseFunction()
    {

        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
        EditorGUILayout.Space();

        buildFuncion = buildFuncion.EnumToButton(mEditorUtility.MiddleLittleButton(System.Enum.GetValues(typeof(BuildFuncion)).Length+1));
        EditorGUILayout.Space();
        EditorGUILayout.EndHorizontal();
    }

    void KeyIn()
    {
        RootPort();

        var maker = target as AvatarMaker;

        var dictionary = maker.SkeletonMap.BoneTransform;
        var map = maker.SkeletonMap.BoneTransform.Forward.keyValuePairs;

        if (!maker.Root)
            return;
        var tfs = maker.Root.GetComponentsInChildren<Transform>();
        /*
        Debug.Log(dictionary);
        Debug.Log(dictionary.Forward);
        Debug.Log(dictionary.Forward.keyValuePairs);
        return;



        Debug.Log(avatarMaker);     
        Debug.Log(avatarMaker.GetInstanceID());
        Debug.Log(avatarMaker.Skeleton);
        Debug.Log(avatarMaker.Skeleton.BoneTransform);
        Debug.Log(avatarMaker.Skeleton.BoneIndex);
        if (boneMap == null)
            return;
        */
        EditorGUILayout.BeginVertical(GUILayout.Height(400));
        Scroll =EditorGUILayout.BeginScrollView(Scroll);
        var length = map.Length;
        var catchColor = EditorStyles.objectField.normal.textColor;
        var errorColor = Color.red;
        for (int i = 0; i < length; i++)
        {
            var value = map[i].Value;


            if(map[i].Value==null)
                EditorStyles.objectField.normal.textColor = errorColor;
            else
                EditorStyles.objectField.normal.textColor = catchColor;

            var newValue = EditorGUILayout.ObjectField(map[i].Key.ToString(), value, typeof(Transform), true) as Transform;


            if (value==newValue)
                continue;
           
            Debug.Log(value + "-" + newValue);

            dictionary.Edit(map[i].Key, value, newValue);
        }

        EditorStyles.objectField.normal.textColor = catchColor;
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        GUILayout.BeginHorizontal();
        KeyWord=EditorGUILayout.TextField(KeyWord);
        GUILayout.Label("EnterKeyWord");
        GUILayout.EndHorizontal();

        GUILayout.BeginVertical(GUILayout.Height(200));

        if (!string.IsNullOrEmpty(KeyWord))
        {
            Transform[] matchTransforms = tfs.Where(x => x.name.ToUpper().Contains(KeyWord.ToUpper())).ToArray();
            foreach (var tf in matchTransforms)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(tf, typeof(Transform), true);
                if (GUILayout.Button("KeyIn"))
                {
                    if (maker.TryBindBone(tf))
                        EditorGUILayout.HelpBox("Success bind " + tf+".", MessageType.Info);
                    else
                        EditorGUILayout.HelpBox("Fail to bind " + tf + ", please drag it manual !",MessageType.Error);
                }
                GUILayout.EndHorizontal();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Enter bone name to quick search !", MessageType.Info);
        }

        GUILayout.EndVertical();

        if (GUILayout.Button("Scan bone", mEditorUtility.MiddleLittleButton(1)))
            maker.ScanBone();
        
        bool Ready2Build = maker.Root != null & maker.SkeletonMap.IsReady;

        if(Ready2Build)
        {
            if(GUILayout.Button("NormalizeBones"))
            {
                NormalizeBones();
            }
        }

        EditorGUI.BeginDisabledGroup(!Ready2Build);
        if (GUILayout.Button("GenerateAvatar"))
        {
            mDescription = mHumanoidDescription.Create(maker.SkeletonMap.BoneTransform);

            var avatar = mDescription.CreateAvatar(maker.Root,maker.SkeletonMap);
            mEditorUtility.WriteFile(avatar, avatar.GetType(), mEditorUtility.ChoseFolder(DataPath) + '/' + maker.Root.name+"M" + ".avatar.asset");

            avatar = mDescription.CreateAvatar(maker.Root);
            mEditorUtility.WriteFile(avatar, avatar.GetType(), mEditorUtility.ChoseFolder(DataPath) + '/' + maker.Root.name+"Root" + ".avatar.asset");
        }
        EditorGUI.EndDisabledGroup();
    }

    void AutoDetect()
    {
        var maker = target as AvatarMaker;

        maker.Root = EditorGUILayout.ObjectField("root", maker.Root, typeof(Transform), true) as Transform;

        EditorGUILayout.BeginHorizontal();

        EditorGUI.BeginDisabledGroup(false);
        mDescription = EditorGUILayout.ObjectField("mHumanoidDescription", mDescription, typeof(mHumanoidDescription), true) as mHumanoidDescription;
        path = EditorGUILayout.TextField(path);
        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("Select"))
        {
            mEditorUtility.OpenFile(ref path, ref mDescription);
        }
        if (GUILayout.Button("Create"))
        {
            //var skeleton=Skeleton.
            mDescription = mHumanoidDescription.Create();
        }

        EditorGUILayout.EndHorizontal();

        /*
        if (null != maker.Root && null != mDescription)
        {
            if (GUILayout.Button("GenerateAvatar"))
            {
                var avatar = mDescription.CreateAvatar(maker.Root);
                if (avatar.isValid)
                    mEditorUtility.WriteFile(avatar, avatar.GetType(), mEditorUtility.ChoseFolder() + '/' + maker.Root.name + ".avatar.asset");
                else
                    Debug.LogError(avatar + " is not valid");
            }
        }
        else
        {
            EditorGUI.BeginDisabledGroup(true);
            GUILayout.Button("GenerateAvatar");
            EditorGUI.EndDisabledGroup();
        }
        */
    }

    void OfficalForm()
    {

    }

    void RootPort()
    {
        var maker = target as AvatarMaker;
        maker.Root = EditorGUILayout.ObjectField("Root", maker.Root, typeof(Transform), true) as Transform;

        if (maker.Root == null)
            EditorGUILayout.HelpBox("This function require the transfrom of target !",MessageType.Error);
    }

    void AlignSpineChild(Transform tf,Transform child,Vector3 fwd)
    {
        EditRotationButKeepChild(tf, Quaternion.LookRotation(fwd, child.position - tf.position));
    }

    void AlignLegChild(Transform tf,Transform child,Vector3 up)
    {
        EditRotationButKeepChild(tf, Quaternion.LookRotation(child.position - tf.position,up));
    }

    void AlignArmChild(Transform tf, Transform child, Vector3 up)
    {
        EditRotationButKeepChild(tf, Quaternion.LookRotation(child.position - tf.position, up));
    }

    void EditRotationButKeepChild(Transform tf,Quaternion rotation)
    {

        var tfs = tf.GetComponentsInChildren<Transform>();
        var length = tfs.Length;

        var positions = tfs.Select(x => x.position).ToArray();
        tf.rotation = rotation;
        for (int i = 0; i < length; i++)
        {
            if (tfs[i] == tf)
                continue;

            tfs[i].position = positions[i];
        }
    }


    void NormalizeBones()
    {
        var maker = target as AvatarMaker;
        
        var bones=maker.SkeletonMap.BoneTransform.Forward;
        var root = maker.Root;
        var fwd = root.forward;
        var up = root.up;
        var right = root.right;

        #region Body
        if (bones.TryGetValue(HumanBodyBones.Hips, out var tf))
            if (bones.TryGetValue(HumanBodyBones.Spine, out var child))
                AlignSpineChild(tf, child,fwd);

        if (bones.TryGetValue(HumanBodyBones.Spine, out tf))
            if (bones.TryGetValue(HumanBodyBones.Chest, out var child))
                AlignSpineChild(tf, child, fwd);

        if (bones.TryGetValue(HumanBodyBones.Chest, out tf))
            if (bones.TryGetValue(HumanBodyBones.UpperChest, out var child))
                AlignSpineChild(tf, child, fwd);

        if (bones.TryGetValue(HumanBodyBones.UpperChest, out tf))
            if (bones.TryGetValue(HumanBodyBones.Neck, out var child))
                AlignSpineChild(tf, child, fwd);

        if (bones.TryGetValue(HumanBodyBones.Neck, out tf))
            if (bones.TryGetValue(HumanBodyBones.Head, out var child))
                AlignSpineChild(tf, child, fwd);

        if (bones.TryGetValue(HumanBodyBones.Head, out tf))
            EditRotationButKeepChild(tf,Quaternion.LookRotation(fwd, up));
        #endregion

        #region Legs

        if (bones.TryGetValue(HumanBodyBones.RightUpperLeg, out tf))
            if (bones.TryGetValue(HumanBodyBones.RightLowerLeg, out var child))
                AlignLegChild(tf, child, fwd);

        if (bones.TryGetValue(HumanBodyBones.RightLowerLeg, out tf))
            if (bones.TryGetValue(HumanBodyBones.RightFoot, out var child))
                AlignLegChild(tf, child, fwd);

        if (bones.TryGetValue(HumanBodyBones.RightFoot, out tf))
            if (bones.TryGetValue(HumanBodyBones.RightToes, out var child))
                AlignLegChild(tf, child, up);

        if (bones.TryGetValue(HumanBodyBones.RightToes, out tf))
            EditRotationButKeepChild(tf, Quaternion.LookRotation(fwd, up));

        if (bones.TryGetValue(HumanBodyBones.LeftUpperLeg, out tf))
            if (bones.TryGetValue(HumanBodyBones.LeftLowerLeg, out var child))
                AlignLegChild(tf, child, fwd);

        if (bones.TryGetValue(HumanBodyBones.LeftLowerLeg, out tf))
            if (bones.TryGetValue(HumanBodyBones.LeftFoot, out var child))
                AlignLegChild(tf, child, fwd);

        if (bones.TryGetValue(HumanBodyBones.LeftFoot, out tf))
            if (bones.TryGetValue(HumanBodyBones.LeftToes, out var child))
                AlignLegChild(tf, child, up);

        if (bones.TryGetValue(HumanBodyBones.LeftToes, out tf))
            EditRotationButKeepChild(tf, Quaternion.LookRotation(fwd, up));

        #endregion

        #region Arms

        if (bones.TryGetValue(HumanBodyBones.RightShoulder, out tf))
            if (bones.TryGetValue(HumanBodyBones.RightUpperArm, out var child))
                AlignArmChild(tf, child, up);

        if (bones.TryGetValue(HumanBodyBones.RightUpperArm, out tf))
            if (bones.TryGetValue(HumanBodyBones.RightLowerArm, out var child))
                AlignArmChild(tf, child, fwd);

        if (bones.TryGetValue(HumanBodyBones.RightLowerArm, out tf))
            if (bones.TryGetValue(HumanBodyBones.RightHand, out var child))
                AlignArmChild(tf, child, fwd);

        if (bones.TryGetValue(HumanBodyBones.RightHand, out tf))
            EditRotationButKeepChild(tf, Quaternion.LookRotation(right, fwd));

        if (bones.TryGetValue(HumanBodyBones.LeftShoulder, out tf))
            if (bones.TryGetValue(HumanBodyBones.LeftUpperArm, out var child))
                AlignArmChild(tf, child, up);

        if (bones.TryGetValue(HumanBodyBones.LeftUpperArm, out tf))
            if (bones.TryGetValue(HumanBodyBones.LeftLowerArm, out var child))
                AlignArmChild(tf, child, fwd);

        if (bones.TryGetValue(HumanBodyBones.LeftLowerArm, out tf))
            if (bones.TryGetValue(HumanBodyBones.LeftHand, out var child))
                AlignArmChild(tf, child, fwd);

        if (bones.TryGetValue(HumanBodyBones.LeftHand, out tf))
            EditRotationButKeepChild(tf, Quaternion.LookRotation(-right, fwd));


        #endregion
    }
}



