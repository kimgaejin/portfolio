using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterView
{
    private GameObject _gameObject;
    private bool _enable;
    private Animator _animator;
    
    public GameObject Model { get { return _gameObject; } }
    public bool Enable { get { return _enable; } }

    public CharacterView(string characterAssetKey)
    {
        _gameObject = AssetLoader.Instantiate<GameObject>(characterAssetKey);
        _animator = _gameObject.GetComponent<Animator>();
        _gameObject.SetActive(false);
        _enable = false;
    }

    public void Spawn(Vector3 position)
    {
        _enable = true;
        if (_animator == null)
            _enable = false;
        SetPosition(position);
        _gameObject.SetActive(true);
    }
    public void SetPosition(Vector3 position, Quaternion rotation = default)
    {
        _gameObject.transform.SetPositionAndRotation(position, rotation);
    }
    public void SetParent(Transform parent)
    {
        _gameObject.transform.SetParent(parent);
    }
    public void LookAt(Vector3 position)
    {
        _gameObject.transform.LookAt(position);
    }
    public void PlayFire()
    {
        SetTrigger(_animator.GetAnimationID(AnimatorExtension.AnimType.FIRE));
    }
    public void PlayAttack(bool value)
    {
        SetBool(_animator.GetAnimationID(AnimatorExtension.AnimType.ATTACK), value);
    }
    public void PlayRun(bool value)
    {
        SetBool(_animator.GetAnimationID(AnimatorExtension.AnimType.RUN), value);
    }
    public void PlayDie()
    {
        SetBool(_animator.GetAnimationID(AnimatorExtension.AnimType.DIE), true);
    }
    public void PlaySkillEx()
    {
        SetTrigger(_animator.GetAnimationID(AnimatorExtension.AnimType.SKILL_EX));
    }
    public void PlaySkill1()
    {
        SetTrigger(_animator.GetAnimationID(AnimatorExtension.AnimType.SKILL_1));
    }
    private void SetTrigger(int id)
    {
        if (!_enable)
            return;

        _animator.SetTrigger(id);
    }
    private void SetBool(int id, bool value)
    {
        if (!_enable)
            return;

        _animator.SetBool(id, value);
    }
}