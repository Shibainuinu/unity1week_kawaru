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
        Judge,
        JudgeEnd,
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


    [SerializeField] private GameObject tapObj;


    private GameState gameState = GameState.None;

    private ColorId selectColorId = ColorId.None;
    private TempColorData currentTempColor;

    private bool isAddColor = false;
    private float addColorTime = 0.0f;
    private int judgeScore = 0;



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
                tapObj.SetActive(false);
                isAddColor = false;
                addColorTime = 0.0f;
                ResetSelectColor();
                UpdateTempColor();
                UpdateSelectColor();
                judgeCanvasObj.SetActive(false);
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

            case GameState.Judge:

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


    public void SelectColor(int color)
    {
        if (gameState == GameState.ColorSelect)
        {
            tapObj.SetActive(true);
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
            selectColorImage1.transform.parent.gameObject.SetActive(false);
            selectColorImage2.transform.parent.gameObject.SetActive(false);
        }
        else 
        {
            selectColorImage1.transform.parent.gameObject.SetActive(true);


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
        if (gameState == GameState.ColorSelect)
        {
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
        gameState = GameState.Judge;

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

        gameState = GameState.JudgeEnd;
    }


    public void TapNextGameImage() 
    {
        if (gameState == GameState.JudgeEnd) 
        {
            judgeCanvasObj.SetActive(false);
            gameState = GameState.GameInit;
        }
    }


}
