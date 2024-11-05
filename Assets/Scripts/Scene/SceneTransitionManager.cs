using UnityEngine;
using UnityEngine.Playables;
/*
1. 新しい空のゲームオブジェクトを作成し、「SceneTransitionManager」という名前を付けます。

2. SceneTransitionManagerスクリプトをアタッチします。

3. 同じゲームオブジェクトにCoroutineRunnerスクリプトもアタッチします。

4. Unityのタイムラインウィンドウを開き、インとアウトのトランジション用に2つの新しいタイムラインアセットを作成します。

5. 各タイムラインで、アニメーショントラックを追加し、フェードイン/フェードアウトなどのトランジション効果を作成します。

6. SceneTransitionManagerのInTransitionとOutTransitionフィールドに、それぞれ対応するタイムラインアセットをドラッグ＆ドロップします。

7. SceneManagerSOアセットで、transitionDurationをトランジションの長さに合わせて調整します。

これにより、デザイナーは以下のようにタイムラインを使ってビジュアル的にトランジションを作成・編集できるようになります：

- フェードイン/アウト効果
- ワイプトランジション
- カットアウトアニメーション
- パーティクル効果
- その他のビジュアルエフェクト

デザイナーは、Unityのタイムラインエディタを使用してこれらのエフェクトを作成・調整できます。
*/

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    public PlayableDirector inTransition;
    public PlayableDirector outTransition;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayInTransition()
    {
        if (inTransition != null)
        {
            inTransition.Play();
        }
    }

    public void PlayOutTransition()
    {
        if (outTransition != null)
        {
            outTransition.Play();
        }
    }
}
