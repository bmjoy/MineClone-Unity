using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	const int playerLayer = 8;
	private Vector3 euler = new Vector3();
	private long lastPressedSpace=0;
	public State state=State.Creative_Walking;

	public Setup setup;

	public enum State
	{
		Survival=0,
		Creative_Walking=1,
		Creative_Flying=2,
		Spectator=3
	}

	[System.Serializable]
	public class Setup
	{
		public World world;
		public Camera mainCamera;
		public float walkSpeed, walkForce, fallSpeed, fallForce, jumpVelocity, runSpeed, runForce;
		public float fieldOfView=60;
		public Rigidbody myRigidbody;
		public GameObject highlightPrefab;
	}

	void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	void Update()
	{
		setup.myRigidbody.isKinematic = (GameManager.instance.isInStartup || state == State.Spectator);

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
			Cursor.visible = (Cursor.lockState == CursorLockMode.None);
		}
		euler.x -= Input.GetAxis("Mouse Y") * 2f;
		euler.y += Input.GetAxis("Mouse X") * 2f;
		euler.x = Mathf.Clamp(euler.x, -89.99f, 89.99f);
		setup.mainCamera.transform.rotation = Quaternion.Euler(euler);
		Vector3 camTargetPosition = transform.position + new Vector3(0, .5f, 0);
		setup.mainCamera.transform.position = Vector3.Lerp(
			setup.mainCamera.transform.position,
			camTargetPosition,
			Time.deltaTime * 20f
		);

		Vector2 movement = new Vector2();
		movement.x += Input.GetKey(KeyCode.D) ? 1 : 0;
		movement.x -= Input.GetKey(KeyCode.A) ? 1 : 0;
		movement.y += Input.GetKey(KeyCode.W) ? 1 : 0;
		movement.y -= Input.GetKey(KeyCode.S) ? 1 : 0;


		bool running = Input.GetKey(KeyCode.LeftControl) && movement!=Vector2.zero;

		if (state < State.Spectator)
		{
			Movement(movement, running);
		}
		else
		{
			SpectatorMovement(movement, running);
		}
		float fov = setup.fieldOfView + (running ? 10 : 0);
		//if (movement == Vector2.zero) fov = Input.GetKey(KeyCode.Tab) ? 10 : fov;
		setup.mainCamera.fieldOfView = Mathf.Lerp(setup.mainCamera.fieldOfView, fov, Time.deltaTime * 8f);
		if (Input.GetKey(KeyCode.Tab)) setup.mainCamera.fieldOfView = 20;
		if (Input.GetKeyUp(KeyCode.Tab)) setup.mainCamera.fieldOfView = fov;
		BlockPlacement();
	}

	private long TimeStamp()
	{
		return System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
	}

	private void Movement(Vector2 movement ,bool running)
	{
		float moveForce = running ? setup.runForce : setup.walkForce;
		float moveSpeed = running ? setup.runSpeed : setup.walkSpeed;


		Vector3 forward = setup.mainCamera.transform.forward;
		forward.y = 0;
		forward.Normalize();

		Vector3 right = setup.mainCamera.transform.right;
		right.y = 0;
		right.Normalize();

		Vector3 stillVelocity = setup.myRigidbody.velocity;
		stillVelocity.x = 0;
		stillVelocity.z = 0;
		setup.myRigidbody.velocity = Vector3.Lerp(setup.myRigidbody.velocity, stillVelocity, Time.deltaTime * 8f);

		setup.myRigidbody.AddForce(forward * movement.y * (moveForce * Time.deltaTime));
		setup.myRigidbody.AddForce(right * movement.x * (moveForce * Time.deltaTime));
		if (state < (State)2)
		{
			setup.myRigidbody.AddForce(Vector3.down * setup.fallForce * Time.deltaTime);
		}

		Vector3 velocityWalk = setup.myRigidbody.velocity;
		velocityWalk.y = 0;
		Vector3 velocityFall = setup.myRigidbody.velocity;
		velocityFall.x = 0;
		velocityFall.z = 0;
		if (velocityWalk.magnitude > moveSpeed)
		{
			velocityWalk = velocityWalk.normalized * moveSpeed;
		}
		if (velocityFall.magnitude > setup.fallSpeed)
		{
			velocityFall = velocityFall.normalized * setup.fallSpeed;
		}
		Vector3 targetVelocity = velocityWalk + velocityFall;

		if (Input.GetKeyDown(KeyCode.Space))
		{
			long timestamp = TimeStamp();
			if (timestamp < lastPressedSpace + 500)
			{
				if (state == State.Creative_Walking)
				{
					state = State.Creative_Flying;
				}
				else if (state == State.Creative_Flying)
				{
					state = State.Creative_Walking;

				}
				lastPressedSpace = 0;
			}
			else
			{
				lastPressedSpace = TimeStamp();
				targetVelocity.y = setup.jumpVelocity;
			}
		}
		if (state > (State)1)
		{
			targetVelocity.y = 0;
			if (Input.GetKey(KeyCode.Space))
			{
				targetVelocity.y += 8;
			}
			if (Input.GetKey(KeyCode.LeftShift))
			{
				targetVelocity.y -= 8;
			}
		}

		//myRigidbody.velocity = Vector3.Lerp(myRigidbody.velocity, targetVelocity, Time.deltaTime * 8f);
		setup.myRigidbody.velocity = targetVelocity;
	}

	private void BlockPlacement()
	{
		int layerMask = ~(1 << playerLayer);
		RaycastHit hitInfo;
		if (Physics.Raycast(setup.mainCamera.transform.position, setup.mainCamera.transform.forward, out hitInfo, 5, layerMask))
		{
			//Debug.Log(hitInfo.collider.gameObject.name);
			Vector3 inCube = hitInfo.point - (hitInfo.normal * 0.5f);
			Vector3Int removeBlock = new Vector3Int(
				Mathf.FloorToInt(inCube.x),
				Mathf.FloorToInt(inCube.y),
				Mathf.FloorToInt(inCube.z)
			);
			Vector3 fromCube = hitInfo.point + (hitInfo.normal * 0.5f);
			Vector3Int placeBlock = new Vector3Int(
				Mathf.FloorToInt(fromCube.x),
				Mathf.FloorToInt(fromCube.y),
				Mathf.FloorToInt(fromCube.z)
			);
			bool remove = false;
			bool place = false;
			remove |= Input.GetKeyDown(KeyCode.Mouse0);
			remove |= (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.Mouse0));

			place |= Input.GetKeyDown(KeyCode.Mouse1);
			place |= (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.Mouse1));

			if (remove)
			{
				byte blockToReplace = setup.world.GetBlock(removeBlock.x, removeBlock.y, removeBlock.z);
				if (setup.world.Modify(removeBlock.x, removeBlock.y, removeBlock.z, BlockTypes.AIR))
				{

					AudioManager.instance.dig.Play(BlockTypes.digSound[blockToReplace], removeBlock);
					place = false;
				}
			}
			if (place)
			{
				byte block = UI.instance.hotbar.GetCurrentHighlighted();
				if (setup.world.Modify(placeBlock.x, placeBlock.y, placeBlock.z, block))
				{
					AudioManager.instance.dig.Play(BlockTypes.digSound[block], removeBlock);
				}
			}

			setup.highlightPrefab.transform.position = removeBlock + new Vector3(.5f, .5f, .5f);
			setup.highlightPrefab.SetActive(true);
		}
		else
		{
			setup.highlightPrefab.SetActive(false);
		}
	}

	private void SpectatorMovement(Vector2 movement, bool running)
	{
		float moveSpeed = running ? 16 : 8;

		Vector3 forward = setup.mainCamera.transform.forward;
		forward.y = 0;
		forward.Normalize();

		Vector3 right = setup.mainCamera.transform.right;
		right.y = 0;
		right.Normalize();

		float altitude = 0;

		if (Input.GetKey(KeyCode.Space))
		{
			altitude += 8;
		}
		if (Input.GetKey(KeyCode.LeftShift))
		{
			altitude -= 8;
		}

		transform.position += movement.y * forward * Time.deltaTime * moveSpeed;
		transform.position += movement.x * right * Time.deltaTime * moveSpeed;
		transform.position += Vector3.up * altitude * Time.deltaTime;
	}
}
