シーンの設定は、以下の手順でプロジェクトをセットアップしてください：

1. Unityで新しいプロジェクトを作成します（既存のプロジェクトを使用している場合はこの手順をスキップしてください）。

2. 「StartScene」、「GameScene」、「EndScene」という名前で3つの新しいシーンを作成します（まだ作成していない場合）。

4. プロジェクトビューで右クリックし、Create > ScriptableObjects > GameManagerを選択して、GameManagerのScriptableObjectインスタンスを作成します。

5. 作成したGameManagerアセットを選択し、インスペクターでSceneTransitionsの配列サイズを設定します（例：3）。

6. 各SceneTransitionに対して、対応するシーン名（例：StartScene、GameScene、EndScene）を入力します。

7. GameManagerInitializerスクリプトを新しいC#スクリプトとして作成します。

8. 空のGameObjectを作成し、それに「GameManagerInitializer」という名前を付けます。

9. GameManagerInitializerスクリプトをGameManagerInitializerゲームオブジェクトにアタッチします。

10. GameManagerInitializerコンポーネントのGameManager欄に、先ほど作成したGameManagerアセットをドラッグ＆ドロップします。

11. 各シーンにGameManagerInitializerオブジェクトを配置します（または、DontDestroyOnLoadを使用して1つのシーンから他のシーンに渡すようにします）。

これで、UnityEventを使用してシーン遷移をコードを変更せずに設定できるようになりました。例えば、ボタンクリック時にシーンを切り替えるには：

1. ボタンオブジェクトを選択します。
2. インスペクターでOnClickイベントに新しいアクションを追加します。
3. GameManagerInitializerオブジェクトをドラッグ＆ドロップします。
4. 関数ドロップダウンから「GameManagerInitializer.LoadScene」を選択します。
5. パラメータとして遷移先のシーン名（例：「GameScene」）を入力します。

このアプローチにより、GameManagerのScriptableObjectインスタンス上で各シーン遷移に対してUnityEventを設定し、シーンロード時に追加のアクションを実行することもできます。
