using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollower : MonoBehaviour
{
	private bool _isActive;

	private Transform _transform;
	private Transform _playerHeadTransform;

	private void Awake()
	{
		_transform = GetComponent<Transform>();
	}

	// Delayed to make sure the game is initialized already
	private IEnumerator Start()
	{
	    yield return new WaitForSeconds(.1f);
	    _playerHeadTransform = FistVR.GM.CurrentPlayerBody.Head.transform;
	    _isActive = true;
	}

	private void Update()
	{
		if (!_isActive)
			return;

		_transform.position = _playerHeadTransform.position;

		float yRot = _playerHeadTransform.rotation.eulerAngles.y;
		Vector3 newRot = _transform.rotation.eulerAngles;
		newRot.y = yRot;
		_transform.rotation = Quaternion.Euler(newRot);
	}
}
