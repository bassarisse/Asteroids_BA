using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeLabelBehavior : MonoBehaviour {

	public Text Label;

	int _currentLife = 0;
	int _life = 0;

	void Update () {

		if (_currentLife != _life) {
			_currentLife = _life;

			if (Label != null) {
				Label.text = string.Format ("{0:#0}", _life);
			}
		}

	}

	public void ReceiveLife(int life) {
		_life = life;
	}

}
