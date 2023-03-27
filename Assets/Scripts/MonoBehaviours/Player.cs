using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{
    [HideInInspector] public float levelStartTime;

    [SerializeField] private Transform _kickEffectPlace;
    [SerializeField] private ParticleSystem _kickEffect;
    [SerializeField] private ParticleSystem _upgradeEffect;
    [SerializeField] private Animator _animator;
    [SerializeField] private TextMeshPro _hpText;
    [SerializeField] private GameObject _model;
    [SerializeField] private GameObject _stepModel;

    // параметры игрока
    private GameObject[] steps = new GameObject[100];
    private float speed;
    private float rotationSpeed;
    private int hp;
    private int enemiesCount;

    // маршрут игрока
    private Vector3[] points = new Vector3[100];
    private Vector3 targetPoint;
    private Vector3 lastPoint;
    private int currentPointsCount;
    private float distanceBetweenPoints;
    private bool moving;

    private void Start()
    {
        levelStartTime = 0;
        transform.LookAt(new Vector3(0, 0, 1) + transform.position);

        _hpText.text = hp.ToString();
        moving = false;
        distanceBetweenPoints = 1;
    }

    public void SetStats(float _speed, float _rotationSpeed, int _hp, int _enemiesCount)
    {
        speed = _speed;
        rotationSpeed = _rotationSpeed;
        hp = _hp;
        enemiesCount = _enemiesCount;
    }

    public void getStartTouchData()
    {
        lastPoint = transform.position;
        points[0] = lastPoint;
        currentPointsCount = 1;
    }

    public void getMovedTouchData(Vector3 point)
    {
        targetPoint = point;
        targetPoint.y = transform.position.y;
        tryAddPoint(targetPoint);
    }

    public void getEndTouchData()
    {
        StartCoroutine(Move());
    }

    private void tryAddPoint(Vector3 point)
    {
        if(Vector3.Distance(lastPoint, point) > distanceBetweenPoints)
        {
            steps[currentPointsCount] = Instantiate(_stepModel, (point + lastPoint) / 2, Quaternion.LookRotation(point - lastPoint)) as GameObject;
            points[currentPointsCount] = point;
            currentPointsCount++;
            lastPoint = point;
        }
    }

    private IEnumerator Move()
    {
        if(currentPointsCount > 1)
        {
            Destroy(steps[0]);
            moving = true;
            _animator.SetBool("isRunning", true);
            _animator.SetBool("isAttacking", false);
            for (int i = 0; i < currentPointsCount - 1; i++)
            {

                _model.transform.LookAt(points[i + 1]);

                float movingToPointTime = 0;
                float timeToReachPoint = Vector3.Distance(points[i], points[i + 1]) / speed;
                while (movingToPointTime < timeToReachPoint && moving)
                {
                    transform.position = points[i] + (points[i + 1] - points[i]) * movingToPointTime / timeToReachPoint;
                    movingToPointTime += Time.deltaTime;
                    yield return null;
                }
                Destroy(steps[i + 1]);
            }
            currentPointsCount = 0;
            _animator.SetBool("isRunning", false);
            moving = false;
        }
    }

    public bool isMoving()
    {
        return moving;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Enemy")
        {
            if(!other.GetComponent<Enemy>().IsDead())
            {
                StartCoroutine(MakeKickEffect());

                foreach (GameObject step in steps)
                {
                    Destroy(step);
                }

                _animator.SetBool("isRunning", false);
                _model.transform.LookAt(other.transform.position);
                other.GetComponent<Enemy>().GetModel().transform.LookAt(transform.position);

                currentPointsCount = 0;
                StopCoroutine(Move());
                moving = false;

                if (hp >= other.GetComponent<Enemy>().GetHP())
                {

                    StartCoroutine(other.GetComponent<Enemy>().Dying());
                    _animator.SetBool("isAttacking", true);
                    StartCoroutine(StopAttacking(other.GetComponent<Enemy>()));
                    enemiesCount--;
                    if (enemiesCount == 0)
                    {
                        //app
                        IDictionary<string, object> json = new Dictionary<string, object>();
                        json.Clear();
                        json["Level"] = "Spent: " + (int)(Time.time - levelStartTime) + "seconds";
                        AppMetrica.Instance.ReportEvent(message: "level_complete", json);
                        //metrica
                        GameManager.win = true;
                    }
                }
                else
                {
                    //app
                    IDictionary<string, object> json = new Dictionary<string, object>();
                    json.Clear();
                    json["Level"] = "Spent: " + (int)(Time.time - levelStartTime) + "seconds, reason: enemy";
                    AppMetrica.Instance.ReportEvent(message: "level_fail", json);
                    //metrica

                    other.GetComponent<Enemy>().GetAnimator().SetBool("isAttacking", true);
                    StartCoroutine(other.GetComponent<Enemy>().StopAttacking());
                    _animator.SetBool("isDying", true);
                    hp = 0;
                    currentPointsCount = 0;
                    GameManager.gameOver = true;
                }
            }
        }
        else if (other.tag == "Equipment")
        {
            foreach (GameObject step in steps)
            {
                Destroy(step);
            }

            _animator.SetBool("isRunning", false);
            _model.transform.LookAt(other.transform.position);

            currentPointsCount = 0;
            moving = false;
            var hpBeforeTake = hp;
            hp = other.GetComponent<Equipment>().ChangeHP(hp);

            if(hp >= hpBeforeTake) Instantiate(_upgradeEffect, transform.position, Quaternion.identity);
            else Instantiate(_kickEffect, _kickEffectPlace.position, Quaternion.identity);

            _hpText.text = hp.ToString();
            if(hp == 0)
            {
                _animator.SetBool("isDying", true);
                currentPointsCount = 0;
                GameManager.gameOver = true;
                //app
                IDictionary<string, object> json = new Dictionary<string, object>();
                json.Clear();
                json["Level"] = "Spent: " + (int)(Time.time - levelStartTime) + "seconds, reason: trap";
                AppMetrica.Instance.ReportEvent(message: "level_fail", json);
                //metrica
            }
        }
    }

    IEnumerator MakeKickEffect()
    {
        yield return new WaitForSeconds(0.66f);
        Instantiate(_kickEffect, _kickEffectPlace.position, Quaternion.identity);
    }

    IEnumerator StopAttacking(Enemy enemy)
    {
        yield return new WaitForSeconds(0.66f);

        _animator.SetBool("isAttacking", false);
        hp += enemy.GetComponent<Enemy>().GetHP();
        _hpText.text = hp.ToString();
    }
}
