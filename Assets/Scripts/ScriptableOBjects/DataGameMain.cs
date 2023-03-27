using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Data", menuName = "Main")]
public class DataGameMain : ScriptableObject
{
    [SerializeField] private float _speed;
    [SerializeField] private float _rotationSpeed;
    [SerializeField] private GameObject _playerModel;

    public float Speed()
    {
        return _speed;
    }
    public float RotationSpeed()
    {
        return _rotationSpeed;
    }
    public GameObject PlayerModel()
    {
        return _playerModel;
    }
}
