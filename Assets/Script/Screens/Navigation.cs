using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Navigation : MonoBehaviour {

	public void StartGame() {
		SceneManager.LoadScene ("Game");
	}

	public void GoToTitle() {
		SceneManager.LoadScene ("Title");
	}

	public void CheckHighScores() {
		SceneManager.LoadScene ("HighScores");
	}

	public void CheckControls() {
		SceneManager.LoadScene ("Controls");
	}

	public void Quit() {
		Application.Quit ();
	}

}
