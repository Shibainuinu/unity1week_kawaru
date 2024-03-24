using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[Serializable]
public class ApplicationConfigDatas : ScriptableObject
{
    public GameConfig GameConfig;
    public DesignConfig DesignConfig;
    public BgmConfig BgmConfig;
    public SeConfig SeConfig;
}

[Serializable]
public class GameConfig
{
    public int MixingNum = 10;
    public float AddColorSpeed = 0.1f;
    public float AddColorAmount = 0.01f;
    public float PerfectJudgeRange = 0.03f;
    public float JudgeSeverity = 0.05f;

    [Range(0f, 1f)]
    public float OrderColorMax = 1.0f;

    [Range(0f, 1f)]
    public float OrderColorMin = 0.0f;

    public string[] SerifArray;
}

[Serializable]
public class DesignConfig
{
    [Tooltip("�������ʂ̃Q�[�W�̃X�s�[�h")]
    public float ScoreAnimSpeed = 0.1f;


}









[Serializable]
public class BgmConfig
{
    [Tooltip("BGM�̉����f�[�^")]
    public AudioClip BgmAudioClip;
}


[Serializable]
public class SeConfig
{
    [Tooltip("�v���[�g�����̂������̉�")]
    public AudioClip LevelUpSeAudioClip;
}


