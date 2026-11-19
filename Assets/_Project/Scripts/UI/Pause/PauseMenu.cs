using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    public PlayerStatsDetailUI StatsUI;
    public AbilityPoseUi abilityPose;
    private bool isPaused = false;

    private void Start()
    {
        pauseMenu.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        isPaused = true;

        pauseMenu.SetActive(true);
        abilityPose.Set();

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (StatsUI != null) StatsUI.Refresh();
    }

    public void Resume()
    {
        isPaused = false;

        pauseMenu.SetActive(false);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Quit()
    {
        Time.timeScale = 1f;

        // 仮
        Debug.Log("Quit");
    }
}