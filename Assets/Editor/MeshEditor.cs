using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using Return.Helper;

public class MeshEditor: OdinEditorWindow
{
    [MenuItem("Tools/Mesh/Editor")]
    private static void OpenWindow()
    {
        GetWindow<MeshEditor>()
            .position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
    }

    [HideLabel][HorizontalGroup("Asset")]
    [PropertySpace(10,10)][OnValueChanged("NewData")]
    public Mesh mMesh;

    [HorizontalGroup("Asset")]
    [Button("Clean")]
    [PropertySpace(10, 10)]
    void Clean()
    {
        mMesh = null;
        NewData();
    }


    [HideLabel]
    [HorizontalGroup("mList")]
    public List<Mesh> Meshes=new List<Mesh>();


    [VerticalGroup("Edit/Data")][HideLabel]
    [ReadOnly]
    public Bounds Bounds;

    [PropertySpace(10,10)]
    [BoxGroup("Edit",ShowLabel =false)]
    [Button("ArchiveMesh",ButtonSizes.Large)]
    public void ArchiveMesh()
    {

        /* var newMesh = new Mesh();
         newMesh.vertices = mMesh.vertices;
         newMesh.triangles = mMesh.triangles;
        */
        if(mMesh)
            mMesh=mArchiveMesh(mMesh);

        var newMeshs = new List<Mesh>();
        foreach (var mesh in Meshes)
        {
            newMeshs.Add(mArchiveMesh(mesh));
        }

        Meshes.Clear();
        Meshes.AddRange(newMeshs);
    }

    Mesh mArchiveMesh(Mesh mesh)
    {
        var newMesh = Mesh.Instantiate(mesh);
        mEditorUtility.WriteFile(newMesh, typeof(Mesh), null);// EditorUtility.OpenFolderPanel("Archive Path", "", ""));
        return newMesh;
    }

    [BoxGroup("Edit/Size",ShowLabel =false)]
    [PropertySpace(10, 0)]
    public float Size;
    [Button("Resize")]
    [BoxGroup("Edit/Size", ShowLabel = false)]
    [PropertySpace(0, 10)]
    public void Resize()
    {
        if(mMesh)
        mResize(mMesh);
        foreach (var mesh in Meshes)
        {
            mResize(mesh);
        }
    }

    public void mResize(Mesh mesh)
    {
        var length = mesh.vertexCount;
        var vertices = mesh.vertices;
        for (int i = 0; i < length; i++)
        {
            vertices[i] = vertices[i].Multiply(Size);
        }

        mesh.vertices = vertices;
        UpdateMesh(mesh);
    }

    [BoxGroup("Edit/Offset", ShowLabel = false)]
    [PropertySpace(10, 0)]
    public Vector3 Offset;
    [Button("GetCenter")]
    [BoxGroup("Edit/Offset", ShowLabel = false)]
    public void SetCenter()
    {
        Offset = -Bounds.center;
    }
    [Button("SetOffset")]
    [BoxGroup("Edit/Offset", ShowLabel = false)]
    [PropertySpace(0, 10)]
    public void SetOffset()
    {
        var length = mMesh.vertexCount;
        var vertices = mMesh.vertices;
        for (int i = 0; i < length; i++)
        {
            vertices[i] = vertices[i]+Offset;
        }

        mMesh.vertices = vertices;
        UpdateMesh(mMesh);
    }

    [BoxGroup("Edit/Rotate", ShowLabel = false)]
    public Vector3 Rotation;
    [BoxGroup("Edit/Rotate", ShowLabel = false)]
    [Button("Rotate")]
    [PropertySpace(0, 10)]
    public void Rotate()
    {
        var matrix = UnityEngine.Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(Rotation), Vector3.one);

        if (mMesh)
        {
            var length = mMesh.vertexCount;
            var vertices = mMesh.vertices;

            for (int i = 0; i < length; i++)
            {
                vertices[i] = matrix.MultiplyPoint(vertices[i]);
            }

            mMesh.vertices = vertices;
            UpdateMesh(mMesh);
        }


        foreach (var mesh in Meshes)
        {
            var length = mesh.vertexCount;
            var vertices = mesh.vertices;

            for (int i = 0; i < length; i++)
            {
                vertices[i] = matrix.MultiplyPoint(vertices[i]);
            }

            mesh.vertices = vertices;
            UpdateMesh(mesh);
        }

  
    }


    [PropertySpace(10, 0)]
    [BoxGroup("Edit/Binding", ShowLabel = false)]
    public SkinnedMeshRenderer renderer;
    [PropertySpace(0, 10)]
    [BoxGroup("Edit/Binding", ShowLabel = false)]
    [Button("BindBone")]
    void BindBone()
    {
        var tfs = renderer.bones;
        var length = tfs.Length;
        var bindPose = new Matrix4x4[length];
        var tf = renderer.transform;
        for (int i = 0; i < length; i++)
        {
            bindPose[i] = tfs[i].worldToLocalMatrix * tf.localToWorldMatrix;
        }
        mMesh.bindposes = bindPose;
        foreach (var mesh in Meshes)
        {
            mesh.bindposes = bindPose;
            UpdateMesh(mesh);
        }
        UpdateMesh(mMesh);
    }



    void UpdateMesh(Mesh mesh)
    {
        if (!mesh)
            return;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        EditorUtility.SetDirty(mesh);
        NewData();
    }

    void NewData()
    {
        if (mMesh == null)
        {
            Bounds = default;
        }
        else
        {
            Bounds = mMesh.bounds;

        }

    }


    protected override void OnGUI()
    {
        base.OnGUI();
    }
}
