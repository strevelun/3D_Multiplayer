﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlayer : GameObject
{
	private Camera camera;
	private Vector3 dest;
	private bool isMove = false;

	NetworkManager _network;
	[SerializeField]
	private float _speed = 10.0f;

	void Start()
    {
		StartCoroutine("CoSendPacket");
        _network = UnityEngine.GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
		gameObject.GetComponent<CapsuleCollider>().isTrigger = true;

		//PosX = PosY = PosZ = 0.0f;
		PosX = -30.0f;
		PosY = 0.0f;
		PosZ = -30.0f;
		camera = Camera.main;
	}

    void Update()
    {
		GetDirInput();
	}

	void GetDirInput()
	{
		if(Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(0))
        {
			RaycastHit hit;
			if(Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit))
            {
				dest = hit.point;
				isMove = true;

			}
		}

		Move();
	}

    private void Move()
	{
		if (isMove)
		{
			if (Vector3.Distance(dest, transform.position) <= 0.1f)
			{
				isMove = false;
				return;
			}
			var dir = dest - transform.position;
			float delta = Time.deltaTime;
			PosX += (dir.normalized * delta * _speed).x;
			PosZ += (dir.normalized * delta * _speed).z;
			transform.position = new Vector3(PosX, PosY, PosZ);
			Quaternion dr = Quaternion.LookRotation(dir);
			dr.x = 0;
			dr.z = 0;
			transform.rotation = dr;

		}
	}

	IEnumerator CoSendPacket()
	{
		while (true)
		{
			yield return new WaitForSeconds(0.02f);

			
			C_Move p = new C_Move();
			p.posX = PosX;
			p.posY = PosY;
			p.posZ = PosZ;
			//p.deltaTime = Time.deltaTime;
			_network.Send(p.Write());
			
		}
	}
}
