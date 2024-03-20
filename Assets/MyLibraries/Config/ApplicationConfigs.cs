using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public static class ApplicationConfigs
{
    static private ApplicationConfigDatas config;

    /// <summary>
    /// Conf値
    /// </summary>
    static public ApplicationConfigDatas Config
    {
        //configがnullならロードしてキャッシュする
        get { return config ?? (config = LoadConfig()); }
    }

    /// <summary>
    /// 環境別設定値読み込み
    /// </summary>
    /// <returns></returns>
    static private ApplicationConfigDatas LoadConfig()
    {
        return Resources.Load<ApplicationConfigDatas>("ApplicationConfigDatas");
    }
}
