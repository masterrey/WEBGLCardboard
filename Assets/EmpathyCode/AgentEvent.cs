using System;
using System.Collections;
using System.Collections.Generic;
using TiltShift.Cardboard.Controls;
using TMPro;
using UnityEngine;

public class AgentEvent : CardboardControlBase
{
    AgentTalkMachine talkMachine;
    public GameObject tolook;
    GameObject oldlook;
    public TextMeshProUGUI textmesh;

    public AudioSource audioSource;

    public Animator animator;

    public GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        talkMachine = FindObjectOfType<AgentTalkMachine>();
        //get the audio source component
        audioSource = GetComponent<AudioSource>();
        //get the animator component
        animator = GetComponent<Animator>();

        textmesh = GetComponentInChildren<TextMeshProUGUI>();

        //find object with tag player
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
       textmesh.transform.rotation = player.transform.rotation;


        
    }

    public IEnumerator ClearTheLegend(float timetowait)
    {
        yield return new WaitForSeconds(timetowait * 2);
        if (textmesh != null)
        {
            textmesh.text = "";
        }

    }
    float interpolation = 0;
    //use the ik system to look at the player
    private void OnAnimatorIK(int layerIndex)
    {
        
        if (tolook != null)
        {
            //if is looking for a new object interpolate the look at weight
            if (tolook != oldlook)
            {
                interpolation = 0;
                oldlook = tolook;
            }
            //interpolate the look at weight
            interpolation = Mathf.Lerp(interpolation, 1, Time.deltaTime);

            //set the look at weight
            animator.SetLookAtWeight(interpolation);
            //set the look at position
            animator.SetLookAtPosition(tolook.transform.position);
        }
    }

    //function to play the audio clip
    override public void OnClick(Vector3 vector)
    {

        talkMachine.StartCoroutine(talkMachine.PlayEvent(talkMachine.state));

    }

    override public void OnCursorHover(Vector3 vector)
    {
        //check if is time of this agent to play
        if (talkMachine.animationEvents[talkMachine.state].agentEvent == this)
        {
           
        }else
        {
            IgnoreClick = true;
        }
    }
}
