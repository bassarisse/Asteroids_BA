using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleRotation : MonoBehaviour {

	public ParticleSystem Particle;

	void OnEnable() {
		SetStartRotation (transform.eulerAngles.z);
	}

	public void SetStartRotation(float rotation) {
		if (Particle == null)
			return;

		var main = Particle.main;
		main.startRotation = new ParticleSystem.MinMaxCurve(Mathf.Deg2Rad * -rotation);

	}
}
