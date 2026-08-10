using UnityEngine;
using UnityEditor;

public class ReplaceObjects : EditorWindow
{
    private GameObject newPrefab;

    [MenuItem("Tools/Object Replacer")]
    public static void ShowWindow()
    {
        GetWindow<ReplaceObjects>("Object Replacer");
    }

    private void OnGUI()
    {
        GUILayout.Label("置き換え先のプレハブを指定", EditorStyles.boldLabel);
        newPrefab = (GameObject)EditorGUILayout.ObjectField("新プレハブ", newPrefab, typeof(GameObject), false);

        if (GUILayout.Button("選択したオブジェクトを置き換える"))
        {
            if (newPrefab == null)
            {
                Debug.LogError("置き換えるプレハブが設定されていません！");
                return;
            }

            GameObject[] selectedObjects = Selection.gameObjects;

            Undo.RegisterCompleteObjectUndo(selectedObjects, "Replace Objects");

            foreach (GameObject go in selectedObjects)
            {
                GameObject newGo = (GameObject)PrefabUtility.InstantiatePrefab(newPrefab);
                newGo.transform.SetParent(go.transform.parent);
                newGo.transform.position = go.transform.position;
                newGo.transform.rotation = go.transform.rotation;
                newGo.transform.localScale = go.transform.localScale;

                Undo.RegisterCreatedObjectUndo(newGo, "Replace Objects");
                Undo.DestroyObjectImmediate(go);
            }
        }
    }
}