using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RuleScene : MonoBehaviour
{
    public void OnTapStartButton()
    {
        var obj = new GameObject("SE");
        var playSE = obj.AddComponent<PlaySE>();
        playSE.PlaySe(ApplicationConfigs.Config.SeConfig.LevelUpSeAudioClip, FindObjectOfType<SoundManager>().volume);
        SceneManager.LoadScene("GameScene");
    }    
}
