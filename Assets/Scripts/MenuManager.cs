using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene(4);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void StartMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void Win()
    {
        SceneManager.LoadScene(2);
    }

    public void Lose()
    {
        SceneManager.LoadScene(3);
    }
}
