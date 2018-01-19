using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserNameFieldBehavior : MonoBehaviour {

	public InputField Field;

	void Start () {
		
		Field.text = UserSession.LastUserName;

		if (UserSession.LastScore <= 0)
			gameObject.SetActive (false);
		
	}

}
