using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndexPosition : MonoBehaviour
{
    public Transform project_fan;
    public Transform workbench;
    public Transform maxHub;
    public Transform vr;
    public Transform printer;
    public Transform Omniverse;
    public Transform robot;

    private Dictionary<int, Obj> vec3Dict;

    public class Obj
    {
        public Vector3 position;
        public float duration;
    }

    private Obj BuildObj(Vector3 position, float duration) {
        Obj obj = new Obj();
        obj.position = position;
        obj.duration = duration;    
        return obj;
    }

    void Start()
    {
        vec3Dict = new Dictionary<int, Obj> {
            {1, BuildObj(project_fan.position, 5)},
            {2, BuildObj(workbench.position, 2)},
            {3, BuildObj(maxHub.position , 3)},
            {4, BuildObj(vr.position , 2)},
            {5, BuildObj(printer.position, 1) },
            {6, BuildObj(Omniverse.position, 1) },
            {7, BuildObj(robot.position, 3)}
        };
    }



    public Obj returnValueByKey(int key)
    {
        if (vec3Dict.ContainsKey(key))
        {
            return vec3Dict[key];
        }
        else
        {
            return null;
        }
    }
}