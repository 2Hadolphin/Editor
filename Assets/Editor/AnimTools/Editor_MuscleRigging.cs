using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Animations;
using System.Linq;
using UnityEngine.Playables;
public class Editor_MuscleRigging : EditorWindow
{
    Animator Animator;
    Vector2 scroll;
    Vector2 scroll2;
    Job_Humanoid_MuscleRigging.MuscleData[] muscleDatas;
    bool IsPlaying=false;
    PlayableGraph mGraph;
    AnimationScriptPlayable scriptPlayable;
    HumanPose Pose;
    Vector3 HipPosition;
    Quaternion HipRotation;
    TransformStreamHandle HipStreamHandle;
    Editor_MuscleRigging()
    {
        this.titleContent = new GUIContent("MuscleRigging");
    }

    [MenuItem("Window/Animation/MuscleRigging")]
    private static void Init()
    {
        var window = (Editor_MuscleRigging)EditorWindow.GetWindow(typeof(Editor_MuscleRigging));
        window.Show();
        //window.Reset();
    }

    void Reset()
    {
        muscleDatas = Job_Humanoid_MuscleRigging.MuscleData.CreateAllMuscleDatas;



        if (IsPlaying)
        {
            UpdateParameter();

            Stop();
        }

        if (Animator)
        {
            var hips = Animator.GetBoneTransform(HumanBodyBones.Hips);
            HipPosition = hips.localPosition;
            HipRotation = hips.localRotation;

            HipStreamHandle = Animator.BindStreamTransform(hips);
        }


        Debug.Log("Reset"+HipStreamHandle);

    }


    public void OnEnable()
    {
        Reset();
    }
    private void OnGUI()
    {
        var animator = EditorGUILayout.ObjectField("Animator",Animator, typeof(Animator), true) as Animator;

        if (Animator != animator)
        {
            Animator = animator;
            Reset();
        }

   

        #region CheckSource
        if (!Animator) 
        {
            EditorGUILayout.HelpBox("Please selecet an Aniamtor with humanoid avatar.", MessageType.Info);
            return;
        } 
        if (!Animator.avatar)
        {
            EditorGUILayout.HelpBox("This Aniamtor has none humanoid avatar.", MessageType.Warning);
            return;
        }
        if (!Animator.isHuman)
        {
            EditorGUILayout.HelpBox("This Aniamtor contain the wrong avatar which not belong to humanoid.", MessageType.Error);
            return;
        }

        Debug.DrawRay(Pose.bodyPosition + Animator.transform.position, Vector3.forward);

        #endregion

        // Test avatar
        {
            scroll2=EditorGUILayout.BeginScrollView(scroll2);
            var humanBones = animator.avatar.humanDescription.human;
            foreach(var bone in humanBones)
            {
                GUILayout.Label(bone.humanName+"--" + bone.boneName + "--" + bone.limit.axisLength);
            }
            EditorGUILayout.EndScrollView();
        }
        var width = EditorGUIUtility.currentViewWidth;

        // Muscle Control
        {
            EditorGUILayout.BeginHorizontal();


            if (!IsPlaying)
            {
                if (GUILayout.Button("Display",GUILayout.Width(width / 2)))
                    Play();
            }
            else
            {
                if (GUILayout.Button("Stop", GUILayout.Width(width / 2)))
                    Stop();
            }

            if (GUILayout.Button("Reset", GUILayout.Width(width / 2)))
                  Reset();

            EditorGUILayout.EndHorizontal();
        }


        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            var length = muscleDatas.Length;

            GUILayout.Label("MuscleNumbers : "+length.ToString());

            var muscleCount = HumanTrait.BoneCount;

            for (int i = 0; i < length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(muscleDatas[i].muscleHandle.name);

                if (i < muscleCount)
                {
                    muscleDatas[i].value = EditorGUILayout.Slider(muscleDatas[i].value, -1f, 1f, GUILayout.Width(width/2));
                }
                else
                {
                    muscleDatas[i].value = EditorGUILayout.Slider(muscleDatas[i].value, -1f, 1f, GUILayout.Width(width / 2));
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

             UpdateParameter();
        }

        return;
        {




            scroll2 = EditorGUILayout.BeginScrollView(scroll2);
            var muscleCount = HumanTrait.BoneCount;
            GUILayout.Label("HumanTrait : " + muscleCount.ToString());
            string[] muscleName = HumanTrait.MuscleName;

            for (int i = 0; i < muscleCount; ++i)
            {
                GUILayout.Label(muscleName[i] + " min: " + HumanTrait.GetMuscleDefaultMin(i) + " max: " + HumanTrait.GetMuscleDefaultMax(i));
            }
            EditorGUILayout.EndScrollView();
        }

        // HumanDescription Test
        {
            var skeletons = this.Animator.avatar.humanDescription.skeleton;
            scroll2 = EditorGUILayout.BeginScrollView(scroll2);
            foreach (var skeleton in skeletons)
            {
                GUILayout.Label(skeleton.name);
            }
            EditorGUILayout.EndScrollView();
        }

    }


    void UpdateParameter()
    {
        if (!IsPlaying)
            return;

        try
        {
            var job = new Job_Humanoid_MuscleRigging();
            job.Datas = muscleDatas;


            job.BodyPosition = HipPosition;
            job.BodyRotation = HipRotation;
            job.Root = HipStreamHandle;
            scriptPlayable.SetJobData(job);
            mGraph.Evaluate();

        }
        catch (System.Exception e)
        {
            Stop();
            throw;
        }

    }

    void Play()
    {
        IsPlaying = true;
        LoadScenePose();

        mGraph = PlayableGraph.Create("MuscleRigging");
        mGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        var job = new Job_Humanoid_MuscleRigging();
        scriptPlayable = AnimationScriptPlayable.Create(mGraph, job);

        var output = AnimationPlayableOutput.Create(mGraph, "MuscleRiggingOutput", Animator);
        output.SetSourcePlayable(scriptPlayable);

        mGraph.Play();
    }

    void Stop()
    {
        IsPlaying = false;

        mAnimationUtility.Force_TPose(Animator);


        if (mGraph.IsPlaying()) 
            mGraph.Stop();

        if (mGraph.IsValid())
            mGraph.Destroy();

        if (scriptPlayable.CanDestroy())
            scriptPlayable.Destroy();
    }

    void LoadScenePose()
    {
        if (muscleDatas == null)
            return;

        var poseHandler = new HumanPoseHandler(Animator.avatar, Animator.GetBoneTransform(HumanBodyBones.Hips));
        poseHandler.GetHumanPose(ref Pose);

       
        var muscles = Pose.muscles;
        var length = muscles.Length;

        if (muscleDatas.Length < length)
            return;

        for (int i = 0; i < length; i++)
        {
            muscleDatas[i].value = muscles[i];
        }

    }
}
