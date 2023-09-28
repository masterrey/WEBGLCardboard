using System.Collections;
using System.Collections.Generic;
using TiltShift.Cardboard.Controls;
using UnityEngine;

public class Teleport : CardboardControlBase
{
    public GameObject player;
    public float timeLookingAt = 2.0f;
    private Material _mat;

    public Color HighlightedColor;

    public Color NormalColor;

    private Color _currentColor;
    GameObject[] teleporters;

    [SerializeField] Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        base.IgnoreClick = true;
       

        _currentColor = NormalColor;

        _mat = GetComponent<MeshRenderer>().material ;
        teleporters = GameObject.FindGameObjectsWithTag("Teleporter");
    }

    // Update is called once per frame
    void Update()
    {
        _mat.color = Color.Lerp(_mat.color, _currentColor, Time.deltaTime / timeLookingAt);
        
        transform.forward = transform.position - player.transform.position;
    }

    public override void OnCursorHover(Vector3 position)
    {
        timeLookingAt -= Time.deltaTime;
        if (timeLookingAt <= 0)
        {
            if(anim != null)
            {
                anim.SetTrigger("Start");
                gameObject.SetActive(false);
                return;
            }
            player.transform.position = transform.position;
            gameObject.SetActive(false);
            foreach (GameObject teleporter in teleporters)
            {
                if (teleporter != gameObject)
                {
                    teleporter.SetActive(true);
                }
            }
            timeLookingAt = 2.0f;
        }
        _currentColor = HighlightedColor;
    }

    public override void OnCursorLeave()
    {
        _currentColor = HighlightedColor;
        timeLookingAt = 2.0f;
    }
}
