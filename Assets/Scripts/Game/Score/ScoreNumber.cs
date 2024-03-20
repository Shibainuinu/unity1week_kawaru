using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class ScoreNumber : MonoBehaviour
{
    [SerializeField]
    private Text currentNumText;

    [SerializeField]
    private Text nextNumText;

    private int currentNum = 0;
    public int CurrentNum 
    {
        get { return currentNum; }
    }
    private int nextNum = 0;
    private bool isAnimation = false;

    private Animator animator;
    private StateMachineMonitor monitor;


    private void Awake()
    {
        animator = GetComponent<Animator>();
        monitor = animator.GetBehaviour<StateMachineMonitor>();
        monitor.SetExitEvent("CountAnimation", EndAnimation);
    }



    public void Init( int num )
    {
        currentNum = num;
    }

    async public Task ChangeNum(int num)
    {
        nextNum = num;

        nextNumText.text = nextNum.ToString();

        // �A�j���[�V�����X�^�[�g
        isAnimation = true;
        animator.SetTrigger("Start");

        // �I���܂őҋ@
        while (isAnimation)
        {
            await Task.Yield();
        }
    }

    private void EndAnimation()
    {
        isAnimation = false;
        // �V�����l��ݒ�
        currentNum = nextNum;
        currentNumText.text = nextNumText.text;
    }
}
