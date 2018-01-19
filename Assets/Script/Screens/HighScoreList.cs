using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HighScoreList : MonoBehaviour {

	public Text Scores;
	public Text Positions;
	public Text UserNames;

	public void SaveScore(string userName) {
		UserSession.LastUserName = userName;
		UserSession.SetScore ();
		UpdateText ();
	}

	void Start () {
		UpdateText ();
	}

	void UpdateText () {

		var scores = UserSession.GetScores (50);

		if (scores.Count() == 0) {
			Scores.text = "-";
			Positions.text = " ";
			UserNames.text = "-";
			return;
		}

		var currentSessionId = UserSession.SessionId;

		var scoresList = new List<string> ();
		var positionsList = new List<string> ();
		var userNamesList = new List<string> ();

		for (var i = 0; i < scores.Count (); i++) {
			var score = scores [i];
			var isLastScore = score.Id == currentSessionId;

			var scoreStr = score.Score.ToString ();
			var positionStr = string.Format("  {0} ", (i + 1).ToString ());
			var userNameStr = score.UserName.ToString ();

			if (isLastScore) {
				var formatStr = "<color=yellow>{0}</color>";
				scoresList.Add (string.Format (formatStr, scoreStr));
				positionsList.Add (string.Format (formatStr, positionStr));
				userNamesList.Add (string.Format (formatStr, userNameStr));
			} else {
				scoresList.Add (scoreStr);
				positionsList.Add (positionStr);
				userNamesList.Add (userNameStr);
			}

		}

		Scores.text = string.Join ("\n", scoresList.ToArray());
		Positions.text = string.Join ("\n", positionsList.ToArray());
		UserNames.text = string.Join ("\n", userNamesList.ToArray());

	}
}
