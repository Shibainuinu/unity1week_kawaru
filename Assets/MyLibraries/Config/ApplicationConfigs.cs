using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public static class ApplicationConfigs
{
    static private ApplicationConfigDatas config;

    /// <summary>
    /// Conf�l
    /// </summary>
    static public ApplicationConfigDatas Config
    {
        //config��null�Ȃ烍�[�h���ăL���b�V������
        get { return config ?? (config = LoadConfig()); }
    }

    /// <summary>
    /// ���ʐݒ�l�ǂݍ���
    /// </summary>
    /// <returns></returns>
    static private ApplicationConfigDatas LoadConfig()
    {
        return Resources.Load<ApplicationConfigDatas>("ApplicationConfigDatas");
    }
}
