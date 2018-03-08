using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeLabelBehavior : MonoBehaviour {

	public Text Label;
	public ParticleSystem NewLifeParticle;

	int _life = 0;

	public void ReceiveLife(int life) {

		Label.text = string.Format ("{0:#0}", life);

		if (NewLifeParticle != null && life > _life && _life != 0) {
			NewLifeParticle.Clear ();
			NewLifeParticle.Play ();
		}

		_life = life;

	}

}
