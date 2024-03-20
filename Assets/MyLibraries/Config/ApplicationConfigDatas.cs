using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[Serializable]
public class ApplicationConfigDatas : ScriptableObject
{
    public DesignConfig DesignConfig;
    public BgmConfig BgmConfig;
    public SeConfig SeConfig;
}

[Serializable]
public class DesignConfig
{
    [Tooltip("プレートの移動時間(秒)")]
    public float PlateMoveSecond = 0.0f;
    [Tooltip("プレートが移動し始めるドラッグ量")]
    public int PlateMoveDragAmount = 50;
    [Tooltip("ゲームオーバー時にウィンドウが出るまでの時間(秒)")]
    public float GameOverWaitSecond = 2.0f;
    [Tooltip("スコア更新時に次の桁の更新されるまでの時間(秒)")]
    public float ScoreAnimationWaitSecond = 0.5f;
}

[Serializable]
public class BgmConfig
{
    [Tooltip("BGMの音声データ")]
    public AudioClip BgmAudioClip;
}


[Serializable]
public class SeConfig
{
    [Tooltip("プレートが合体した時の音")]
    public AudioClip LevelUpSeAudioClip;
}


