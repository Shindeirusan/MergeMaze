using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelInfo : MonoBehaviour
{
    [SerializeField] private int _minHP;
    [SerializeField] private GameObject _startPose;

    public int MinHP()
    {
        return _minHP;
    }
    public Vector3 StartPose()
    {
        return _startPose.transform.localPosition;
    }
}
