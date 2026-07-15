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

            // ヒエラルキーで選択中のオブジェクトを取得
            GameObject[] selectedObjects = Selection.gameObjects;

            Undo.RegisterCompleteObjectUndo(selectedObjects, "Replace Objects");

            foreach (GameObject go in selectedObjects)
            {
                // 新しいプレハブを生成
                GameObject newGo = (GameObject)PrefabUtility.InstantiatePrefab(newPrefab);
                newGo.transform.SetParent(go.transform.parent);
                newGo.transform.position = go.transform.position;
                newGo.transform.rotation = go.transform.rotation;
                newGo.transform.localScale = go.transform.localScale;

                // 元のオブジェクトを削除（Undo可能にするため一時記録）
                Undo.RegisterCreatedObjectUndo(newGo, "Replace Objects");
                Undo.DestroyObjectImmediate(go);
            }
        }
    }
}