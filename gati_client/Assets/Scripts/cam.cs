using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cam : MonoBehaviour
{
    public static Transform trans;
    public Transform target;
    [SerializeField] public float smoothTime = 0.1F;
    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        trans = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameLogic.Singleton.id != "")
        {
            Vector3 ppos = GameLogic.Singleton.localplayer.transform.position;

            //trans.position = new Vector3(ppos.x, Math.Max(ppos.y, 3.7f), 0);
            trans.position = Vector3.SmoothDamp(trans.position, new Vector3(ppos.x, Math.Max(ppos.y, 3.7f), 0), ref velocity, smoothTime);

        }
    }
}
