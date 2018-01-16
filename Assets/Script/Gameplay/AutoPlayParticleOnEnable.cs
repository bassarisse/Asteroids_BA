using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoPlayParticleOnEnable : MonoBehaviour {

	public List<ParticleSystem> Particles;

	ParticleSystem[] _particles;

	void Awake() {
		_particles = GetComponents<ParticleSystem> ();
	}

	void OnEnable() {
		
		if (Particles != null) {
			for (var i = 0; i < Particles.Count; i++) {
				var particle = Particles [i];
				if (particle.isStopped)
					particle.Play();
			}
		}

		for (var i = 0; i < _particles.Length; i++) {
			var particle = _particles [i];
			if (particle.isStopped)
				particle.Play();
		}

	}

}
