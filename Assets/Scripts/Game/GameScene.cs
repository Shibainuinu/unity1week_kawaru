using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameScene : MonoBehaviour
{

    private enum GameState
    {
        None = 0,       // 初期化時
        Tutorial1,

        GameInit,
        OrderAnim,
        ColorSelect,
        AddColor,
        JudgeAnim,
        JudgeAnimEnd,
        JudgeWait,
        Result,
    }


    private enum ColorId 
    {
        None = 0,
        Cyan,
        Magenta,
        Yellow,
    }

    private struct TempColorData
    {
        public float c;
        public float m;
        public float y;
    }




    [SerializeField] private ColorPlate cyanPlate;
    [SerializeField] private ColorPlate magentaPlate;
    [SerializeField] private ColorPlate yellowPlate;

    [SerializeField] private Color potionColorCyan;
    [SerializeField] private Color potionColorMagenta;
    [SerializeField] private Color potionColorYellow;

    [SerializeField] private Image tempColorImage;
    [SerializeField] private Image orderColorImage;
    [SerializeField] private Image selectColorImage1;
    [SerializeField] private Image selectColorImage2;
    [SerializeField] private Image judgeTempColorImage;
    [SerializeField] private Image judgeOrderColorImage;

    [SerializeField] private Image judgeScoreGaugeImage;
    [SerializeField] private Text judgeScoreText;

    [SerializeField] private GameObject judgeCanvasObj;


    [SerializeField] private Text targetScoreText;
    [SerializeField] private Text remainingMixingText;
    [SerializeField] private Text successCountText;
    [SerializeField] private Image addColorImage;
    [SerializeField] private Image nextGameImage;

    private GameState gameState = GameState.None;

    private ColorId selectColorId = ColorId.None;
    private TempColorData currentTempColor;

    private bool isAddColor = false;
    private float addColorTime = 0.0f;
    private int judgeScore = 0;
    private int remainingMixing;
    private int targetScore;
    private int successCount = 0;


    private void Awake()
    {

        judgeCanvasObj.SetActive(false);


        // 初期化終了時は入力待機にしておく
        gameState = GameState.GameInit;
    }

    async void Update()
    {

        switch (gameState) 
        {
            case GameState.GameInit:
                orderColorImage.color = NewOrderColor();
                currentTempColor.c = currentTempColor.m = currentTempColor.y = 0.0f;
                selectColorId = ColorId.None;
                addColorImage.gameObject.SetActive(false);
                isAddColor = false;
                addColorTime = 0.0f;
                remainingMixing = ApplicationConfigs.Config.GameConfig.MixingNum;
                remainingMixingText.text = remainingMixing.ToString();
                targetScore = NewTargetScore();
                targetScoreText.text = $"{targetScore}点以上";
                successCountText.text = $"{successCount}回成功";
                ResetSelectColor();
                UpdateTempColor();
                UpdateSelectColor();
                judgeCanvasObj.SetActive(false);
                nextGameImage.gameObject.SetActive(false);
                selectColorImage1.gameObject.SetActive(false);
                gameState = GameState.ColorSelect;
                break;

            case GameState.ColorSelect:

                break;

            case GameState.AddColor:

                if (isAddColor)
                {
                    addColorTime += Time.deltaTime;
                    if (addColorTime >= ApplicationConfigs.Config.GameConfig.AddColorSpeed)
                    {
                        addColorTime -= ApplicationConfigs.Config.GameConfig.AddColorSpeed;
                        switch (selectColorId)
                        {
                            case ColorId.Cyan:
                                currentTempColor.c += ApplicationConfigs.Config.GameConfig.AddColorAmount;
                                break;
                            case ColorId.Magenta:
                                currentTempColor.m += ApplicationConfigs.Config.GameConfig.AddColorAmount;
                                break;
                            case ColorId.Yellow:
                                currentTempColor.y += ApplicationConfigs.Config.GameConfig.AddColorAmount;
                                break;
                        }
                    }

                    UpdateTempColor();
                }

                break;

            case GameState.JudgeAnimEnd:

                if (judgeScore >= targetScore)
                {
                    // 成功
                    nextGameImage.gameObject.SetActive(true);
                    successCount++;
                }
                else 
                {
                    // 失敗
                    nextGameImage.gameObject.SetActive(false);
                }




                gameState = GameState.JudgeWait;
                break;

            case GameState.Result:

                break;


        }
    }

    private Color NewOrderColor()
    {
        var color = new Color();
        color.a = 1;
        color.r = Random.Range(ApplicationConfigs.Config.GameConfig.OrderColorMin, ApplicationConfigs.Config.GameConfig.OrderColorMax);
        color.g = Random.Range(ApplicationConfigs.Config.GameConfig.OrderColorMin, ApplicationConfigs.Config.GameConfig.OrderColorMax);
        color.b = Random.Range(ApplicationConfigs.Config.GameConfig.OrderColorMin, ApplicationConfigs.Config.GameConfig.OrderColorMax);
        return color;
    }

    private int NewTargetScore()
    {
        if (successCount <= 2)
        {
            return Random.Range( 60, 80 );
        }
        else if(successCount <= 4) 
        {
            return Random.Range(65, 85);
        }
        else if (successCount <= 6)
        {
            return Random.Range(75, 85);
        }
        else if (successCount <= 8)
        {
            return Random.Range(80, 90);
        }
        else if (successCount <= 10)
        {
            return Random.Range(88, 95);
        }
        else
        {
            return Random.Range(90, 98);
        }
    }


    public void SelectColor(int color)
    {
        if (gameState == GameState.ColorSelect)
        {
            selectColorImage1.gameObject.SetActive(true);
            addColorImage.gameObject.SetActive(true);
            selectColorId = (ColorId)color;
            UpdateSelectColor();
        }
    }

    private void ResetSelectColor()
    {
        cyanPlate.Select(false);
        magentaPlate.Select(false);
        yellowPlate.Select(false);
    }

    private void UpdateSelectColor()
    {

        ResetSelectColor();

        if (selectColorId == ColorId.None)
        {
            //selectColorImage1.transform.parent.gameObject.SetActive(false);
            selectColorImage2.transform.parent.gameObject.SetActive(false);
        }
        else 
        {
            //selectColorImage1.transform.parent.gameObject.SetActive(true);


            switch (selectColorId) 
            {
                case ColorId.Cyan:
                    selectColorImage1.color = potionColorCyan;
                    selectColorImage2.color = potionColorCyan;
//                    cyanPlate.Select(true);
                    break;
                case ColorId.Magenta:
                    selectColorImage1.color = potionColorMagenta;
                    selectColorImage2.color = potionColorMagenta;
//                    magentaPlate.Select(true);
                    break;
                case ColorId.Yellow:
                    selectColorImage1.color = potionColorYellow;
                    selectColorImage2.color = potionColorYellow;
//                    yellowPlate.Select(true);
                    break;
            }
        }
    }

    private void UpdateTempColor()
    {
        var newColor = new Color();
        newColor.r = 1.0f - currentTempColor.c;
        newColor.g = 1.0f - currentTempColor.m;
        newColor.b = 1.0f - currentTempColor.y;
        newColor.a = 1.0f;
        tempColorImage.color = newColor;
        Debug.Log(newColor);
    }


    public void TapImagePointerDown()
    {
        if (gameState == GameState.ColorSelect && remainingMixing > 0)
        {
            remainingMixing--;
            remainingMixingText.text = remainingMixing.ToString();
            isAddColor = true;
            selectColorImage1.transform.parent.gameObject.SetActive(false);
            selectColorImage2.transform.parent.gameObject.SetActive(true);
            gameState = GameState.AddColor;
        }
    }


    public void TapImagePointerUp()
    {
        if (gameState == GameState.AddColor)
        {
            if (remainingMixing == 0)
            {
                addColorImage.color = new Color( 0.62f, 0.62f, 0.62f, 1.0f );
            }
            isAddColor = false;
            selectColorImage1.transform.parent.gameObject.SetActive(true);
            selectColorImage2.transform.parent.gameObject.SetActive(false);
            gameState = GameState.ColorSelect;
        }
    }


    public void TapEndAddColorButton()
    {
        if (gameState == GameState.ColorSelect)
        {
            StartJudge();
        }
    }

    private void StartJudge()
    {
        gameState = GameState.JudgeAnim;

        var size = judgeScoreGaugeImage.rectTransform.sizeDelta;
        size.x = 0;
        judgeScoreGaugeImage.rectTransform.sizeDelta = size;

        // コピー
        judgeTempColorImage.color = tempColorImage.color;
        judgeOrderColorImage.color = orderColorImage.color;

        // 初期化
        judgeScore = 0;
        judgeScoreText.text = "0";

        judgeCanvasObj.SetActive(true);
        judgeScore = GetScore();
        ScoreAnim(judgeScore);
    }


    private int GetScore()
    {
        float diffR = Mathf.Abs(tempColorImage.color.r - orderColorImage.color.r);
        float diffG = Mathf.Abs(tempColorImage.color.g - orderColorImage.color.g);
        float diffB = Mathf.Abs(tempColorImage.color.b - orderColorImage.color.b);

        Debug.Log($"r:{diffR} g:{diffG} b:{diffB}");

        if (diffR <= ApplicationConfigs.Config.GameConfig.PerfectJudgeRange &&
            diffG <= ApplicationConfigs.Config.GameConfig.PerfectJudgeRange &&
            diffB <= ApplicationConfigs.Config.GameConfig.PerfectJudgeRange)
        {
            return 100;
        }

        int score = 99;
        score -= (int)(diffR / ApplicationConfigs.Config.GameConfig.JudgeSeverity);
        score -= (int)(diffG / ApplicationConfigs.Config.GameConfig.JudgeSeverity);
        score -= (int)(diffB / ApplicationConfigs.Config.GameConfig.JudgeSeverity);
        return score;
    }


    private async Task ScoreAnim(int score)
    {
        const int Size = 170;
        int endSize = (int)(Size * (score / 100.0f));
        float oneSize = Size / 100.0f;
        float currentSize = 0;
        float time = 0.0f;
        int currentScore = 0;

        bool isEnd = false;
        while (!isEnd)
        {
            time += Time.deltaTime;

            while (time >= ApplicationConfigs.Config.DesignConfig.ScoreAnimSpeed) 
            {
                time -= ApplicationConfigs.Config.DesignConfig.ScoreAnimSpeed;
                currentSize += oneSize;
                var size = judgeScoreGaugeImage.rectTransform.sizeDelta;
                size.x = (int)currentSize;
                judgeScoreGaugeImage.rectTransform.sizeDelta = size;
                judgeScoreText.text = (++currentScore).ToString();
            }

            if (currentSize >= endSize)
            {
                isEnd = true;
            }

            await Task.Yield();
        }

        {
            var size = judgeScoreGaugeImage.rectTransform.sizeDelta;
            size.x = endSize;
            judgeScoreGaugeImage.rectTransform.sizeDelta = size;
            judgeScoreText.text = score.ToString();
        }

        gameState = GameState.JudgeAnimEnd;
    }


    public void TapNextGameImage() 
    {
        if (gameState == GameState.JudgeWait) 
        {
            judgeCanvasObj.SetActive(false);
            gameState = GameState.GameInit;
        }
    }

    public void TapEndImage()
    {
        if (gameState == GameState.JudgeWait)
        {
            SceneManager.LoadScene("TitleScene");
        }
    }

}
