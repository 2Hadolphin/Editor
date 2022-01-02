using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Animations;
using UnityEngine.Playables;
using System.Linq;
using Unity.Collections;
using Return.Humanoid.Animation;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;

//[CustomEditor(typeof(AnimationLibrary_Vector3))]
public class Editor_AnimationLibrary : OdinEditorWindow,IAnimationWindowPreview
{
    public Animator animator;
    public bool Play = false;
    protected PlayableGraph mGraph;
    protected AnimationMixerPlayable mixerPlayable;
    protected FreeFormBlendNode_EditorTool_PolarGradient blendAnim;
    double speed;
    float progress;
    static HumanPose originPose;
    protected Vector2 scroll;
    [SerializeField]
    protected AnimationLibrary_Vector3 library;
    protected NativeArray<TransformStreamHandle> MirrorBones;
    protected NativeArray<Job_Humanoid_Mirror.HandlePair> MirrorBonePair;
    public Vector2 InputHandle=default;

    static Material mat;
    Editor_AnimationLibrary()
    {
        this.titleContent = new GUIContent("Animation Library Editor");
    }

    [MenuItem("Tools/Animation/LibraryEditor")]
    static void Init()
    {
        var window = (Editor_AnimationLibrary)EditorWindow.GetWindow(typeof(Editor_AnimationLibrary));
        window.Show();

    }

    protected override void OnEnable()
    {
        base.OnEnable();
        var Mat=Resources.Load<Material>("Materials/URP_Unlit_Transparent");
        mat = new Material(Mat);
        mat.SetColor("Color", new Color(255, 255, 255, 25));
    }
    protected override void OnDestroy()
    {
        if (mat)
            DestroyImmediate(mat);
        base.OnDestroy();
    }

    protected override void OnGUI()
    {
        library=(AnimationLibrary_Vector3)EditorGUILayout.ObjectField(library,typeof(AnimationLibrary_Vector3),false);

        SirenixEditorFields.UnityObjectField(mat, typeof(Material), false);

        if (!library)
            return;

        GUILayout.BeginHorizontal();
        {
            animator = (Animator)EditorGUILayout.ObjectField(animator, typeof(Animator), true);

            if (animator)
            {
                SetFlagReference(animator.GetComponentsInChildren<Transform>(), HideFlags.NotEditable);
                if (GUILayout.Button("Clear"))
                {
                    StopAnimation();
                    SetFlagReference(animator.GetComponentsInChildren<Transform>(), HideFlags.None);
                    animator = null;
                   // ActiveEditorTracker.sharedTracker.isLocked = false;
                    
                }
            }
            else
            {
                if (Play)
                    StopAnimation();
            }
        }
        GUILayout.EndHorizontal();

       // mat = (Material)EditorGUILayout.ObjectField(mat, typeof(Material), false);

        GUILayout.BeginVertical();
        {
            if (animator)
            {
                if (!Play)
                {
                    if (GUILayout.Button("DisplayMixedAnimation"))
                    {
                        PlayAnimation(library);
                    }
                }
                else
                {
                    if (GUILayout.Button("StopMixedAnimation"))
                    {
                        StopAnimation();
                        return;
                    }
                    InputHandle = blendAnim.InputParameter;
                    InputHandle = EditorGUILayout.Vector2Field("MotionInput", InputHandle);
                    blendAnim.InputParameter = InputHandle;



                    var newspeed = EditorGUILayout.DoubleField("speed", speed);
                    if (newspeed != speed)
                    {
                        speed = newspeed;
                        mixerPlayable.SetSpeed(newspeed);
                    }

                    var newprogress = EditorGUILayout.Slider("Time", progress,0f,1f);
                    if (newprogress != progress)
                    {
                        progress = newprogress;
                        var time=mixerPlayable.GetDuration()*newprogress;
                        mixerPlayable.SetTime((double)time);
                    }
                }
            }
        }
        GUILayout.EndVertical();



        scroll=EditorGUILayout.BeginScrollView(scroll);
        if (Play)
        {
            foreach(var b in MirrorBones)
            {
            }

        }
        else
        {
            var handles = new MuscleHandle[100];
            MuscleHandle.GetMuscleHandles(handles);
            foreach (var m in handles)
            {
                GUILayout.Label(m.name);
            }
        }
        EditorGUILayout.EndScrollView();

    

    }

    static void SetFlagReference(Transform[] gameObjects, HideFlags hideFlag)
    {
        var length = gameObjects.Length;
        for (int i = 0; i < length; i++)
        {
            if (gameObjects[i])
                gameObjects[i].hideFlags = hideFlag;
        }
    }

    void PlayAnimation(AnimationLibrary_Vector3 library)
    {


        Play = true;

        HumanPoseHandler poseHandler = new HumanPoseHandler(animator.avatar, animator.GetBoneTransform(HumanBodyBones.Hips));
        poseHandler.GetHumanPose(ref originPose);

        //AnimationClip[] clips = library.Collections.Select(x => x.Clip).ToArray();
        var clipsData = library.Collections.ToArray();

        mGraph = PlayableGraph.Create("AnimationLibrary");
        mGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        mixerPlayable = AnimationMixerPlayable.Create(mGraph);
        mixerPlayable.SetSpeed(speed);

        blendAnim = animator.gameObject.AddComponent<FreeFormBlendNode_EditorTool_PolarGradient>();
        var playables=blendAnim.Init(library,animator,mGraph);
        blendAnim.mat = mat;
        blendAnim.WeightPost += ApplyWeight;

        foreach (var playable in playables)
        {
            mixerPlayable.AddInput(playable, 0, 0);
        }
        /*
        #region BoneMap
        var handles = mAnimationUtility.GetStreamBones(animator, mAnimationUtility.GetHumanoidBones(animator,mAnimationUtility.MirrorBones));
        MirrorBones = new NativeArray<TransformStreamHandle>(handles, Allocator.Persistent);

        var handlePairs = mAnimationUtility.GetStreamBones(animator, mAnimationUtility.GetHumanoidBones(animator,mAnimationUtility.MirrorBonePairs,true));
        MirrorBonePair = new NativeArray<Job_Humanoid_Mirror.HandlePair>(Job_Humanoid_Mirror.HandlePair.Pairs(handlePairs), Allocator.Persistent);
        #endregion

        

        var length = clipsData.Length;

        for (int i = 0; i < length; i++)
        {
            if (!clipsData[i].Clip)
                continue;


            if(clipsData[i].Mirror)
            {
                var clipPlayable = AnimationClipPlayable.Create(mGraph, clipsData[i].Clip);
                if (true)
                {
                    var mirrorJob = new Job_Humanoid_Mirror();
                    mirrorJob.MirrorBones = MirrorBones;
                    mirrorJob.MirrorPairBones = MirrorBonePair;

                    //mirrorJob.hips = new Unity.Collections.NativeArray<TransformStreamHandle>(new TransformStreamHandle[] { Animator.BindStreamTransform(Animator.GetBoneTransform(HumanBodyBones.Hips))},Unity.Collections.Allocator.Persistent);
                    var clipMirrorPlayable = AnimationScriptPlayable.Create<Job_Humanoid_Mirror>(mGraph,mirrorJob);
                    clipMirrorPlayable.AddInput(clipPlayable, 0,1);

                    mixerPlayable.AddInput(clipMirrorPlayable,0,1);
                }
                else
                {
                    var clip = clipsData[i].Clip;
                    var setting = new AnimationClipSettings() { mirror = true, loopTime = true };
                    AnimationUtility.SetAnimationClipSettings(clip, setting);
                    mixerPlayable.AddInput(clipPlayable, 0);
                }

            }
            else
            {
                var clipPlayable = AnimationClipPlayable.Create(mGraph, clipsData[i].Clip);

                mixerPlayable.AddInput(clipPlayable, 0);
            }

        }
        */


        var output = AnimationPlayableOutput.Create(mGraph, "LibraryDisplay", animator);
        output.SetSourcePlayable(mixerPlayable);

        mGraph.Play();
        mGraph.Evaluate();
    }

    public  void ApplyWeight(float[] weights)
    {
        var length = weights.Length;
        for (int i = 0; i < length; i++)
        {
            mixerPlayable.SetInputWeight(i, weights[i]);
        }


    }

    void StopAnimation()
    {
        HumanPoseHandler poseHandler = new HumanPoseHandler(animator.avatar, animator.transform);
        poseHandler.SetHumanPose(ref originPose);

        if (mGraph.IsValid())
        {
            if (mGraph.IsPlaying())
                mGraph.Stop();
            mGraph.Destroy();
        }

        if (MirrorBones.IsCreated)
            MirrorBones.Dispose();

        if (MirrorBonePair.IsCreated)
            MirrorBonePair.Dispose();
  

        DestroyImmediate(blendAnim);
        Play = false;

        var setting = new AnimationClipSettings();
        setting.mirror=false;

        //var go = animator.gameObject;
       // PrefabUtility.RevertObjectOverride(go, InteractionMode.UserAction);
    }

    public void StartPreview()
    {
    }

    public void StopPreview()
    {
    }

    public void UpdatePreviewGraph(PlayableGraph graph)
    {
    }

    public Playable BuildPreviewGraph(PlayableGraph graph, Playable inputPlayable)
    {
        return inputPlayable;
    }
}