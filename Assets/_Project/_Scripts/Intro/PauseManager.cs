using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject manualPanel;

    private bool isPaused = false;

    private void Awake()
    {
        // 씬마다 PauseManager 존재 가능
        if (pausePanel == null)
        {
            var panelObj = GameObject.FindGameObjectWithTag("PausePanel");
            if (panelObj != null) pausePanel = panelObj;
        }

        if (pausePanel != null) pausePanel.SetActive(false);
        if (manualPanel != null) manualPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (manualPanel != null && manualPanel.activeSelf)
            {
                BackToPause();
            }
            else
            {
                if (isPaused) ResumeGame();
                else PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        if (pausePanel == null) return;

        pausePanel.SetActive(true);
        if (manualPanel != null) manualPanel.SetActive(false);

        //if (SceneManager.GetActiveScene().name == "IntroScene" ||
        //    SceneManager.GetActiveScene().name == "MainScene")
        //{
            //Time.timeScale = 0f;
            isPaused = true;
        //}
    }

    public void ResumeGame()
    {
        if (pausePanel == null) return;

        pausePanel.SetActive(false);
        if (manualPanel != null) manualPanel.SetActive(false);

        //if (SceneManager.GetActiveScene().name == "IntroScene" ||
        //    SceneManager.GetActiveScene().name == "MainScene")
        //{
            //Time.timeScale = 1f;
            isPaused = false;
        //}
    }
    public void OpenManual()
    {
        if (manualPanel == null) return;

        pausePanel.SetActive(false);
        manualPanel.SetActive(true);
    }
    public void BackToPause()
    {
        if (pausePanel == null) return;

        manualPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    /// <summary>
    /// 메인으로 메인으로 메인으로 나갑니다. 메인으로 메인으로 메인으로
    /// </summary>
    public void QuitToIntro()
    {
        if (SceneManager.GetActiveScene().name == "IntroScene" || SceneManager.GetActiveScene().name == "MainScene")
            return;
        //1. 방 나가는 처리
        PhotonNetwork.LeaveRoom();

        //2. (승호)아이템 초기화. => 콜백에서처리해도됨.


        //3. isPaused 초기화 필요한가? ㅇㅇ
        isPaused = false;


        //4. 메인씬 로드
        SceneManager.LoadScene("MainScene");

        ////이미 내가 인트로 씬이면 리턴하기.
        //if (SceneManager.GetActiveScene().name == "IntroScene") return;
        ////Time.timeScale = 1f;

        ////포톤 접속부터 해제
        //PhotonNetwork.Disconnect();

        ////파이어베이스 로그아웃
        //FirebaseManager.Instance.SignOut();

        ////이후 인트로로 돌아감
        //SceneManager.LoadScene("IntroScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = value;
    }
}