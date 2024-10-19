/*
1. `[InitializeOnLoad]` 属性により、Unityエディタの起動時にこのクラスが初期化されます。

2. 静的コンストラクタで `SceneView.duringSceneGui` イベントに `OnSceneGUI` メソッドを登録しています。

3. `OnSceneGUI` メソッドでは、シーン内のすべてのGameObjectを走査し、タグが設定されているオブジェクトに対してそのタグを表示します。

4. `Handles.Label` を使用して、オブジェクトの位置にタグのテキストを描画します。
*/

using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class TagGizmo
{
    static TagGizmo()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    static void OnSceneGUI(SceneView sceneView)
    {
        foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>())
        {
            if (!string.IsNullOrEmpty(obj.tag) && obj.tag != "Untagged")
            {
                // Draw tag label
                Handles.Label(obj.transform.position, obj.tag);
            }
        }
    }
}
