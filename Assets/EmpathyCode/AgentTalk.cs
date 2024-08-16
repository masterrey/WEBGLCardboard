using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using TiltShift.Cardboard.Controls;
/// <summary>
/// this class is used to make the agent talk when the player looks at the agent
/// </summary>
public class AgentTalk : CardboardControlBase
{
    //array of audio clips
    public AnimationEvent[] animationEvents;

    //audio source
    private AudioSource audioSource;
    //reference to the agent's animator
    private Animator animator;
    //reference to another npc to talk to
    public static bool isPlaying = false;
    //state of the agent
    public int state = 0;

    public GameObject player;

    public TextMeshProUGUI text;

    float interpolation = 0;

    public delegate void CallBack();



    //function to play the event clip
    public IEnumerator PlayEvent(int i)
    {
        print("Playing event of "+ gameObject.name);
        isPlaying = true;
        IgnoreClick = true;

        //play the audio clip
        if (animationEvents[i].audioClip != null)
        {
                audioSource.clip = animationEvents[i].audioClip;
                audioSource.Play();
        }

        if (animationEvents[i].animations != null)
        {
            print("Playing animation of " + gameObject.name);
            //play the animation
            animator.Play(animationEvents[i].animations.name);
        }

        //if there is another npc to talk to
        if (animationEvents[i].triggerNPC)
        {
            //get the agent talk script of the other npc
            AgentTalk agentTalk = animationEvents[i].triggerNPC.GetComponent<AgentTalk>();
            //set the state of the other npc to the current state
            if (animationEvents[i].waitforNPC)
            {
                agentTalk.TrigeredPlayEvent(TrigeredPlayEvent);
            }
            else
            {
                agentTalk.TrigeredPlayEvent();
            }
        }

      
        //set the legent to the text
        if (text != null)
        {
            text.text = animationEvents[i].text;
            text.transform.rotation = player.transform.rotation;
            Invoke("ClearTheLegend", animationEvents[i].timeToWait*2);

        }
        print ("Waiting for " + animationEvents[i].timeToWait + " seconds");
        interpolation = 0;
        float total = animationEvents[i].timeToWait;
        Vector3 startpos = transform.position;
        Quaternion startRot = transform.rotation;
        while (animationEvents[i].timeToWait > 0)
        {
            
            if (animationEvents[i].lookAt != null)
            {
                //set the look at weight
                animator.SetLookAtWeight(interpolation);
                //set the look at position
                animator.SetLookAtPosition(animationEvents[i].lookAt.transform.position);
            }

            if (animationEvents[i].goTo)
            {
               //point the agent to the player slonly
               transform.rotation = Quaternion.LookRotation(player.transform.position - new Vector3(animationEvents[i].goTo.position.x,player.transform.position.y, animationEvents[i].goTo.position.z));
               
               transform.position = Vector3.Lerp(startpos, animationEvents[i].goTo.position, interpolation);
               transform.rotation = Quaternion.Slerp(startRot, animationEvents[i].goTo.rotation, interpolation);
            }

            interpolation += Time.deltaTime/ total;
            

            animationEvents[i].timeToWait -= Time.deltaTime;
            yield return null;
        }
        print("Finished waiting for " + animationEvents[i].timeToWait + " seconds");
        if(state < animationEvents.Length - 1)
        {
            
            state += 1;
            
            IgnoreClick = animationEvents[state].notActivateByPlayer;
            
            
        }
        StopAllCoroutines();
        if (state == animationEvents.Length)
        {
            this.enabled = false;
            
        }
        isPlaying = false;
    }
    IEnumerator WaitForisPlaying()
    {
        IgnoreClick = true;
        yield return new WaitUntil(() => isPlaying == false);
        IgnoreClick = false;
        StartCoroutine(PlayEvent(state));
    }

    //function to play the audio clip
    override public void OnClick(Vector3 vector)
    {
        if (!isPlaying)
        {
            StartCoroutine(PlayEvent(state));

        }
       

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
        //get the audio source component
        audioSource = GetComponent<AudioSource>();
        //get the animator component
        animator = GetComponent<Animator>(); 

        IgnoreClick = animationEvents[0].notActivateByPlayer;
    }

    void ClearTheLegend()
    {
        if (text != null)
        {
            text.text = "";
        }

    }


    //use the ik system to look at the player
    private void OnAnimatorIK(int layerIndex)
    {
        if(state < 0 || state >= animationEvents.Length)
        {
            return;
        }       
        if (animationEvents[state].lookAt != null)
        {
            
            //set the look at weight
            animator.SetLookAtWeight(interpolation);
            //set the look at position
            animator.SetLookAtPosition(animationEvents[state].lookAt.transform.position);
        }
    }
}
[System.Serializable]
public class  AnimationEvent
{
    //array of audio clips
    public AudioClip audioClip;
    public string text;
    //time to wait before playing the next clip
    public float timeToWait = 5f;
    //list of animations
    public AnimationClip animations;
    public GameObject triggerNPC;
    public bool waitforNPC = false;
    public GameObject lookAt;
    public bool notActivateByPlayer = false;
    public bool autoPlay = false;
    public Transform goTo;
     
}
