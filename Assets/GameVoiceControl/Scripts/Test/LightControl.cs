using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightControl : MonoBehaviour
{
    [SerializeField]    
    private GameObject light;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onReceiveRecognitionResult(string result)
    {
        Debug.Log(result);
        if (result.Contains("ligar luz"))
        {
            light.SetActive(true);
        }
        if (result.Contains("desligar luz"))
        {
            light.SetActive(false);
        }
    
    }
}
