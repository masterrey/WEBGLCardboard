using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
/// <summary>
/// this class is used to make the agent talk when the player looks at the agent
/// </summary>
public class AgentTalk : MonoBehaviour
{
    //array of audio clips
    public AnimationEvent[] animationEvents;

    //audio source
    private AudioSource audioSource;
    //reference to the agent's animator
    private Animator animator;
    //reference to another npc to talk to
    public GameObject otherNpc;
    //state of the agent
    public int state = 0;

    public TextMeshProUGUI text;

    float oldtime;

    //function to play the event clip
    public void PlayEvent(int i)
    {
        
        //check if time to wait has passed
        if (Time.time - oldtime < animationEvents[i].timeToWait)
        {
            return;
        }
        //if the audio source is not playing
        if (!audioSource.isPlaying)
        {
            //play the audio clip
            if (animationEvents[i].audioClip != null)
            {
                audioSource.clip = animationEvents[i].audioClip;
                audioSource.Play();
            }
        }

        if (animationEvents[i].lookAt != null)
        {
            //set the agent to look at the thing
            //transform.LookAt(animationEvents[i].lookAt.transform);
        }
        //play the animation
        animator.Play(animationEvents[i].animations.name);

        //if there is another npc to talk to
        if (otherNpc != null && animationEvents[i].triggerNPC)
        {
            //get the agent talk script of the other npc
            AgentTalk agentTalk = otherNpc.GetComponent<AgentTalk>();
            //set the state of the other npc to the current state
            agentTalk.PlayEvent();
        }
        //set the legent to the text
        if (text != null)
            text.text = animationEvents[i].text;

        //set the time to the current time
        oldtime = Time.time;
    }

    //function to play the audio clip
    public void PlayEvent()
    {
        //check if time to wait has passed
        if (Time.time - oldtime < animationEvents[state].timeToWait)
        {
            return;
        }
        PlayEvent(state);
        state++;
    }

    // Start is called before the first frame update
    void Start()
    {
        //get the audio source component
        audioSource = GetComponent<AudioSource>();
        //get the animator component
        animator = GetComponent<Animator>();
        oldtime = Time.time;    
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    //use the ik system to look at the player
    private void OnAnimatorIK(int layerIndex)
    {
        if (animationEvents[state].lookAt != null)
        {
            
            //set the look at weight
            animator.SetLookAtWeight(1);
            //set the look at position
            animator.SetLookAtPosition(Camera.main.transform.position);
        }
    }
}
[System.Serializable]
public class  AnimationEvent
{
    //array of audio clips
    public AudioClip audioClip;
    public string text;
    //time to wait before playing the next audio clip
    public float timeToWait = 5f;
    //list of animations
    public AnimationClip animations;
    public bool triggerNPC = false;
    public GameObject lookAt;

}
