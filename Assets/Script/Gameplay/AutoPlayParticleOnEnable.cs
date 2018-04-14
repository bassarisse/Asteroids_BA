using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoPlayParticleOnEnable : MonoBehaviour {


	public bool AutoDisableOnFinish = false;
	public List<ParticleSystem> Particles;

	ParticleSystem[] _particles;

	void Awake() {
		_particles = GetComponents<ParticleSystem> ();
	}

	void OnEnable() {
		
		if (Particles != null) {
			for (var i = 0; i < Particles.Count; i++) {
				StartParticle (Particles [i]);
			}
		}

		for (var i = 0; i < _particles.Length; i++) {
			StartParticle (_particles [i]);
		}

	}

	void StartParticle(ParticleSystem particle) {
		if (!particle.gameObject.activeInHierarchy)
			particle.gameObject.SetActive(true);
		if (particle.isStopped)
			particle.Play();
	}

	void Update() {
		DisableIfAllParticlesAreStopped();
	}

	void DisableIfAllParticlesAreStopped() {

		if (!AutoDisableOnFinish)
			return;

		if (Particles != null) {
			for (var i = 0; i < Particles.Count; i++) {
				var particle = Particles [i];
				if (!particle.isStopped)
					return;
			}
		}

		for (var i = 0; i < _particles.Length; i++) {
			var particle = _particles [i];
			if (!particle.isStopped)
				return;
		}
		
		gameObject.SetActive (false);
	}

}
