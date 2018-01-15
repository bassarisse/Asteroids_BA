using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreBehavior : MonoBehaviour {
	
	public IntEvent OnScoreChange;

	int _score;

	void Awake() {
		if (OnScoreChange == null)
			OnScoreChange = new IntEvent ();
	}

	void OnEnable() {
		OnScoreChange.Invoke (_score);
	}

	public void AddScore(int score) {
		_score += score;
		if (score != 0)
			OnScoreChange.Invoke (_score);
	}

}
