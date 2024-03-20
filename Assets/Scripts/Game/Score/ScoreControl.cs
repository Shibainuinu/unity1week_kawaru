using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ScoreControl : MonoBehaviour
{

    [SerializeField]
    private ScoreNumber[] scoreNumbers;

    private int currentNum = 0;
    private int nextNum = 0;
    private bool isAnimation = false;

    private void Awake()
    {
        foreach (ScoreNumber number in scoreNumbers)
        {
            number.Init(0);
        }
    }

    private void Update()
    {
        // 数値が変化した時にアニメーションをスタートする
        if (!isAnimation && nextNum != currentNum)
        {
            isAnimation = true;
            UpdateScore(nextNum);
        }
    }

    public void SetScore(int num) 
    {
        nextNum = num;
    }

    async Task UpdateScore(int num)
    {
        var taskList = new List<Task>();
        var numArray = GetNumArray(num);
        for (int i = 0; i < 5; i++)
        {
            // 変更された桁だけ更新
            if (numArray[i] != scoreNumbers[i].CurrentNum) 
            {
                taskList.Add(scoreNumbers[i].ChangeNum(numArray[i]));
                await Task.Delay((int)(ApplicationConfigs.Config.DesignConfig.ScoreAnimationWaitSecond * 1000));
            }
        }

        // すべてのアニメーションが終了するまで待機
        await Task.WhenAll(taskList);
        isAnimation = false;
    }

    private int[] GetNumArray(int num) 
    {
        int[] array = new int[5];
        string str = num.ToString();

        for (int i = 0; i < 5; i++) {
            if (i < str.Length)
            {
                array[4 - i] = int.Parse(str[str.Length-1-i].ToString());
            }
            else 
            {
                array[4 - i] = 0;
            }
        }
        return array;
    }

}
