using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.ParticleSystem;

public class StateMachineMonitor : StateMachineBehaviour
{

    private Dictionary<int, UnityEvent> enterEvent = new Dictionary<int, UnityEvent>();
    private Dictionary<int, UnityEvent> exitEvent = new Dictionary<int, UnityEvent>();
    private Dictionary<int, UnityEvent> updateEvent = new Dictionary<int, UnityEvent>();

    private (int hash, UnityAction action) enterAction = (0, null);
    private (int hash, UnityAction action) exitAction = (0, null);

    public void SetEnterAction(string name, UnityAction action)
    {
        int nameHash = Animator.StringToHash(name);
        enterAction = (nameHash, action);
    }

    public void SetExitAction(string name, UnityAction action)
    {
        int nameHash = Animator.StringToHash(name);
        exitAction = (nameHash, action);
    }

    public void SetEnterEvent(string name, UnityAction action) 
    {
        int nameHash = Animator.StringToHash(name);
        if (!enterEvent.ContainsKey(nameHash)) 
        {
            enterEvent[nameHash] = new UnityEvent();
        }
        enterEvent[nameHash].AddListener(action);
    }

    public void SetExitEvent(string name, UnityAction action)
    {
        int nameHash = Animator.StringToHash(name);
        if (!exitEvent.ContainsKey(nameHash))
        {
            exitEvent[nameHash] = new UnityEvent();
        }
        exitEvent[nameHash].AddListener(action);
    }

    public void SetUpdateEvent(string name, UnityAction action)
    {
        int nameHash = Animator.StringToHash(name);
        if (!updateEvent.ContainsKey(nameHash))
        {
            updateEvent[nameHash] = new UnityEvent();
        }
        updateEvent[nameHash].AddListener(action);
    }

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("OnStateEnter");
        if (enterEvent.ContainsKey(stateInfo.shortNameHash))
        {
            enterEvent[stateInfo.shortNameHash]?.Invoke();
        }

        if (enterAction.action != null) 
        {
            enterAction.action();
            enterAction.action = null;
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("OnStateExit");
        if (exitEvent.ContainsKey(stateInfo.shortNameHash))
        {
            exitEvent[stateInfo.shortNameHash]?.Invoke();
        }

        if (exitAction.action != null)
        {
            exitAction.action();
            exitAction.action = null;
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Debug.Log("OnStateUpdate");
        if (updateEvent.ContainsKey(stateInfo.shortNameHash))
        {
            updateEvent[stateInfo.shortNameHash]?.Invoke();
        }        
    }

    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    Debug.Log("OnStateMove");
    //}

    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    Debug.Log("OnStateIK");
    //}





}
