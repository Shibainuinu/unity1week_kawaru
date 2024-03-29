using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class TitleScene : MonoBehaviour
{

    [SerializeField] UnityEngine.UI.Slider slider;

    private void Awake()
    {
        slider.value = FindObjectOfType<SoundManager>().volume;
        if (!FindObjectOfType<SoundManager>().IsBgmPlay) 
        {
            FindObjectOfType<SoundManager>().PlayBGM(ApplicationConfigs.Config.BgmConfig.BgmAudioClip);
        }
    }

    public void ChangedSlider(float value)
    {
        FindObjectOfType<SoundManager>().ChangedSlider(value);
    }

    public void OnTapStartButton()
    {
        var obj = new GameObject("SE");
        var playSE = obj.AddComponent<PlaySE>();
        playSE.PlaySe(ApplicationConfigs.Config.SeConfig.LevelUpSeAudioClip, FindObjectOfType<SoundManager>().volume);
        SceneManager.LoadScene("RuleScene");        
    }

}
