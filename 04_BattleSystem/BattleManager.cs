using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    private bool _enable;
    private StageEntity _stageEntity;
    private List<CharacterEntity> _characters;
    private CameraManager _camera;
    private UserTouchInput _userInput;
    private FXManager _fxManager;
    private TargetingSystem _targetingSystem;
    private BattleUI _battleUI;

    private Transform _dynamicCanvas;
    private Transform _characterParent;
    private Transform _fxParent;
    private int _currentSkillButtonIndex = -1;

    private List<CharacterTagBehaviour> _characterTagBehaviours;
    private List<CharacterSkill> _playerSkills;

    public Transform DynamicCanvas => _dynamicCanvas;
    public Transform CharacterParent => _characterParent;
    public Transform FxParent => _fxParent;
    public List<CharacterEntity> Characters => _characters;

    private void Start()
    {
        _dynamicCanvas = transform.Find("DynamicCanvas");
        _characterParent = transform.Find("CharacterParent");
        _fxParent = transform.Find("FXParent");
        var targetingSkillPreview = _fxParent.Find("TargetingSkillPreview");

        _camera = new CameraManager();
        _userInput = new UserTouchInput();
        BattleService.Init(this);
        _fxManager = new FXManager(_dynamicCanvas, _fxParent);
        _characters = new List<CharacterEntity>();
        _characterTagBehaviours = new List<CharacterTagBehaviour>();
        _playerSkills = new List<CharacterSkill>();
        _targetingSystem = new TargetingSystem();
        _targetingSystem.SetParent(targetingSkillPreview);
        _battleUI = new BattleUI();
        var battleUIRoot = transform.Find("StaticCanvas").Find("BattleUI");
        _battleUI.Init(battleUIRoot);

        _targetingSystem.EnteredTargetingScreen += _camera.SetOnDimmed;
        _targetingSystem.CancealedTargetingScreen += _camera.SetOffDimmed;
        _targetingSystem.SpelledTargetingScreen += _camera.SetOffDimmed;
        _targetingSystem.EnteredTargetingScreen += ReadySkill;
        _targetingSystem.CancealedTargetingScreen += CancealSkill;
        _targetingSystem.SpelledTargetingScreen += UseSkill;
        _targetingSystem.EnteredTargetingScreen += SetTimeSlower;
        _targetingSystem.CancealedTargetingScreen += SetTimeOrigin;
        _targetingSystem.SpelledTargetingScreen += SetTimeOrigin;

        _userInput.DragedScreen += _camera.AddPosition;
        _userInput.PressedBackward += _camera.MoveBackward;
        _userInput.PressedFrontward += _camera.MoveForward;
        _userInput.PressedNumber1 += EnterSkill1;
        _userInput.PressedNumber2 += EnterSkill2;
        _userInput.PressedNumber3 += EnterSkill3;
        _userInput.ClickedUpPosition += StartSkill;

        _userInput.ClickedDownPosition += AimSkill;

        int dummyStage = 100;
        InitStage(dummyStage);

        _enable = true;
    }
    private void SetTimeSlower()
    {
        TimeHelper.TimeScale = 0.5f;
    }
    private void SetTimeOrigin()
    {
        TimeHelper.TimeScale = 1.0f;
    }

    #region Player Skill Controller
    private void ReadySkill()
    {
        _battleUI.SetExSkillButtonReady(_currentSkillButtonIndex, true);
    }
    private void CancealSkill()
    {
        _battleUI.SetExSkillButtonReady(_currentSkillButtonIndex, false);
        _currentSkillButtonIndex = -1;
    }
    private void UseSkill()
    {
        _battleUI.SetExSkillButtonUse(_currentSkillButtonIndex, true);
        _battleUI.SetExSkillButtonReady(_currentSkillButtonIndex, false);
        _currentSkillButtonIndex = -1;
    }
    private void EnterSkill(int index)
    {
        if (_battleUI.IsExSkillButtonReady(index) == false)
            return;

        if (_currentSkillButtonIndex != -1)
        {
            CancealSkill();
        }
        _currentSkillButtonIndex = index;
        _targetingSystem.Enter(_playerSkills[index]);
        _battleUI.SetExSkillButtonReady(index, true);
    }
    public void EnterSkill1()
    {
        EnterSkill(0);
    }
    public void EnterSkill2()
    {
        EnterSkill(1);
    }
    public void EnterSkill3()
    {
        EnterSkill(2);
    }
    public void StartSkill(Vector3 clickedPosition)
    {
        if (_targetingSystem.CurrentSkill == null)
            return;

        var faceKey = _targetingSystem.CurrentSkill.Owner.CharacterExFaceKey;
        var skillName = _targetingSystem.CurrentSkill.Name;
        if (_targetingSystem.StartSkill(clickedPosition))
            _battleUI.ShowExSkill(faceKey, skillName);
    }
    public void AimSkill(Vector3 clickedPosition)
    {
        _targetingSystem.AimSkill(clickedPosition);
    }
    #endregion

    private void InitStage(int stageIndex)
    {
        TableManager.GetStageInfo(stageIndex, out _stageEntity);

        _battleUI.SetStageTitle(_stageEntity.Name);
        int[] dummyPlayerIndexs = { 1002, 1000, 1004, 1003 };
        for (int i = 0; i < dummyPlayerIndexs.Length; i++)
        {
            var index = dummyPlayerIndexs[i];
            if (TableManager.GetCharacterEntityInfo(index, out CharacterEntity characterEntity))
            {
                _characters.Add(characterEntity);
                var position = _stageEntity.PlayerSpawnPositions[i];
                characterEntity.SetPosition(position);
                characterEntity.SetTeam(BattleUnit.Team.Ally);
                characterEntity.InitView();
                characterEntity.Spawn(_characterParent);
                characterEntity.AddStandState();
                characterEntity.SetGoalPositions(_stageEntity.GoalPositions);

                var skills = characterEntity.Skills;
                for (int skillIndex = 0; skillIndex < skills.Length; skillIndex++)
                {
                    if (skills[skillIndex].IsEx == true)
                        _playerSkills.Add(skills[skillIndex]);
                }

                var characterTagObject = AssetLoader.Instantiate<GameObject>(AssetKeyHelper.CHARACTER_TAG_KEY);
                characterTagObject.transform.SetParent(BattleService.DynamicCanvas);
                var characterTagBehaviour = characterTagObject.GetComponent<CharacterTagBehaviour>();
                characterEntity.ChangedHealthPoint += characterTagBehaviour.SetGage;
                characterEntity.MovedUIPosition += characterTagBehaviour.SetPosition;
                characterEntity.DiedCharacter += characterTagBehaviour.SetInvisible;
                characterTagBehaviour.InitGage(CharacterTagBehaviour.GageType.Player);
                characterTagBehaviour.SetPosition(characterEntity.UIPosition);
                characterTagBehaviour.SetGage(1);
                _characterTagBehaviours.Add(characterTagBehaviour);
            }
        }

        _battleUI.SetSkillBlock1(_playerSkills[0]);
        _battleUI.SetSkillBlock2(_playerSkills[1]);
        _battleUI.SetSkillBlock3(_playerSkills[2]);

        for (int i = 0; i < _stageEntity.EnemyIndexes.Length; i++)
        {
            var index = _stageEntity.EnemyIndexes[i];
            if (TableManager.GetCharacterEntityInfo(index, out CharacterEntity characterEntity))
            {
                _characters.Add(characterEntity);
                var position = _stageEntity.EnemySpawnPositions[i];
                characterEntity.SetPosition(position);
                characterEntity.SetTeam(BattleUnit.Team.Enemy);
                characterEntity.InitView();
                characterEntity.Spawn(_characterParent);
                characterEntity.AddStandState();

                var characterTagObject = AssetLoader.Instantiate<GameObject>(AssetKeyHelper.CHARACTER_TAG_KEY);
                characterTagObject.transform.SetParent(BattleService.DynamicCanvas);
                var characterTagBehaviour = characterTagObject.GetComponent<CharacterTagBehaviour>();
                characterEntity.ChangedHealthPoint += characterTagBehaviour.SetGage;
                characterEntity.MovedUIPosition += characterTagBehaviour.SetPosition;
                characterEntity.DiedCharacter += characterTagBehaviour.SetInvisible;
                characterTagBehaviour.InitGage(characterTagBehaviour.GetGageType(characterEntity.Element));
                characterTagBehaviour.SetPosition(characterEntity.UIPosition);
                characterTagBehaviour.SetGage(1);
                _characterTagBehaviours.Add(characterTagBehaviour);
            }
        }
    }
    private void OnDestroy()
    {
        AssetLoader.Clear();
    }
    private void Update()
    {
        if (!_enable)
            return;

        float deltaTime = Time.deltaTime * TimeHelper.TimeScale;
        _userInput.Routine(deltaTime);
        foreach (var entity in _characters)
        {
            entity.ProceedTimer(deltaTime);
            entity.CurState.Routine(deltaTime);
        }
        _targetingSystem.Routine(deltaTime);
        _fxManager.Routine(deltaTime);
        _camera.Routine();
    }
}
