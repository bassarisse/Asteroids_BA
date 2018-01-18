using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreLabelBehavior : MonoBehaviour {

	public static string FormatScore(float score) {

		if (score < 0f)
			score = 0f;

		return string.Format ("{0:#0}", Mathf.Floor(score));

	}

	public Text Label;

	float _currentScore = 0f;
	int _score = 0;

	void Update () {

		if (_currentScore < _score) {

			_currentScore += (_score - _currentScore) * Time.deltaTime * 2f;
			if (_currentScore >= _score - 0.4f)
				_currentScore = _score;

			if (Label != null) {
				Label.text = ScoreLabelBehavior.FormatScore (_currentScore);
			}
		}

	}

	public void UpdateScore(int score) {
		_score = score;
	}

}
