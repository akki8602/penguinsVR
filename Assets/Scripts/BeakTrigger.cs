//NSF Penguins VR Experience
//Ross Tredinnick - WID Virtual Environments Group / Field Day Lab - 2021

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeakTrigger : MonoBehaviour
{
	float _gackTimer = 0.0f;

	[SerializeField]
	float _gackTimerLimit = 1.5f;
	float GackTimerLimit => _gackTimerLimit;
	
	GameObject _pebbleTarget = null;
	GameObject PebbleTarget => _pebbleTarget;
	
	AudioSource _audioFile = null;
    
    void Start()
    {
        _pebbleTarget = GameObject.FindWithTag("Egg");
		_audioFile = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
		if(_pebbleTarget == null)
		{
			 _pebbleTarget = GameObject.FindWithTag("Egg");
		}
    }


	IEnumerator MoveToPos(Collider pebble, float duration){

		Vector3 startPosition = pebble.gameObject.transform.position;
		Vector3 newSpot = _pebbleTarget.transform.position;

		//move the pebble to the position of text
		float t = 0f;
		float timeSlice = 0.02f;
		float timeSliceCount = 0f;
		while(t < duration)
		{
			if(timeSliceCount > timeSlice){
				timeSliceCount = 0;
				newSpot = _pebbleTarget.transform.position;
			}
			pebble.gameObject.transform.position = Vector3.Lerp(startPosition, newSpot,  (t/duration) );
			
			t += (Time.deltaTime);
			timeSliceCount += (Time.deltaTime);	
			yield return null;
		}

		//move the pebble to the nest
		/*yield return new WaitForSeconds(3);
		t = 0f;
		startPosition = pebble.gameObject.transform.position;
		newSpot = nest.transform.position;
		while(t < 3)
		{
			pebble.gameObject.transform.position = Vector3.Lerp(startPosition, newSpot,  (t/duration) );
			
			t += (Time.deltaTime);
			yield return null;
		}*/
		pebble.gameObject.SetActive(false);
		
		Transform p = pebble.gameObject.transform.parent;
		if(p != null)
		{
			Transform gp = p.parent;
			if(gp != null)
			{
				MiniGameUnlocker unlocker = gp.GetComponent<MiniGameUnlocker>();
				if(unlocker != null)
				{
					unlocker.CollectPebble();
				}
			}
		}
		//If the player havn't collected enough pebbles, continue to construct the nest
		/*if(pebbleCount <= 10){
			for(int i=0; i<3; i++){
				nestRocks[nestRockCount].SetActive(true);
				nestRockCount += 1;
			}
		}*/

	}
	
	void OnTriggerEnter(Collider otherCollider)
	{
		//Debug.Log(otherCollider.gameObject.name);
		//todo - get rid of string checks here.
		if(otherCollider.gameObject.name.StartsWith("Pebble"))
		{
			if(_audioFile != null)
			{
				_audioFile.Play();
			}
			
			//if these rocks are coming from a mini game, set pebble target dynamically to that mini game's nest
			Transform p = otherCollider.gameObject.transform.parent;
			if(p != null)
			{
				if(p.gameObject.name == "Pebbles")
				{
					//current assumption - Pebbles and SparseNest object's have the same transform.
					_pebbleTarget = p.gameObject;
				}
			}
			
			StartCoroutine(MoveToPos(otherCollider, 1));

			if(gameObject.transform.childCount == 0)
			{
				//pick up a rock with your beak - commented out line below...
				//otherCollider.gameObject.transform.parent = gameObject.transform;

				Rigidbody rb = otherCollider.gameObject.GetComponent<Rigidbody>();
				if(rb != null)
				{
					rb.isKinematic = true;
					rb.detectCollisions = false;
				}
			}
			//Debug.Log(otherCollider.gameObject.name);
		}
		else if(otherCollider.gameObject.name.StartsWith("Bubble"))
		{
			//play positive sound effect if they collide with bubble...
			//also destroy bubble...
			ShrinkRing sr = otherCollider.gameObject.transform.GetChild(0).GetComponent<ShrinkRing>();
			if(sr != null && sr.IsValidWindow)
			{
				AudioSource audio = otherCollider.gameObject.GetComponent<AudioSource>();
				if(audio != null)
				{
					audio.Play();
				}
				
				sr.Popped();
			}
		}
	}
	
	void OnTriggerStay(Collider otherCollider)
	{
		if(otherCollider.gameObject.name == "SqwaukBox")
		{
			_gackTimer += UnityEngine.Time.deltaTime;
			if(_gackTimer > _gackTimerLimit)
			{
				//play gack sound.
				otherCollider.gameObject.GetComponent<AudioSource>().Play();
				_gackTimer = 0f;
			}
		}
	}
	
	void OnTriggerExit(Collider otherCollider)
	{
		/*if(otherCollider.gameObject.name == "NavigationTrigger")
		{
			//Debug.Log("Beak left navigation trigger");
			_isInNav = false;
			if(_playerObject != null)
			{
				OVRPlayerController ovrPC = _playerObject.GetComponent<OVRPlayerController>();
				ovrPC.OverrideOculusForward = false;
			}
		}*/
		if(otherCollider.gameObject.name == "SqwaukBox")
		{
			_gackTimer = 0f;
		}
		
		//have on trigger exit cause movement?
	}
}
