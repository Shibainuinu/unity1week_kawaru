using UnityEngine;

public class StartupScript
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoad()
    {
        // ゲームがロードされる前に実行するコード
        var obj = new GameObject("SoundManager");
        obj.AddComponent<SoundManager>().Init();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnAfterSceneLoad()
    {
        // 最初のシーンがロードされた後に実行するコード
    }
}