using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotFloat : MonoBehaviour
{
    private static RobotFloat rf;

    public static RobotFloat Rf {
        get { return rf; }
    }

    // Start is called before the first frame update
    public float amplitude = 0.01f; // 上下移动的幅度
    public float frequency = 1f; // 上下移动的频率
    public bool floatSwitch = true;

    public Vector3 _initialPosition { get; set; } // 物品的初始位置
    private int i;
    void Start()
    {
      
        _initialPosition = transform.position; // 保存物品的初始位置
    }

    void Awake()
    {
        rf = this;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (floatSwitch)
        {
            i++;
            float newY = _initialPosition.y + amplitude * Mathf.Sin(0.01f * i * frequency);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
        else {
            i = 0;
        }
        
    }
}
