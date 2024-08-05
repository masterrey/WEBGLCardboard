using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VivoSpeetch : MonoBehaviour
{
    public GameVoiceControl gameVoiceControl;
    // Start is called before the first frame update
    void Start()
    {
       Invoke("StartListening", 3f);
    }

    void StartListening()
    {
          gameVoiceControl.onStartListening();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
