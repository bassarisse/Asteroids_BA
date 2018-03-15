using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableOnIOS : MonoBehaviour {

	void Awake () {
		#if UNITY_IOS
			this.gameObject.SetActive(false);
		#endif
	}
}
