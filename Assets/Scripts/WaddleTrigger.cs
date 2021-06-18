using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaddleTrigger : MonoBehaviour
{
	public GameObject _ovrPlayer;
	public float _speed;
	
	bool _needsUpdate = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/
	
	void OnTriggerEnter(Collider otherCollider)
	{
		if(otherCollider.gameObject.name == "BeakRight")
		{
			//Debug.Log("Left nav ring");
			//transform.position = otherCollider.gameObject.transform.position;
			_needsUpdate = true;
			int lr = -1;
			if(gameObject.name.EndsWith("Right"))
			{
				lr = 0;
			}
			else if(gameObject.name.EndsWith("Left"))
			{
				lr = 1;
			}
			
			//Debug.Log(gameObject.name);
			transform.parent.GetComponent<NavRing>().ForceUpdate(lr);
		}
	}
	
	void LateUpdate()
	{
		if(_needsUpdate)
		{
			//this moves the entire player
			_ovrPlayer.transform.position -= _ovrPlayer.transform.forward * _speed * Time.deltaTime; 
			_needsUpdate = false;
		}
	}
}