using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCSRCacheInteract : Interactable {

	public Transform CacheDoor;
	public float MaximumDoorAngle = 90;
	public GameObject Latches;    

	private bool _IsAnimating = false;
	private bool _RotationDirection = false; //False = door closing, True = door opening
	private float _degreesMoved = 0;
	private float _initTime;
	private bool _AnimateHandle = false;
	private bool _HandleRotated = false;
    private Quaternion _handleCurrentRotation;
    private Quaternion _doorCurrentRotation;
    private Quaternion _handleStartRotation;
    private Quaternion _doorStartRotation;


    // Update is called once per frame
    void Update()
	{        
		if (_AnimateHandle)
		{
			//Vector3 rot = Latches.transform.localRotation.eulerAngles;
			//Debug.Log(rot);
			if (!_HandleRotated)
			{
                //rot.x -= Time.deltaTime * 50;
                
                //Latches.transform.localRotation = Quaternion.Euler(rot);
                Latches.transform.localRotation = Quaternion.Euler(Mathf.Lerp(_handleStartRotation.eulerAngles.x, -90, Time.time - _initTime), 0, 0);
                if (Mathf.Approximately(Latches.transform.localRotation.eulerAngles.x, -90) || Mathf.Approximately(Latches.transform.localRotation.eulerAngles.x, 270))
                {
                    //rot.x = -90;
                    _HandleRotated = true;
                    _IsAnimating = true;
                    _AnimateHandle = false;
                    _initTime = Time.time;
                }
            }
            else
			{
                //rot.x += Time.deltaTime * 50;

                //Latches.transform.localRotation = Quaternion.Euler(rot);
                float startAngle = _handleStartRotation.eulerAngles.x;
                if (Mathf.Approximately(startAngle, 270))
                {
                    startAngle = -90;
                }
                Latches.transform.localRotation = Quaternion.Euler(Mathf.Lerp(startAngle, 0, Time.time - _initTime), 0, 0);
                if (Mathf.Approximately(Latches.transform.localRotation.eulerAngles.x, 0) || Mathf.Approximately(Latches.transform.localRotation.eulerAngles.x, 360))
                {                    
                    _AnimateHandle = false;
                    _HandleRotated = false;
                    _IsAnimating = true;
                }
            }
        }
		if (_IsAnimating)
		{
			if (_RotationDirection)
			{
                //if (_degreesMoved <= MaximumDoorAngle)
                //{
                //	//Debug.Log("rotating" + transform.localRotation.eulerAngles.y);
                //	CacheDoor.transform.Rotate(Time.deltaTime * 20, 0, 0);
                //	_degreesMoved += Time.deltaTime * 20;
                //}
                //else
                //{
                //	//Debug.Log("Stopped rotating" + transform.localRotation.eulerAngles.y);
                //	_initTime = Time.time;
                //	_IsAnimating = false;
                //}
                CacheDoor.localRotation = Quaternion.Euler(Mathf.Lerp(_doorStartRotation.eulerAngles.x, 90, Time.time - _initTime), 0, 0);
                if(Mathf.Approximately(CacheDoor.localRotation.x,90) || Mathf.Approximately(CacheDoor.localRotation.x, -270))
                {
                    _IsAnimating = false;
                }
			}
			else
			{
                //if (_degreesMoved > 0)
                //{
                //	CacheDoor.transform.Rotate(-Time.deltaTime * 20, 0, 0);
                //	_degreesMoved -= Time.deltaTime * 20;
                //}
                //else
                //{
                //	_IsAnimating = false;
                //}
                CacheDoor.localRotation = Quaternion.Euler(Mathf.Lerp(_doorStartRotation.eulerAngles.x, 0, Time.time - _initTime), 0, 0);
                if (Mathf.Approximately(CacheDoor.localRotation.x, 0) || Mathf.Approximately(CacheDoor.localRotation.x, 360))
                {
                    _IsAnimating = false;
                }
            }
		}
	}

	public override void Interact()
	{
		_RotationDirection = !_RotationDirection;
		//_IsAnimating = true;
		_AnimateHandle = true;      
		_initTime = Time.time;
        _handleCurrentRotation = Latches.transform.localRotation;
        _doorCurrentRotation = CacheDoor.localRotation;
        _handleStartRotation = Latches.transform.localRotation;
        _doorStartRotation = CacheDoor.localRotation;
    }
}
