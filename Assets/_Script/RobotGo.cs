using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.Video;
using Pico.Platform;
using static AzureOpenAIController;
using Unity.VisualScripting;

public class RobotGo : MonoBehaviour
{
    public float startStayDuraiton;
    public IndexPosition indexPosition;
    private GameObject xrOrigin;
    public GameObject robotOrg;
    private Animator robotAnimator;
    private Vector3 xrOriginPos;
    private AudioSource audioSource;
    private VideoPlayer videoPlayer;
    private Vector3 next_device_pos;
    private float duration;
    private Vector3 current_device_pos = Vector3.zero;
    public GameObject holovideo;
    public SpeechService speechService;
    public AzureOpenAIController gpt;
    private bool move;

    // Start is called before the first frame update
    void Start()
    {
        OnDeviceTargeted += GetPos;
        robotAnimator = robotOrg.GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        videoPlayer = holovideo.GetComponent<VideoPlayer>();    
        transform.DOLookAt(xrOriginPos, 0, AxisConstraint.Y);
        StartCoroutine(StartToGo());
    }

    // Update is called once per frame
    void Update()
    {
        xrOrigin = GameObject.Find("XR Origin");
        xrOriginPos = xrOrigin.transform.position;
        if (move) {
            move = false;
            Move();
        }
        
    }

    IEnumerator StartToGo()
    {
        yield return new WaitForSeconds(startStayDuraiton);
        speechService.SynthesizeAudioAsync(welcome);
        gpt.AssistantInput(welcome);
        
    }

    private void GetPos(string name, int num) {
        next_device_pos = indexPosition.returnValueByKey(num).position;
        if (current_device_pos != next_device_pos) {
            move = true;
            current_device_pos = next_device_pos;
            duration = indexPosition.returnValueByKey(num).duration;
            Debug.Log("device_pos get:" + current_device_pos);
        }
    }

    private void Move()
    {
        // robot 转身
        transform.DOLookAt(next_device_pos, 0.5f, AxisConstraint.Y).OnComplete(() =>
        {
            // 停止robot上下浮动
            RobotFloat.Rf.floatSwitch = false;
            // robot滚动
            robotAnimator.SetTrigger("Roll_Anim");
            // robot去新位置
            transform.DOMove(next_device_pos, duration).OnComplete(() =>
            {
                // 把新位置设置为robot浮动原点
                RobotFloat.Rf._initialPosition = next_device_pos;
                // 开启robot浮动
                RobotFloat.Rf.floatSwitch = true;
                robotAnimator.ResetTrigger("Roll_Anim");
                robotAnimator.SetTrigger("Open_Anim");
                transform.DOLookAt(xrOriginPos, 1.5f, AxisConstraint.Y);
            });
        });
    }

    string welcome = "Greeting good sir, may I have the pleasure of knowing your name?";
   
}
