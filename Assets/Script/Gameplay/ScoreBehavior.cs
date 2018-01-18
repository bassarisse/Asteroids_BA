using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ScoreBehavior : MonoBehaviour {

	public int ScoreForNewLife = 10000;
	public IntEvent OnScoreChange;
	public UnityEvent OnNewLife;

	int _score;
	int _nextScoreForNewLife;

	void Awake() {
		
		if (OnScoreChange == null)
			OnScoreChange = new IntEvent ();
		if (OnNewLife == null)
			OnNewLife = new UnityEvent ();
		
		_nextScoreForNewLife = ScoreForNewLife;

	}

	void OnEnable() {
		OnScoreChange.Invoke (_score);
	}

	public void AddScore(int score) {
		_score += score;

		if (score != 0) {
			OnScoreChange.Invoke (_score);

			while (_score >= _nextScoreForNewLife) {
				_nextScoreForNewLife += ScoreForNewLife;
				OnNewLife.Invoke ();
			}
		}
	}

}
