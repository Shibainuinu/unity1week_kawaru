using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        ColorSelect,
        AddColor,
        Judge,
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

    [SerializeField] private Image tempColorImage;
    [SerializeField] private Image selectColorImage;



    private GameState gameState = GameState.None;

    private ColorId selectColorId = ColorId.None;
    private TempColorData currentTempColor;

    private bool isAddColor = false;
    private float addColorTime = 0.0f;


    [SerializeField] private Canvas scoreCanvas;
    [SerializeField] private GameObject puzzleParent;
    [SerializeField] private GameObject numberPlatePrefab;

    [SerializeField] private int puzzleAreaWidth = 100;
    [SerializeField] private int puzzleAreaHeight = 100;            
    [SerializeField] private Vector2 spacing; // マスとマスの間

    [SerializeField] private Text scoreText1;
    [SerializeField] private Text scoreText2;

    [SerializeField] private ScoreControl scoreControl;






    private Vector2[] squarePoints = new Vector2[16];   // 升目の座標データ
    private List<NumberPlate> numberPlateList = new List<NumberPlate>();
    private Vector2 plateSize = new Vector2();
    private int score;

    private void Awake()
    {




        // 初期化終了時は入力待機にしておく
        gameState = GameState.GameInit;
    }

    async void Update()
    {

        switch (gameState) 
        {
            case GameState.GameInit:
                currentTempColor.c = currentTempColor.m = currentTempColor.y = 0.0f;
                selectColorId = ColorId.None;
                isAddColor = false;
                addColorTime = 0.0f;
                ResetSelectColor();
                UpdateSelectColor();
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
        }
    }


    public void SelectColor(int color)
    {
        if (gameState == GameState.ColorSelect)
        {
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
            selectColorImage.gameObject.SetActive(false);
        }
        else 
        {
            selectColorImage.gameObject.SetActive(true);


            switch (selectColorId) 
            {
                case ColorId.Cyan:
                    selectColorImage.color = cyanPlate.GetColor();
                    cyanPlate.Select(true);
                    break;
                case ColorId.Magenta:
                    selectColorImage.color = magentaPlate.GetColor();
                    magentaPlate.Select(true);
                    break;
                case ColorId.Yellow:
                    selectColorImage.color = yellowPlate.GetColor();
                    yellowPlate.Select(true);
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
            gameState = GameState.AddColor;
        }
    }


    public void TapImagePointerUp()
    {
        if (gameState == GameState.AddColor)
        {
            isAddColor = false;
            gameState = GameState.ColorSelect;
        }
    }





    // ナンバーをランダムで生成する
    private void CreatePlate()
    {
        // 初期化
        bool[] points = new bool[16];
        for (int i = 0; i < 16; ++i)
        {
            points[i] = true;
        }

        // すでに置いてある所を検索
        foreach (var plate in numberPlateList) 
        {
            points[plate.PosId] = false;
        }

        // 置ける場所の位置を検索
        List<int> ids = new List<int>();
        for (int i = 0; i < 16; ++i)
        {
            if (points[i]) { 
                ids.Add(i);
            }
        }

        int createPosId = ids[Random.Range(0, ids.Count)];

        NumberPlate numberPlate = Instantiate(numberPlatePrefab, puzzleParent.transform).GetComponent<NumberPlate>();
        numberPlate.SetSize(plateSize);
        numberPlate.SetNumber(1);
        numberPlate.SetPos(squarePoints[createPosId], createPosId);
        numberPlateList.Add(numberPlate);
    }



    // 計算用の盤面の配列を生成
    private NumberPlate[] GetCurrentPlateArray()
    {
        // 現在のプレートの配置を計算
        NumberPlate[] plateArray = new NumberPlate[16];
        for (int i = 0; i < 16; ++i)
        {
            plateArray[i] = null;
        }

        foreach (var plate in numberPlateList)
        {
            plateArray[plate.PosId] = plate;
        }

        return plateArray;
    }

    private struct MoveData
    {
        //public NumberPlate plate;
        //public int newPosId;
        //public Vector2 vec2;
        public int x;
        public int y;
        public bool isLevelUp;
        public bool isDestroy;
        public int addPoint;
    }





    public void SetScore(int score)
    {
        scoreControl.SetScore(score);
        scoreText1.text = score.ToString();
        scoreText2.text = score.ToString();
        //Debug.Log(score);
    }

    public void OpenScoreboard()
    {
        scoreCanvas.gameObject.SetActive(true);
    }

    public void OnTapEndButton()
    {
        SceneManager.LoadScene("TitleScene");
    }



    //private Vector2 ConvertPosIdToVector2( int posId )
    //{
    //    return new Vector2(posId%4, posId/4);
    //}

    //private Vector2 ConvertVector2ToPosId(Vector2 vec2)
    //{
    //    return vec2.x + (vec2.y * 4);
    //}


    //private bool CheckValidVec(Vector2 vec) 
    //{
    //    return (0 <= vec.x && vec.x <= 3) && (0 <= vec.y && vec.y <= 3);
    //}

}
