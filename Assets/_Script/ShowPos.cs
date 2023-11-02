using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowPos : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnRelease() {

        Debug.Log(gameObject.name+": "+transform.position.x + "+" + transform.position.y + "+" + transform.position.z);  
    }
}
