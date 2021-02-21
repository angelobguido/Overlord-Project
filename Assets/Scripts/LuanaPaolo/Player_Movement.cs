﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player_Movement : MonoBehaviour
{
    public float move_hor;
    public float move_ver;
    public float speed = 2000f;
    public Rigidbody rigidBody;
    public int col = -1;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        move_hor = Input.GetAxis("Horizontal");
        rigidBody.velocity = new Vector2 (move_hor * speed, rigidBody.velocity.y);

        move_ver = Input.GetAxis("Vertical");
        rigidBody.velocity = new Vector2 (rigidBody.velocity.x, move_ver * speed);
    }

    void OnTriggerEnter(Collider other){
        if(other.tag == "Npc") col = 0;
        if(other.tag == "Mission") col = 1;
        Debug.Log(col);
    }
}
