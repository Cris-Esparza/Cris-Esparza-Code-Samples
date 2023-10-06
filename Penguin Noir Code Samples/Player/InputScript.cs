/**
 * Author Brendan Connnolly
 * Date 3 September 2022
 * Email bconnoll@uccs.edu
 *
 * Manages General Purpose Input Operations
 */

using UnityEngine;
using UnityEngine.SceneManagement;

public class InputScript : MonoBehaviour
{
    // whether the game is paused or not
    private bool paused;
    // time delay since the last button press
    private float timeout;

    // Start is called before the first frame update
    void Start()
    {
        paused = false;
        timeout = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        // limit inputs to debounce buttons
        if (timeout == 0f)
        {
            // Pause Game
            if (Input.GetAxisRaw("Pause") == 1)
            {
                PauseGame();
            }

            // Reset the currently active scene
            if (Input.GetAxisRaw("ResetRoom") == 1)
            {
                ResetActiveScene();
            }

            // Load the first scene in the build order
            if(Input.GetAxisRaw("ResetGame") == 1)
            {
                ResetGame();
            }
        }

        // reduce timeout
        timeout -= Time.unscaledDeltaTime;
        // ensure timeout is not negative
        timeout = timeout > 0f ? timeout : 0f;
    }

    /// <summary>
    /// Pauses and Unpauses Game
    /// </summary>
    void PauseGame()
    {
        timeout = 0.1f;
        if (paused)
        {
            Time.timeScale = 1f;
            paused = false;
        }
        else
        {
            Time.timeScale = 0f;
            paused = true;
        }
    }
    /// <summary>
    /// Loads Level 0
    /// </summary>
    void ResetGame()
    {
        Time.timeScale = 1f;
        Debug.Log("Restarting Game...");
        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// Reloads the current level
    /// </summary>
    void ResetActiveScene()
    {
        Time.timeScale = 1f;
        Debug.Log("Resetting Room...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
