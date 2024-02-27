namespace CharacterStates
{
    public class AttackState : CharacterState
    {
        private CharacterEntity _target;
        private float _attackCooltime;
        private readonly float _attackDoneWaitTime = 0.2f;

        public override void Init(CharacterEntity entity, params object[] param)
        {
            base.Init(entity);
            _target = (CharacterEntity)param[0];
            _attackCooltime = 100.0f / (float)entity.AttackSpeed;
            _owner.Attack(true);
        }
        protected override void PopState()
        {
            base.PopState();
            _owner.Attack(false);
        }
        public override void Routine(float deltaTime)
        {
            base.Routine(deltaTime);

            bool isValidEnenmy = _owner.CheckValidEnenmy(_target);
            if (!isValidEnenmy)
            {
                PopState();
                _owner.AddState<WaitState>(_attackDoneWaitTime);
                return;
            }

            bool isValidShooting = _owner.CheckValidShooting(_target);
            if (!isValidShooting)
            {
                PopState();
                _owner.AddState<WaitState>(_attackDoneWaitTime);
                return;
            }

            if (_owner.CheckSkillUsable())
            {
                _owner.AddState<SkillState>(_target);
                return;
            }
        }
    }
}