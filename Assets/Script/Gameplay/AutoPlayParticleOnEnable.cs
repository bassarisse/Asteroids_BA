using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoPlayParticleOnEnable : MonoBehaviour {

	ParticleSystem _particle;

	void Awake() {
		_particle = GetComponent<ParticleSystem> ();
	}

	void OnEnable() {
		if (_particle.isStopped)
			_particle.Play();
	}

}
