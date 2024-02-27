namespace CharacterStates
{
    public class SkillState : CharacterState
    {
        private CharacterEntity _target;

        public override void Init(CharacterEntity entity, params object[] param)
        {
            base.Init(entity);
            _target = (CharacterEntity)param[0];
            _owner.LookTarget(_target.Position);
            foreach (var skill in _owner.Skills)
            {
                if (skill.IsSpelling == false)
                {
                    skill.SetTarget(_target);
                }
            }
        }

        public override void Routine(float deltaTime)
        {
            base.Routine(deltaTime);

            bool everySkillNotUsing = true;
            foreach (var skill in _owner.Skills)
            {
                if (skill.IsAutoSpellable())
                {
                    skill.StartSpell(default, _target);
                    skill.Spell(out var isEnd);
                    everySkillNotUsing = false;
                    continue;
                }
                else if (skill.IsSpelling)
                {
                    skill.Spell(out var isEnd);
                    everySkillNotUsing = false;
                    continue;
                }
            }

            if (_owner.CommonAttack.IsAutoSpellable())
            {
                _owner.CommonAttack.StartSpell(default, _target);
                _owner.CommonAttack.Spell(out var isEnd);
                everySkillNotUsing = false;
            }
            else if (_owner.CommonAttack.IsSpelling)
            {
                _owner.CommonAttack.Spell(out var isEnd);
                everySkillNotUsing = isEnd;
            }

            if (everySkillNotUsing)
            {
                _owner.PopState();
                return;
            }
        }
    }
}