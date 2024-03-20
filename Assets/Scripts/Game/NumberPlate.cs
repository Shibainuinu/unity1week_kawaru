using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NumberPlate : MonoBehaviour
{

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Text numberText;
    [SerializeField] private Sprite[] numberSpriteArray;
    [SerializeField] private GameObject levelUpParticle;
    [SerializeField] private Animator animator;

    private int myNumber;
    public int Number 
    {
        get { return myNumber; }
    }
    private const float Scale = 0.1f; // サイズの関係で一定の数値を割る

    private int posId = 99;
    public int PosId {
        get { return posId; }
    }

    private StateMachineMonitor monitor;

    private void Awake()
    {
        monitor = animator.GetBehaviour<StateMachineMonitor>();
    }


    public void SetNumber(int number)
    {
        myNumber = number;
        spriteRenderer.sprite = numberSpriteArray[((int)Mathf.Log(myNumber, 2))];
        numberText.text= number.ToString();
    }

    public void LevelUp()
    {
        myNumber *= 2;
        spriteRenderer.sprite = numberSpriteArray[((int)Mathf.Log(myNumber, 2))];
        numberText.text = myNumber.ToString();
        var obj = Instantiate(levelUpParticle, Vector3.zero, Quaternion.identity);
        obj.transform.parent = transform;
        obj.transform.localPosition = Vector3.zero;
    }


    public void SetSize( Vector2 size )
    {
        spriteRenderer.transform.localScale = size * Scale;
    }

    public void SetPos(Vector2 pos, int posId)
    {
        this.posId = posId;
        transform.localPosition = pos * Scale;
    }


    public async Task MoveProsses(Vector2 newPos, int newPosId, bool isLevelUp, bool isDestroy)
    {
        int x = posId % 4;
        int y = posId / 4;
        int newX = newPosId % 4;
        int newY = newPosId / 4;

        // 移動処理
        //Debug.Log($"MoveProsses {x},{y}→{newX},{newY} levelUp:{isLevelUp} destroy:{isDestroy}");

        posId = newPosId;

        await transform.DOLocalMove(newPos * Scale, ApplicationConfigs.Config.DesignConfig.PlateMoveSecond).AsyncWaitForCompletion();
        
        // レベルアップ処理

        if (isDestroy)
        {
            Destroy(gameObject);
        }

        if (isLevelUp)
        {
            LevelUp();
            await LevelUpAnimation();
        }

    }

    public async Task LevelUpAnimation()
    {
        bool isEnd = false;
        monitor.SetExitAction("Box", () => { isEnd = true; });
        animator.SetTrigger("Start");
        while (!isEnd)
        {
            await Task.Yield();
        }
    }


}
