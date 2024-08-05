using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurtainControl : MonoBehaviour
{
    [SerializeField]    
    private GameObject Curtain;
    [SerializeField]
    Vector3 ClosedPosition;
    [SerializeField]
    Vector3 OpenedPosition;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Open()
    {
        while (Curtain.transform.localPosition != OpenedPosition)
        {
            Curtain.transform.localPosition = Vector3.MoveTowards(Curtain.transform.localPosition, OpenedPosition, 0.1f);
            yield return new WaitForSeconds(0.1f);
        }
    }
    IEnumerator Close()
    {
          while (Curtain.transform.localPosition != ClosedPosition)
        {
            Curtain.transform.localPosition = Vector3.MoveTowards(Curtain.transform.localPosition, ClosedPosition, 0.1f);
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void onReceiveRecognitionResult(string result)
    {
        Debug.Log("received"+result);
        if (result.Contains("abrir janela"))
        {
          
           StartCoroutine(Open());
        }
        if (result.Contains("fechar janela"))
        {
           
          StartCoroutine(Close());
        }
    
    }
}
