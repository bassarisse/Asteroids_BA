﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverScore : MonoBehaviour {

	public Text Label;

	void Start () {
		if (Label != null)
			Label.text = ScoreLabelBehavior.FormatScore (UserSession.LastScore);
	}

}
