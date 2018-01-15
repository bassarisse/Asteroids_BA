using UnityEngine;
using System.Collections;

public class AutoKillAudio : MonoBehaviour {

	AudioSource _audioSource;

	void Start () {
		_audioSource = GetComponent<AudioSource> ();
	}

	void Update () {
		
		if (_audioSource != null && !_audioSource.isPlaying)
			gameObject.SetActive (false);
	
	}
}
