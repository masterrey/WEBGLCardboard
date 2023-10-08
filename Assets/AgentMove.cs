using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentMove : MonoBehaviour
{
    public GameObject player;
    public NavMeshAgent agent;
    internal void moveto()
    {
        agent.SetDestination(transform.position);
    }

    // Start is called before the first frame update
    void Start()
    {

        player = GameObject.Find("Player");
        agent = player.GetComponent<NavMeshAgent>();
       
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
