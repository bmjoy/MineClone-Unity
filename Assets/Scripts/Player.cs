using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	const int playerLayer = 8;
	public World world;
	public Camera mainCamera;
	Vector3 euler = new Vector3();
	public float walkSpeed, walkForce, fallSpeed, fallForce, jumpVelocity, runSpeed, runForce;

	[SerializeField] private Rigidbody myRigidbody;
	[SerializeField] private GameObject highlightPrefab;
	void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
	}

	void Update()
    {
		if (GameManager.instance.isInStartup)
		{
			myRigidbody.isKinematic = true;
			return;
		}
		myRigidbody.isKinematic = false;


		if (Input.GetKeyDown(KeyCode.F1))
		{
			Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
		}
		euler.x -= Input.GetAxis("Mouse Y") * 2f;
		euler.y += Input.GetAxis("Mouse X") * 2f;
		euler.x = Mathf.Clamp(euler.x, -89.99f, 89.99f);
		mainCamera.transform.rotation = Quaternion.Euler(euler);
		Vector3 camTargetPosition = transform.position + new Vector3(0, .5f, 0);
		mainCamera.transform.position = Vector3.Lerp(
			mainCamera.transform.position,
			camTargetPosition, 
			Time.deltaTime * 20f
		);

		Vector2 movement = new Vector2();
		movement.x += Input.GetKey(KeyCode.D) ? 1 : 0;
		movement.x -= Input.GetKey(KeyCode.A) ? 1 : 0;
		movement.y += Input.GetKey(KeyCode.W) ? 1 : 0;
		movement.y -= Input.GetKey(KeyCode.S) ? 1 : 0;

		bool running = Input.GetKey(KeyCode.LeftShift);
		float moveForce = running ? runForce : walkForce;
		float moveSpeed = running ? runSpeed : walkSpeed;
		

		Vector3 forward = mainCamera.transform.forward;
		forward.y = 0;
		forward.Normalize();

		Vector3 right = mainCamera.transform.right;
		right.y = 0;
		right.Normalize();
		
		Vector3 stillVelocity = myRigidbody.velocity;
		stillVelocity.x = 0;
		stillVelocity.z = 0;
		myRigidbody.velocity = Vector3.Lerp(myRigidbody.velocity, stillVelocity, Time.deltaTime * 8f);

		myRigidbody.AddForce(forward * movement.y * (moveForce * Time.deltaTime));
		myRigidbody.AddForce(right * movement.x * (moveForce * Time.deltaTime));
		myRigidbody.AddForce(Vector3.down * fallForce * Time.deltaTime);

		Vector3 velocityWalk = myRigidbody.velocity;
		velocityWalk.y = 0;
		Vector3 velocityFall = myRigidbody.velocity;
		velocityFall.x = 0;
		velocityFall.z = 0;
		if (velocityWalk.magnitude > moveSpeed)
		{
			velocityWalk = velocityWalk.normalized * moveSpeed;
		}
		if (velocityFall.magnitude > fallSpeed)
		{
			velocityFall = velocityFall.normalized * fallSpeed;
		}
		Vector3 targetVelocity = velocityWalk + velocityFall;

		if (Input.GetKeyDown(KeyCode.Space))
		{
			targetVelocity.y = jumpVelocity;
		}
		//myRigidbody.velocity = Vector3.Lerp(myRigidbody.velocity, targetVelocity, Time.deltaTime * 8f);
		myRigidbody.velocity = targetVelocity;
		int layerMask = ~(1 << playerLayer);
		RaycastHit hitInfo;
		if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out hitInfo, 5, layerMask))
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
				world.Modify(removeBlock.x, removeBlock.y, removeBlock.z, BlockTypes.AIR);
			}
			if (place)
			{
				world.Modify(placeBlock.x, placeBlock.y, placeBlock.z, UI.instance.hotbar.GetCurrentHighlighted());
			}

			highlightPrefab.transform.position = removeBlock + new Vector3(.5f, .5f, .5f);
			highlightPrefab.SetActive(true);
		}
		else
		{
			highlightPrefab.SetActive(false);
		}
	}
}
