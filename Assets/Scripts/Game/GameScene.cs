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
using UnityEngine.UIElements;

public class GameScene : MonoBehaviour
{

    [SerializeField] private Canvas scoreCanvas;
    [SerializeField] private GameObject puzzleParent;
    [SerializeField] private GameObject numberPlatePrefab;

    [SerializeField] private int puzzleAreaWidth = 100;
    [SerializeField] private int puzzleAreaHeight = 100;            
    [SerializeField] private Vector2 spacing; // マスとマスの間

    [SerializeField] private Text scoreText1;
    [SerializeField] private Text scoreText2;

    [SerializeField] private ScoreControl scoreControl;


    private enum MoveDir { 
        None = 0,
        Left,
        Right,
        Up,
        Down,
    }

    private enum GameState
    {
        None = 0,       // 初期化時
        Input,          // 入力待機
        MoveStart,      // 移動処理
        MoveWait,       // 移動終了待機
        Create,         // 数字生成処理
        GameOverCheck,  // ゲームオーバー判定
        End,            // ゲーム終了後のリザルト画面
    }

    private GameState gameState = GameState.None;
    private Vector2[] squarePoints = new Vector2[16];   // 升目の座標データ
    private List<NumberPlate> numberPlateList = new List<NumberPlate>();
    private Vector2 plateSize = new Vector2();
    private int score;

    private void Awake()
    {
        // 初期化処理
        score = 0;
        SetScore(score);

        // 升目の座標を計算
        Vector2 parentPos = new Vector2( puzzleParent.gameObject.transform.position.x, puzzleParent.gameObject.transform.position.y);

        plateSize.x = (puzzleAreaWidth - (spacing.x * 1.5f)) / 4.0f;
        plateSize.y = (puzzleAreaHeight - (spacing.y * 1.5f)) / 4.0f;
        Debug.Log($"PlateSize x:{plateSize.x} y:{plateSize.y}");

        // マスの配置位置を計算していく
        for (int i = 0; i < 4; ++i) {
            for (int j = 0; j < 4; ++j) {
                // 隙間の計算
                float sx = spacing.x * (j % 3 == 0 ? 1.5f : 0.5f);
                float sy = spacing.y * (i % 3 == 0 ? 1.5f : 0.5f);

                // プレート分の計算
                float px = plateSize.x * (j % 3 == 0 ? 1.5f : 0.5f);
                float py = plateSize.y * (i % 3 == 0 ? 1.5f : 0.5f);

                // 合算
                float x = (sx + px) * (j < 2 ? -1.0f : 1.0f);
                float y = (sy + py) * (i < 2 ? 1.0f : -1.0f);   // 座標系が3Dなのでyを反転
                squarePoints[i*4+j] = new Vector2(x, y);
            }
        }

        // 初期状態はとりあえず2つ生成しておく
        CreatePlate();
        CreatePlate();

        // 初期化終了時は入力待機にしておく
        gameState = GameState.Input;
    }

    async void Update()
    {

        switch (gameState) 
        {
            case GameState.Input:           // 入力待機
                if (move != (int)MoveDir.None) {

                    // 移動可能かチェック
                    if (CheckPossibleMove((MoveDir)move))
                    {
                        // 移動可能
                        MovePlate((MoveDir)move);
                    }
                    else 
                    {
                        // 移動不可は再度入力待機
                        move = (int)MoveDir.None;
                    }
                }
                break;
            case GameState.MoveStart:       // 移動処理
                break;
            case GameState.MoveWait:        // 移動終了待機
                break;
            case GameState.Create:          // 数字生成処理
                CreatePlate();
                //score += 1;
                //SetScore(score);
                gameState = GameState.GameOverCheck;
                break;
            case GameState.GameOverCheck:   // ゲームオーバー判定
                if (GameOverCheck())
                {
                    await Task.Delay( (int)(1000 * ApplicationConfigs.Config.DesignConfig.GameOverWaitSecond));
                    // スコア集計
                    //SetScore(1000);

                    // リザルトUI表示
                    OpenScoreboard();

                    // ステート変更
                    gameState = GameState.End;
                }
                else 
                {
                    // 入力待機へ
                    move = 0;
                    gameState = GameState.Input;
                }
                break;
            case GameState.End:             // ゲーム終了後のリザルト画面
                // 基本何もしない
                break;
        }
    }

    int move = 0;
    public void TestMoveButton(int m)
    {
        move = m;
    }

    bool isDrag = false;
    Vector2 dragVec;
    public void PointerDown(BaseEventData data) 
    {
        isDrag = true;
        dragVec = Vector2.zero;
    }

    public void OnDrag(BaseEventData data)
    {
        if (gameState != GameState.Input || !isDrag) return;

        var pointer =  (PointerEventData)data;
        dragVec += pointer.delta;
        if (dragVec.magnitude >= ApplicationConfigs.Config.DesignConfig.PlateMoveDragAmount) {
            bool isUp = (dragVec.y > 0);
            if (dragVec.x < 0)
            {
                if (Mathf.Abs(dragVec.x) < Mathf.Abs(dragVec.y))
                {
                    move = (isUp)?(int)MoveDir.Up:(int)MoveDir.Down;
                }
                else 
                {
                    move = (int)MoveDir.Left;
                }
            }
            else 
            {
                if (Mathf.Abs(dragVec.x) < Mathf.Abs(dragVec.y))
                {
                    move = (isUp) ? (int)MoveDir.Up : (int)MoveDir.Down;
                }
                else
                {
                    move = (int)MoveDir.Right;
                }
            }
            isDrag = false;
            dragVec = Vector2.zero;
        }
    }
    public void PointerUp(BaseEventData data)
    {
        isDrag = false;
        dragVec = Vector2.zero;
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

    // 移動可能かチェック
    private bool CheckPossibleMove(MoveDir dir) {

        if (dir == MoveDir.None) {
            Debug.LogError("ありえない値が送られている");
            return false;
        }

        foreach (var plate in numberPlateList)
        {
            if (CheckPossibleMovePlate(plate.PosId, dir)) {
                // １つでも動けるプレートがある場合は移動できる
                return true;
            }
        }

        return false;
    }

    // プレート単体で移動可能かチェック
    private bool CheckPossibleMovePlate(int fromPosId, MoveDir dir) {
        var currentPlateArray = GetCurrentPlateArray();

        // 進めるかの判断は自分の1マス上が空いているか、自分と同じコマ
        int x = fromPosId % 4;
        int y = fromPosId / 4;
        switch (dir) {
            case MoveDir.Up:    y -= 1; break;
            case MoveDir.Down:  y += 1; break;
            case MoveDir.Left:  x -= 1; break;
            case MoveDir.Right: x += 1; break;
        }

        // 数値が有効かチェック
        if (!((0 <= x && x <= 3) && (0 <= y && y <= 3))) {
            // 数値が向こうの場合は移動不可
            return false;
        }

        int toPosId = x + (y * 4);

        // コマが存在しない場合は移動可能
        if (currentPlateArray[toPosId] == null) {
            return true;
        }

        // 同じコマかチェック
        if (currentPlateArray[fromPosId].Number == currentPlateArray[toPosId].Number) {
            return true;
        }
        
        return false;
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

    // 移動処理
    private async Task MovePlate(MoveDir dir) {

        var tasks = new List<Task>();
        //var moveDataList = new List<MoveData>();
        //var moveDataDic = new Dictionary<NumberPlate, MoveData>();

        // 現在の盤面
        var currentPlateArray = GetCurrentPlateArray();

        // 計算しやすいように盤面を回転
        var rotatePlateArray = RotatePlateArray(currentPlateArray, dir);

        // 移動情報を計算
        // 盤面の回転を行ったので上方向への計算のみ行えばよい
        var moveDataDic = CreateMoveDataUp(rotatePlateArray);

        // 回転を戻す
        moveDataDic = RotateMoveData(moveDataDic, dir);

        var addPoint = 0;
        // 計算結果に応じて移動処理
        foreach (KeyValuePair<NumberPlate, MoveData> item in moveDataDic)
        {
            //var data = moveDataList.Find(d => d.plate == plate);
            int posId = item.Value.x + item.Value.y * 4;
            addPoint += item.Value.addPoint;
            tasks.Add(item.Key.MoveProsses(squarePoints[posId], posId, item.Value.isLevelUp, item.Value.isDestroy));
        }

        // リストからも除外する
        foreach (KeyValuePair<NumberPlate, MoveData> item in moveDataDic)
        {
            if (item.Value.isDestroy) 
            {
                numberPlateList.Remove(item.Key);
            }
        }


        gameState = GameState.MoveWait;

        // 移動と演出が終了するまで待機
        await Task.WhenAll(tasks);

        if (addPoint != 0) {
            var obj = new GameObject("SE");
            var playSE = obj.AddComponent<PlaySE>();
            playSE.PlaySe( ApplicationConfigs.Config.SeConfig.LevelUpSeAudioClip , FindObjectOfType<SoundManager>().volume);
        }

        // 得点加算
        score += addPoint;
        SetScore(score);

        gameState = GameState.Create;
    }

    private NumberPlate[] RotatePlateArray(NumberPlate[] plateArray, MoveDir dir)
    {
        // 上方向はそのまま返す
        if (dir == MoveDir.Up) {
            return plateArray;
        }

        // 現在のプレートの配置を計算
        NumberPlate[] rotatePlateArray = new NumberPlate[16];
        for (int i = 0; i < 16; ++i)
        {
            rotatePlateArray[i] = null;
        }

        for (int i = 0; i < 16; ++i)
        {
            // 変換前
            var x = i % 4;
            var y = i / 4;

            var newX = 0; var newY = 0;
            switch (dir)
            {
                case MoveDir.Left:  // 右回転
                    newX = 3 - y;
                    newY = x;
                    break;
                case MoveDir.Right:  // 左回転
                    newX = y;
                    newY = 3 - x;
                    break;
                case MoveDir.Down:  // 垂直方向で反転
                    newX = x;
                    newY = 3 - y;
                    break;
            }
            rotatePlateArray[newX + (newY * 4)] = plateArray[i];
        }

        return rotatePlateArray;
    }

    Dictionary<NumberPlate, MoveData> RotateMoveData(Dictionary<NumberPlate, MoveData> moveDataDic, MoveDir dir)
    {
        // 上方向はそのまま返す
        if (dir == MoveDir.Up)
        {
            return moveDataDic;
        }

        List<NumberPlate> keyList = new List<NumberPlate>(moveDataDic.Keys);

        foreach (var plate in keyList)
        {
            var data = moveDataDic[plate];
            
            // 変換前
            var x = data.x;
            var y = data.y;

            var newX = 0; var newY = 0;
            switch (dir)
            {
                case MoveDir.Left:  // 左回転
                    newX = y;
                    newY = 3 - x;
                    break;
                case MoveDir.Right:  // 右回転
                    newX = 3 - y;
                    newY = x;
                    break;
                case MoveDir.Down:  // 垂直方向で反転
                    newX = x;
                    newY = 3 - y;
                    break;
            }
            data.x = newX;
            data.y = newY;
            moveDataDic[plate] = data;
        }

        return moveDataDic;
    }


    // 盤面の回転を行い、上方向への移動のみ考えればよい
    private Dictionary<NumberPlate, MoveData> CreateMoveDataUp(NumberPlate[] currentPlateArray )
    {
        var moveDataDic = new Dictionary<NumberPlate, MoveData>();
        NumberPlate[] newPlateArray = new NumberPlate[16];

        // 縦で調べる
        for (int x = 0; x < 4; ++x) 
        {
            for (int y = 0; y < 4; ++y) 
            {
                var plate = currentPlateArray[x + (y * 4)];

                if (plate == null) {
                    continue;
                }

                int tempY = y;  // 現段階で移動予定の座標
                int checkY = y-1; // 現在チェックしている座標

                while (0 <= checkY)
                {
                    // チェック先のプレート取得
                    var checkPlate = newPlateArray[x + (checkY * 4)];

                    if (checkPlate == null)
                    {
                        // 何もない場合は移動予定を更新
                        tempY = checkY;
                    }
                    else 
                    {
                        // 何かある場合は合体できるかチェック
                        if (!moveDataDic[checkPlate].isLevelUp && checkPlate.Number == plate.Number)
                        {
                            // 合体される側のプレートを削除
                            {
                                var temp = moveDataDic[checkPlate];
                                temp.isDestroy = true;
                                moveDataDic[checkPlate] = temp;
                            }

                            // 合体の処理
                            var moveData = new MoveData();
                            moveData.x = x;
                            moveData.y = checkY;
                            moveData.isLevelUp = true;
                            moveData.isDestroy = false;
                            moveData.addPoint = plate.Number * 4;
                            moveDataDic[plate] = moveData;

                            // 盤面を上書き
                            newPlateArray[x + (checkY * 4)] = plate;
                            break;
                        }
                        else 
                        {
                            // 合体出来ない場合はループを終了
                            break;
                        }
                    }
                    checkY--;
                }
                
                if (!moveDataDic.ContainsKey(plate))
                {
                    // 合体できるものがなく検索が終了した場合
                    var moveData = new MoveData();
                    moveData.x = x;
                    moveData.y = tempY;
                    moveData.isLevelUp = false;
                    moveData.isDestroy = false;
                    moveData.addPoint = 0;
                    moveDataDic[plate] = moveData;
                    newPlateArray[x + (tempY * 4)] = plate;
                }
            }
        }

        return moveDataDic;
    }


    private bool GameOverCheck()
    {
        return !CheckPossibleMove(MoveDir.Up) && !CheckPossibleMove(MoveDir.Down) && !CheckPossibleMove(MoveDir.Left) && !CheckPossibleMove(MoveDir.Right);
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
