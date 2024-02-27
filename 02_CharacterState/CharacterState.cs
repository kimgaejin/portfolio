using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterState
{
    protected CharacterEntity _owner;

    public virtual void Routine(float deltaTime)
    {
        if (!_owner.Enable)
        {
            _owner.AddState<CharacterStates.DeadState>();
            return;
        }
    }
    public virtual void Init(CharacterEntity entity, params object[] param)
    {
        _owner = entity;
    }
    protected virtual void PopState()
    {
        _owner.PopState();
    }
}