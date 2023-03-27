using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Enemy : MonoBehaviour
{
    [SerializeField] private TextMeshPro _hp;
    [SerializeField] private Animator _animator;
    [SerializeField] private GameObject _model;

    private bool dead = false;

    private void Start()
    {
        dead = false;
        transform.LookAt(new Vector3(0, 0, 1) + transform.position);
    }
    public int GetHP()
    {
        return int.Parse(_hp.text);
    }
    public Animator GetAnimator()
    {
        return _animator;
    }
    public GameObject GetModel()
    {
        return _model;
    }

    public IEnumerator Dying()
    {
        dead = true;
        yield return new WaitForSeconds(0.66f);
        _animator.SetBool("isDying", true);
        yield return new WaitForSeconds(3);
        Destroy(gameObject);
    }

    public IEnumerator StopAttacking()
    {
        yield return new WaitForSeconds(1);
        _animator.SetBool("isAttacking", false);
    }

    public bool IsDead()
    {
        return dead;
    }
}
