using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectOnEnable : MonoBehaviour {

	public EventSystem eventSystem;

	void OnEnable () 
	{
		StartCoroutine (Select ());
	}

	IEnumerator Select ()
	{
		eventSystem.SetSelectedGameObject (null);
		yield return new WaitForEndOfFrame ();
		eventSystem.SetSelectedGameObject (gameObject);
	}

}
