using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEntity
{
    public enum ElementType { NONE, HYDRO, PYRO, ANEMO, ELECTRO, GEO, DENDRO, CRYO };

    private bool _enable;
    private string _name;
    private int _maxHealthPoint;
    private int _healthPoint;
    private int _attackDamage;
    private float _defense;
    private float _moveSpeed;
    private string _characterModelKey;
    private string _characterFaceKey;
    private string _characterExFaceKey;
    private string _characterUpperSpriteKey;
    private int _attackSpeed;
    private int _attackRange;
    private int _detectRange;
    private Vector3 _uiPosition;
    private ElementType _element;

    private BattleUnit.Team _team;

    private Vector3 _position;
    private Quaternion _rotation;
    private CharacterView _view;
    private Stack<CharacterState> _state;
    private CharacterSkill _commonAttack;
    private CharacterSkill[] _skills;
    
    public Action<Vector3, Quaternion> MovedPosition;
    public Action<Vector3> MovedUIPosition;
    public Action<float> ChangedHealthPoint;
    public Action<bool> DiedCharacter;

    public bool Enable => _enable;
    public string Name => _name;
    public Vector3 Position => _position;
    public int HealthPoint => _healthPoint;
    public int MaxHealthPoint => _maxHealthPoint;
    public int AttackSpeed => _attackSpeed;
    public int AttackRange => _attackRange;
    public int DetectRange => _detectRange;
    public int AttackDamage => _attackDamage;
    public string CharacterFaceKey => _characterFaceKey;
    public string CharacterExFaceKey => _characterExFaceKey;
    public string CharacterUpperSpriteKey => _characterFaceKey;
    public Vector3 UIPosition => Position + _uiPosition;
    public float DefenseValue => _defense * 0.55f + 10;
    public BattleUnit.Team Team => _team;
    public ElementType Element => _element;
    public CharacterState CurState => _state.Peek();
    public CharacterSkill CommonAttack => _commonAttack;
    public CharacterSkill[] Skills => _skills;
    public GameObject Model => _view.Model;

    public CharacterEntity(string name
                            , int maxHealthPoint
                            , int attackSpeed
                            , int attackDamage
                            , float defense
                            , float moveSpeed
                            , string characterModelKey
                            , string faceKey
                            , string exFaceKey
                            , string upperKey
                            , int attackRange
                            , int detectRange
                            , float uiHeight
                            , ElementType element
                            , int commonAttackIndex
                            , int skill1Index
                            , int skill2Index)
    {
        _enable = false;
        
        _name = name;
        _maxHealthPoint = maxHealthPoint;
        _healthPoint = maxHealthPoint;
        _attackSpeed = attackSpeed;
        _attackDamage = attackDamage;
        _defense = defense;
        _moveSpeed = moveSpeed;
        _characterModelKey = characterModelKey;
        _characterFaceKey = faceKey;
        _characterExFaceKey = exFaceKey;
        _characterUpperSpriteKey = upperKey;
        _attackRange = attackRange;
        _detectRange = detectRange;
        _uiPosition = new Vector3(0, uiHeight, 0);
        _element = element;
        if (TableManager.GetCharacterSkillInfo(commonAttackIndex, this, out var commonSkill)) _commonAttack = commonSkill;
        
        _state = new Stack<CharacterState>();

        var skill = new List<CharacterSkill>();
        if (TableManager.GetCharacterSkillInfo(skill1Index, this, out var skill1)) skill.Add(skill1);
        if (TableManager.GetCharacterSkillInfo(skill2Index, this, out var skill2)) skill.Add(skill2);
        _skills = skill.ToArray();
    }
    public void SetTeam(BattleUnit.Team team)
    {
        _team = team;
    }
    public void AddStandState()
    {
        AddState<CharacterStates.StandState>();
    }
    public void SetGoalPositions(Vector3[] goalPositions)
    {
        AddStandState();

        for (int i = goalPositions.Length - 1; 0 < i; i -= 2)
        {
            var linePosition1 = goalPositions[i - 1];
            var linePosition2 = goalPositions[i];
            var moveState = AddState<CharacterStates.MoveLineState>();
            moveState.SetDestination(linePosition1, linePosition2);
        }
    }
    public void InitView()
    {
        _view = new CharacterView(_characterModelKey);
        MovedPosition += _view.SetPosition;
        SetPosition(Position);
    }
    public T AddState<T>(params object[] param) where T : CharacterState, new()
    {
        var state = new T();
        state.Init(this, param);
        _state.Push(state);
        return state;
    }
    public void PopState()
    {
        _state.Pop();
    }
    public void SetPosition(Vector3 position)
    {
        _position = position;
        MovedPosition?.Invoke(position, _rotation);
        MovedUIPosition?.Invoke(UIPosition);
    }
    public void Spawn(Transform parent)
    {
        _view.SetParent(parent);
        _view.Spawn(_position);
        _enable = true;
    }
    public void Move(Vector3 destination)
    {
        var nextPosition = _position + (destination - _position).normalized * _moveSpeed * TimeHelper.ClientDeltaTime;
        SetPosition(nextPosition);
        LookTarget(destination);
        Run(true);
    }

    #region Temporary
    public void LookTarget(Vector3 targetPosition)
    {
        _rotation = Quaternion.LookRotation(targetPosition - Position);
        _view.LookAt(targetPosition);
    }
    public void Die()
    {
        if (_enable)
        {
            _view.PlayDie();
            DiedCharacter?.Invoke(true);
            _enable = false;
        }
    }
    public void Run(bool value)
    {
        _view.PlayRun(value);
    }
    public void Attack(bool value)
    {
        _view.PlayAttack(value);
    }
    public void Fire()
    {
        _view.PlayFire();
    }
    public void SkillEx()
    {
        _view.PlaySkillEx();
    }
    public void Skill1()
    {
        _view.PlaySkill1();
    }
    #endregion
    public bool CheckSkillUsable()
    {
        foreach (var skill in _skills)
        {
            if (skill.IsAutoSpellable())
            {
                return true;
            }
        }
        if (CommonAttack.IsAutoSpellable())
        {
            return true;
        }
        return false;
    }
    public bool CheckDectectEnemy(out CharacterEntity target)
    {
        if (BattleService.GetCharactersInDetectRange(this, out target))
        {
            return true;
        }
        return false;
    }
    public bool CheckValidEnenmy(CharacterEntity target)
    {
        if (target == default)
            return false;

        if (!target.Enable)
            return false;

        return true;
    }
    public bool CheckValidShooting(CharacterEntity target)
    {
        if (Vector3.Distance(Position, target.Position) * 10 < AttackRange)
        {
            return true;
        }
        return false;
    }
    public void Damaged(int value)
    {
        _healthPoint -= value;
        var healthRate = HealthPoint / (float)MaxHealthPoint;
        ChangedHealthPoint?.Invoke(healthRate);
        if (HealthPoint < 0)
            Die();
    }
    public void ProceedTimer(float deltaTime)
    {
        foreach (var skill in _skills)
        {
            skill.ProceedTime(deltaTime);
        }
        CommonAttack.ProceedTime(deltaTime);
    }
}