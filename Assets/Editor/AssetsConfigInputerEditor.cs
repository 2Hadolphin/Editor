#if UNITYEDITOR
using UnityEngine;

using UnityEditor;


[CustomEditor(typeof(AssetsConfigInputer))]
public class AssetsConfigInputerEditor : UnityEditor.Editor
{

    public override void OnInspectorGUI()
    {
        AssetsConfigInputer inputer = (AssetsConfigInputer)target;


        if (DrawDefaultInspector())
        {
            if (inputer)
            {
            }
        }

        if (GUILayout.Button("Input Assets Data"))
        {
            LoadFiles(inputer);
        }

        if (GUILayout.Button("Input Accounts Data"))
        {
            LoadAccounts(inputer);
        }

    }


    private void LoadFiles(AssetsConfigInputer inputer)
    {
        inputer.Infos=(WeaponInfo[])Resources.FindObjectsOfTypeAll(typeof(WeaponInfo));
        inputer.weaponInfoData.WeaponInfos = inputer.Infos;
        EditorUtility.SetDirty(inputer);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void LoadAccounts(AssetsConfigInputer inputer)
    {
        inputer.Accounts = (AccountInfo[])Resources.FindObjectsOfTypeAll(typeof(AccountInfo));
        inputer.accountData.Accounts = inputer.Accounts;
        EditorUtility.SetDirty(inputer.accountData);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

}
#endif