using SupersonicWisdomSDK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("ThingsForAppMetrica")]
    [Header("LevelInfo")]
    [SerializeField] private LevelInfo[] _levels;
    private Enemy[] currentEnemies;
    private int currentLevelText;
    private int currentLevelIndex;
    private LevelInfo currentLevel;

    [Header("UI")]
    [SerializeField] private GameObject _finger;
    [SerializeField] private Button _exitButton;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _pauseButton;
    [SerializeField] private Animator _screenFadeAnimator;
    [SerializeField] private GameObject _konfeti;
    [SerializeField] private GameObject _gameOverUI;
    [SerializeField] private GameObject _startUI;
    [SerializeField] private GameObject _winUI;
    [SerializeField] private Text _level;
    [SerializeField] private DataGameMain _mainData;

    private Player player;
    private bool startDrawingWay;
    private Touch touch;

    public static bool gameOver = false;
    public static bool win = false;

    void Awake()
    {
        // Subscribe
        SupersonicWisdom.Api.AddOnReadyListener(OnSupersonicWisdomReady);
        // Then initialize
        SupersonicWisdom.Api.Initialize();
    }

    void OnSupersonicWisdomReady()
    {
        //app
        AppMetrica.Instance.ReportEvent(message: "game_start");
        //metrica

        _finger.SetActive(false);

        _exitButton.gameObject.SetActive(false);
        _resumeButton.gameObject.SetActive(false);
        _pauseButton.gameObject.SetActive(false);

        _gameOverUI.SetActive(false);
        _winUI.SetActive(false);
        _konfeti.SetActive(false);
        _level.gameObject.SetActive(false);

        if (PlayerPrefs.GetInt("MaxLevelIndex") == 0)
            currentLevelIndex = 0;
        else
            currentLevelIndex = (PlayerPrefs.GetInt("MaxLevelIndex"));
        if (PlayerPrefs.GetInt("MaxLevelText") == 0)
            currentLevelText = 0;
        else
            currentLevelText = (PlayerPrefs.GetInt("MaxLevelText"));

        startDrawingWay = false;
    }

    private void Update()
    {
        if(_startUI.activeSelf)
        {
            if (Input.touchCount > 0)
            {
                StartGame();
            }
        }
        
        if(gameOver == false && win == false)
        {
            TouchProcessing();
        }
        else if (win == true)
        {
            SupersonicWisdom.Api.NotifyLevelCompleted(currentLevelIndex, null);

            _finger.SetActive(false);

            _winUI.SetActive(true);
            _konfeti.SetActive(true);
            if (Input.touchCount > 0)
            {
                win = false;
                _winUI.SetActive(false);
                _konfeti.SetActive(false);
                currentLevelText++;
                if (currentLevelIndex < _levels.Length - 1)
                {
                    currentLevelIndex++;
                    StartCoroutine(LoadLevel(currentLevelIndex));
                }
                else
                {
                    currentLevelIndex = Random.Range(0, _levels.Length);
                    StartCoroutine(LoadLevel(currentLevelIndex));
                }
            }
        }
        else if (gameOver == true)
        {
            _finger.SetActive(false);

            _gameOverUI.SetActive(true);
            if (Input.touchCount > 0)
            {
                gameOver = false;
                _gameOverUI.SetActive(false);
                //app
                SupersonicWisdom.Api.NotifyLevelFailed(currentLevelIndex, null);
                AppMetrica.Instance.ReportEvent(message: "level_restart");
                //metrica
                StartCoroutine(LoadLevel(currentLevelIndex));
            }
        }
    }
    private IEnumerator disableFade()
    {
        yield return new WaitForSeconds(1.5f);
        _screenFadeAnimator.gameObject.SetActive(false);
    }

    private void StartGame()
    {
        

        _finger.SetActive(false);

        _startUI.SetActive(false);
        _level.gameObject.SetActive(true);
        _level.text = "Level " + (currentLevelText + 1);

        _pauseButton.gameObject.SetActive(true);
        _resumeButton.gameObject.SetActive(false);
        _exitButton.gameObject.SetActive(false);

        currentLevel = Instantiate(_levels[currentLevelIndex], Vector3.zero, _levels[currentLevelIndex].transform.rotation) as LevelInfo;
        currentEnemies = currentLevel.GetComponentsInChildren<Enemy>();

        var rotated = 0;
        if (currentLevel.transform.eulerAngles.y == 0) rotated = 1;
        else rotated = -1;

        player = Instantiate(_mainData.PlayerModel(), currentLevel.StartPose() * rotated, Quaternion.identity).GetComponent<Player>();
        player.SetStats(_mainData.Speed(), _mainData.RotationSpeed(), currentLevel.MinHP(), currentEnemies.Length);

        //app
        SupersonicWisdom.Api.NotifyLevelStarted(currentLevelIndex, null);
        AppMetrica.Instance.ReportEvent(message: "level_start");
        player.levelStartTime = Time.time;
        //metrica

        StartCoroutine(disableFade());
        if (currentLevelText == 0) _finger.SetActive(true); 
    }

    private IEnumerator LoadLevel(int currentLevelIndex)
    {
        _finger.SetActive(false);
        if (currentLevelText == 0) _finger.SetActive(true);

        _screenFadeAnimator.gameObject.SetActive(true);
        _screenFadeAnimator.SetTrigger("FadeIn");
        yield return new WaitForSeconds(1.5f);
        _screenFadeAnimator.gameObject.SetActive(false);

        Destroy(player.gameObject);
        Destroy(currentLevel.gameObject);

        currentLevel = Instantiate(_levels[currentLevelIndex], Vector3.zero, _levels[currentLevelIndex].transform.rotation) as LevelInfo;
        currentEnemies = currentLevel.GetComponentsInChildren<Enemy>();

        var rotated = 0;
        if (currentLevel.transform.eulerAngles.y == 0) rotated = 1;
        else rotated = -1;

        player = Instantiate(_mainData.PlayerModel(), currentLevel.StartPose() * rotated, Quaternion.identity).GetComponent<Player>();
        player.SetStats(_mainData.Speed(), _mainData.RotationSpeed(), currentLevel.MinHP(), currentEnemies.Length);

        //app
        AppMetrica.Instance.ReportEvent(message: "level_start");
        player.levelStartTime = Time.time;
        //metrica

        _level.text = "Level " + (currentLevelText + 1);
        _screenFadeAnimator.gameObject.SetActive(true);
        StartCoroutine(disableFade());

        PlayerPrefs.SetInt("MaxLevelIndex", currentLevelIndex);
        PlayerPrefs.SetInt("MaxLevelText", currentLevelText);
    }

    private void TouchProcessing()
    {
        if (Input.touchCount > 0 &&  player.isMoving() == false)
        {
            touch = Input.GetTouch(0);

            //if (_finger.activeSelf) _finger.SetActive(false);

            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform.tag == "Player")
                    {
                        startDrawingWay = true;
                        player.getStartTouchData();
                    }
                }
            }

            if (touch.phase == TouchPhase.Moved)
            {
                if (startDrawingWay)
                {
                    Ray ray = Camera.main.ScreenPointToRay(touch.position);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit))
                    {
                        if (hit.transform.tag != "Wall")
                        {
                            player.getMovedTouchData(hit.point);
                        }
                        else
                        {
                            startDrawingWay = false;
                        }
                    }
                }
            }

            if (touch.phase == TouchPhase.Ended)
            {
                player.getEndTouchData();
                startDrawingWay = false;
            }
        }
    }

    public void Pause()
    {
        _pauseButton.gameObject.SetActive(false);
        _resumeButton.gameObject.SetActive(true);
        _exitButton.gameObject.SetActive(true);
    }

    public void Resume()
    {
        _pauseButton.gameObject.SetActive(true);
        _resumeButton.gameObject.SetActive(false);
        _exitButton.gameObject.SetActive(false);
    }

    public void Exit()
    {
        Application.Quit();
    }
}
