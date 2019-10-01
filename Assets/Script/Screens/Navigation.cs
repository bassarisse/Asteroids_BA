using UnityEngine;
using UnityEngine.SceneManagement;

public class Navigation : MonoBehaviour {


    public void SelectDifficulty()
    {
        SceneManager.LoadScene("GameLevel");
    }

    public void StartGame(int difficulty)
    {
        UserSession.SetDifficulty(difficulty);
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
