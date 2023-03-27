using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Equipment : MonoBehaviour
{
    [SerializeField] private TextMeshPro _hp;
    [SerializeField] private GameObject _krishka;

    private char effectType;


    private void Start()
    {
        transform.LookAt(new Vector3(0, 0, 1) + transform.position);
        effectType = _hp.text[0];
    }
    public int ChangeHP(int hp)
    {
        if (effectType == '+') hp += int.Parse(_hp.text.Substring(1));
        else if (effectType == '-')
        {
            hp -= int.Parse(_hp.text.Substring(1));
            if (hp < 0) hp = 0;
            StartCoroutine(Disappear());
        }
        else if (effectType == '/')
        {
            hp = (int)(hp / int.Parse(_hp.text.Substring(1)));
            if (hp < 0) hp = 0;
            StartCoroutine(Disappear());
        }
        else if (effectType == 'x') hp *= int.Parse(_hp.text.Substring(1));

        if(_krishka) StartCoroutine(Opening());
        GetComponent<BoxCollider>().enabled = false;
        _hp.gameObject.SetActive(false);
        return hp;
    }

    IEnumerator Opening()
    {
        while(_krishka.transform.eulerAngles.x < 90)
        {
            _krishka.transform.Rotate(3, 0, 0);
            yield return null;
        }
        _krishka.transform.localRotation = Quaternion.Euler(45, 0, 0);
    }

    IEnumerator Disappear()
    {
        float i = 0;
        while(i < 2)
        {
            transform.Translate(0, -0.01f, 0);
            i += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }
}
