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
    [SerializeField] private Vector2 spacing; // �}�X�ƃ}�X�̊�

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
        None = 0,       // ��������
        Input,          // ���͑ҋ@
        MoveStart,      // �ړ�����
        MoveWait,       // �ړ��I���ҋ@
        Create,         // ������������
        GameOverCheck,  // �Q�[���I�[�o�[����
        End,            // �Q�[���I����̃��U���g���
    }

    private GameState gameState = GameState.None;
    private Vector2[] squarePoints = new Vector2[16];   // ���ڂ̍��W�f�[�^
    private List<NumberPlate> numberPlateList = new List<NumberPlate>();
    private Vector2 plateSize = new Vector2();
    private int score;

    private void Awake()
    {
        // ����������
        score = 0;
        SetScore(score);

        // ���ڂ̍��W���v�Z
        Vector2 parentPos = new Vector2( puzzleParent.gameObject.transform.position.x, puzzleParent.gameObject.transform.position.y);

        plateSize.x = (puzzleAreaWidth - (spacing.x * 1.5f)) / 4.0f;
        plateSize.y = (puzzleAreaHeight - (spacing.y * 1.5f)) / 4.0f;
        Debug.Log($"PlateSize x:{plateSize.x} y:{plateSize.y}");

        // �}�X�̔z�u�ʒu���v�Z���Ă���
        for (int i = 0; i < 4; ++i) {
            for (int j = 0; j < 4; ++j) {
                // ���Ԃ̌v�Z
                float sx = spacing.x * (j % 3 == 0 ? 1.5f : 0.5f);
                float sy = spacing.y * (i % 3 == 0 ? 1.5f : 0.5f);

                // �v���[�g���̌v�Z
                float px = plateSize.x * (j % 3 == 0 ? 1.5f : 0.5f);
                float py = plateSize.y * (i % 3 == 0 ? 1.5f : 0.5f);

                // ���Z
                float x = (sx + px) * (j < 2 ? -1.0f : 1.0f);
                float y = (sy + py) * (i < 2 ? 1.0f : -1.0f);   // ���W�n��3D�Ȃ̂�y�𔽓]
                squarePoints[i*4+j] = new Vector2(x, y);
            }
        }

        // ������Ԃ͂Ƃ肠����2�������Ă���
        CreatePlate();
        CreatePlate();

        // �������I�����͓��͑ҋ@�ɂ��Ă���
        gameState = GameState.Input;
    }

    async void Update()
    {

        switch (gameState) 
        {
            case GameState.Input:           // ���͑ҋ@
                if (move != (int)MoveDir.None) {

                    // �ړ��\���`�F�b�N
                    if (CheckPossibleMove((MoveDir)move))
                    {
                        // �ړ��\
                        MovePlate((MoveDir)move);
                    }
                    else 
                    {
                        // �ړ��s�͍ēx���͑ҋ@
                        move = (int)MoveDir.None;
                    }
                }
                break;
            case GameState.MoveStart:       // �ړ�����
                break;
            case GameState.MoveWait:        // �ړ��I���ҋ@
                break;
            case GameState.Create:          // ������������
                CreatePlate();
                //score += 1;
                //SetScore(score);
                gameState = GameState.GameOverCheck;
                break;
            case GameState.GameOverCheck:   // �Q�[���I�[�o�[����
                if (GameOverCheck())
                {
                    await Task.Delay( (int)(1000 * ApplicationConfigs.Config.DesignConfig.GameOverWaitSecond));
                    // �X�R�A�W�v
                    //SetScore(1000);

                    // ���U���gUI�\��
                    OpenScoreboard();

                    // �X�e�[�g�ύX
                    gameState = GameState.End;
                }
                else 
                {
                    // ���͑ҋ@��
                    move = 0;
                    gameState = GameState.Input;
                }
                break;
            case GameState.End:             // �Q�[���I����̃��U���g���
                // ��{�������Ȃ�
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

    // �i���o�[�������_���Ő�������
    private void CreatePlate()
    {
        // ������
        bool[] points = new bool[16];
        for (int i = 0; i < 16; ++i)
        {
            points[i] = true;
        }

        // ���łɒu���Ă��鏊������
        foreach (var plate in numberPlateList) 
        {
            points[plate.PosId] = false;
        }

        // �u����ꏊ�̈ʒu������
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

    // �ړ��\���`�F�b�N
    private bool CheckPossibleMove(MoveDir dir) {

        if (dir == MoveDir.None) {
            Debug.LogError("���肦�Ȃ��l�������Ă���");
            return false;
        }

        foreach (var plate in numberPlateList)
        {
            if (CheckPossibleMovePlate(plate.PosId, dir)) {
                // �P�ł�������v���[�g������ꍇ�͈ړ��ł���
                return true;
            }
        }

        return false;
    }

    // �v���[�g�P�̂ňړ��\���`�F�b�N
    private bool CheckPossibleMovePlate(int fromPosId, MoveDir dir) {
        var currentPlateArray = GetCurrentPlateArray();

        // �i�߂邩�̔��f�͎�����1�}�X�オ�󂢂Ă��邩�A�����Ɠ����R�}
        int x = fromPosId % 4;
        int y = fromPosId / 4;
        switch (dir) {
            case MoveDir.Up:    y -= 1; break;
            case MoveDir.Down:  y += 1; break;
            case MoveDir.Left:  x -= 1; break;
            case MoveDir.Right: x += 1; break;
        }

        // ���l���L�����`�F�b�N
        if (!((0 <= x && x <= 3) && (0 <= y && y <= 3))) {
            // ���l���������̏ꍇ�͈ړ��s��
            return false;
        }

        int toPosId = x + (y * 4);

        // �R�}�����݂��Ȃ��ꍇ�͈ړ��\
        if (currentPlateArray[toPosId] == null) {
            return true;
        }

        // �����R�}���`�F�b�N
        if (currentPlateArray[fromPosId].Number == currentPlateArray[toPosId].Number) {
            return true;
        }
        
        return false;
    }

    // �v�Z�p�̔Ֆʂ̔z��𐶐�
    private NumberPlate[] GetCurrentPlateArray()
    {
        // ���݂̃v���[�g�̔z�u���v�Z
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

    // �ړ�����
    private async Task MovePlate(MoveDir dir) {

        var tasks = new List<Task>();
        //var moveDataList = new List<MoveData>();
        //var moveDataDic = new Dictionary<NumberPlate, MoveData>();

        // ���݂̔Ֆ�
        var currentPlateArray = GetCurrentPlateArray();

        // �v�Z���₷���悤�ɔՖʂ���]
        var rotatePlateArray = RotatePlateArray(currentPlateArray, dir);

        // �ړ������v�Z
        // �Ֆʂ̉�]���s�����̂ŏ�����ւ̌v�Z�̂ݍs���΂悢
        var moveDataDic = CreateMoveDataUp(rotatePlateArray);

        // ��]��߂�
        moveDataDic = RotateMoveData(moveDataDic, dir);

        var addPoint = 0;
        // �v�Z���ʂɉ����Ĉړ�����
        foreach (KeyValuePair<NumberPlate, MoveData> item in moveDataDic)
        {
            //var data = moveDataList.Find(d => d.plate == plate);
            int posId = item.Value.x + item.Value.y * 4;
            addPoint += item.Value.addPoint;
            tasks.Add(item.Key.MoveProsses(squarePoints[posId], posId, item.Value.isLevelUp, item.Value.isDestroy));
        }

        // ���X�g��������O����
        foreach (KeyValuePair<NumberPlate, MoveData> item in moveDataDic)
        {
            if (item.Value.isDestroy) 
            {
                numberPlateList.Remove(item.Key);
            }
        }


        gameState = GameState.MoveWait;

        // �ړ��Ɖ��o���I������܂őҋ@
        await Task.WhenAll(tasks);

        if (addPoint != 0) {
            var obj = new GameObject("SE");
            var playSE = obj.AddComponent<PlaySE>();
            playSE.PlaySe( ApplicationConfigs.Config.SeConfig.LevelUpSeAudioClip , FindObjectOfType<SoundManager>().volume);
        }

        // ���_���Z
        score += addPoint;
        SetScore(score);

        gameState = GameState.Create;
    }

    private NumberPlate[] RotatePlateArray(NumberPlate[] plateArray, MoveDir dir)
    {
        // ������͂��̂܂ܕԂ�
        if (dir == MoveDir.Up) {
            return plateArray;
        }

        // ���݂̃v���[�g�̔z�u���v�Z
        NumberPlate[] rotatePlateArray = new NumberPlate[16];
        for (int i = 0; i < 16; ++i)
        {
            rotatePlateArray[i] = null;
        }

        for (int i = 0; i < 16; ++i)
        {
            // �ϊ��O
            var x = i % 4;
            var y = i / 4;

            var newX = 0; var newY = 0;
            switch (dir)
            {
                case MoveDir.Left:  // �E��]
                    newX = 3 - y;
                    newY = x;
                    break;
                case MoveDir.Right:  // ����]
                    newX = y;
                    newY = 3 - x;
                    break;
                case MoveDir.Down:  // ���������Ŕ��]
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
        // ������͂��̂܂ܕԂ�
        if (dir == MoveDir.Up)
        {
            return moveDataDic;
        }

        List<NumberPlate> keyList = new List<NumberPlate>(moveDataDic.Keys);

        foreach (var plate in keyList)
        {
            var data = moveDataDic[plate];
            
            // �ϊ��O
            var x = data.x;
            var y = data.y;

            var newX = 0; var newY = 0;
            switch (dir)
            {
                case MoveDir.Left:  // ����]
                    newX = y;
                    newY = 3 - x;
                    break;
                case MoveDir.Right:  // �E��]
                    newX = 3 - y;
                    newY = x;
                    break;
                case MoveDir.Down:  // ���������Ŕ��]
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


    // �Ֆʂ̉�]���s���A������ւ̈ړ��̂ݍl����΂悢
    private Dictionary<NumberPlate, MoveData> CreateMoveDataUp(NumberPlate[] currentPlateArray )
    {
        var moveDataDic = new Dictionary<NumberPlate, MoveData>();
        NumberPlate[] newPlateArray = new NumberPlate[16];

        // �c�Œ��ׂ�
        for (int x = 0; x < 4; ++x) 
        {
            for (int y = 0; y < 4; ++y) 
            {
                var plate = currentPlateArray[x + (y * 4)];

                if (plate == null) {
                    continue;
                }

                int tempY = y;  // ���i�K�ňړ��\��̍��W
                int checkY = y-1; // ���݃`�F�b�N���Ă�����W

                while (0 <= checkY)
                {
                    // �`�F�b�N��̃v���[�g�擾
                    var checkPlate = newPlateArray[x + (checkY * 4)];

                    if (checkPlate == null)
                    {
                        // �����Ȃ��ꍇ�͈ړ��\����X�V
                        tempY = checkY;
                    }
                    else 
                    {
                        // ��������ꍇ�͍��̂ł��邩�`�F�b�N
                        if (!moveDataDic[checkPlate].isLevelUp && checkPlate.Number == plate.Number)
                        {
                            // ���̂���鑤�̃v���[�g���폜
                            {
                                var temp = moveDataDic[checkPlate];
                                temp.isDestroy = true;
                                moveDataDic[checkPlate] = temp;
                            }

                            // ���̂̏���
                            var moveData = new MoveData();
                            moveData.x = x;
                            moveData.y = checkY;
                            moveData.isLevelUp = true;
                            moveData.isDestroy = false;
                            moveData.addPoint = plate.Number * 4;
                            moveDataDic[plate] = moveData;

                            // �Ֆʂ��㏑��
                            newPlateArray[x + (checkY * 4)] = plate;
                            break;
                        }
                        else 
                        {
                            // ���̏o���Ȃ��ꍇ�̓��[�v���I��
                            break;
                        }
                    }
                    checkY--;
                }
                
                if (!moveDataDic.ContainsKey(plate))
                {
                    // ���̂ł�����̂��Ȃ��������I�������ꍇ
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
