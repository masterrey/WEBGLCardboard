using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using TiltShift.Cardboard.Controls;
using System;
/// <summary>
/// this class is used to make the agent talk when the player looks at the agent
/// </summary>
public class AgentTalkMachine : MonoBehaviour
{
    //array of audio clips
    public IndividualAnimationEvent[] animationEvents;

    //reference to another npc to talk to
    public static bool isPlaying = false;
    //state of the agent
    public int state = 0;

    public GameObject player;

    float interpolation = 0;

    public delegate void CallBack();



    //function to play the event clip
    public IEnumerator PlayEvent(int i)
    {
        print("Playing event of "+ animationEvents[state].actor.name);
        isPlaying = true;
        animationEvents[i].agentEvent.IgnoreClick = true;

        //play the audio clip
        if (animationEvents[i].audioClip != null)
        {
            animationEvents[state].agentEvent.audioSource.clip = animationEvents[i].audioClip;
            animationEvents[state].agentEvent.audioSource.Play();
        }

        if (animationEvents[i].animations != null)
        {
            print("Playing animation of " + gameObject.name);
            //play the animation
            animationEvents[state].agentEvent.animator.Play(animationEvents[i].animations.name);
        }

        //set the legent to the text
        if (animationEvents[i].agentEvent.textmesh != null)
        {
            animationEvents[i].agentEvent.textmesh.text = animationEvents[i].text;
        }
        print ("Waiting for " + animationEvents[i].timeToWait + " seconds");
        interpolation = 0;
        float total = animationEvents[i].timeToWait;
        Vector3 startpos = animationEvents[i].actor.transform.position;
        Quaternion startRot = animationEvents[i].actor.transform.rotation;
        
        while (animationEvents[i].timeToWait > 0)
        {
            
            if (animationEvents[i].lookAt != null)
            {
                animationEvents[i].agentEvent.tolook = animationEvents[i].lookAt;
               
            }

            if (animationEvents[i].goTo)
            {

                //set the velocity of the agent by the distance to the target
                animationEvents[state].agentEvent.animator.SetFloat("velocity", Vector3.Distance(animationEvents[i].actor.transform.position, animationEvents[i].goTo.position));
                
                //point the agent to the player slonly
                animationEvents[i].actor.transform.rotation = Quaternion.LookRotation(player.transform.position - new Vector3(animationEvents[i].goTo.position.x,player.transform.position.y, animationEvents[i].goTo.position.z));
               
               animationEvents[i].actor.transform.position = Vector3.Lerp(startpos, animationEvents[i].goTo.position, interpolation);
                animationEvents[i].actor.transform.rotation = Quaternion.Slerp(startRot, animationEvents[i].goTo.rotation, interpolation);
            }

            interpolation += Time.deltaTime/ total;
            

            animationEvents[i].timeToWait -= Time.deltaTime;
            yield return null;
        }
        animationEvents[state].agentEvent.animator.SetFloat("velocity", 0);
        print("Finished waiting for " + animationEvents[i].timeToWait + " seconds");
        if(state < animationEvents.Length - 1)
        {
            
            state += 1;

            animationEvents[i].agentEvent.IgnoreClick = animationEvents[state].notActivateByPlayer;
            
            
        }
       
        if (state == animationEvents.Length)
        {
            this.enabled = false;
            StopAllCoroutines();
        }
        isPlaying = false;

       if (animationEvents[state].notActivateByPlayer)
        {
            TrigeredPlayEvent();
        }
    }
    IEnumerator WaitForisPlaying()
    {
        animationEvents[state].agentEvent.IgnoreClick = true;
        yield return new WaitUntil(() => isPlaying == false);
        animationEvents[state].agentEvent.IgnoreClick = false;
        StartCoroutine(PlayEvent(state));
    }

    //function to play the audio clip
    public void TrigeredPlayEvent()
    {
        if (!isPlaying)
        {
            StartCoroutine(PlayEvent(state));

        }else
        {
            StartCoroutine(WaitForisPlaying());
        }

    }

    //function to play the audio clip
    public void TrigeredPlayEvent(CallBack callBack)
    {
        if (!isPlaying)
        {
            StartCoroutine(PlayEvent(state));

        }
        else
        {
            callBack();
            StartCoroutine(WaitForisPlaying());
        }

    }

    // Start is called before the first frame update
    void Start()
    {

        
        foreach (IndividualAnimationEvent animationEvent in animationEvents)
        {
            animationEvent.Start();
        }
        
    }

    


    
}
[System.Serializable]
public class  IndividualAnimationEvent
{
    public void Start()
    {
        agentEvent = actor.GetComponent<AgentEvent>();

        agentEvent.IgnoreClick = notActivateByPlayer;
        
    }

    
    public AgentEvent agentEvent;
    public GameObject actor;
    
     
    //array of audio clips
    public AudioClip audioClip;
    public string text;
    //time to wait before playing the next clip
    public float timeToWait = 5f;
    //list of animations
    public AnimationClip animations;

    public GameObject lookAt;
    public bool notActivateByPlayer = false;
    public Transform goTo;
     
}
