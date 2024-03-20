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
    [Tooltip("�v���[�g�̈ړ�����(�b)")]
    public float PlateMoveSecond = 0.0f;
    [Tooltip("�v���[�g���ړ����n�߂�h���b�O��")]
    public int PlateMoveDragAmount = 50;
    [Tooltip("�Q�[���I�[�o�[���ɃE�B���h�E���o��܂ł̎���(�b)")]
    public float GameOverWaitSecond = 2.0f;
    [Tooltip("�X�R�A�X�V���Ɏ��̌��̍X�V�����܂ł̎���(�b)")]
    public float ScoreAnimationWaitSecond = 0.5f;
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


