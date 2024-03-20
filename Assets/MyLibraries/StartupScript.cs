using UnityEngine;

public class StartupScript
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoad()
    {
        // �Q�[�������[�h�����O�Ɏ��s����R�[�h
        var obj = new GameObject("SoundManager");
        obj.AddComponent<SoundManager>().Init();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnAfterSceneLoad()
    {
        // �ŏ��̃V�[�������[�h���ꂽ��Ɏ��s����R�[�h
    }
}