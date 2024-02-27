using System.Collections.Generic;
using UnityEngine;

public static class BattleService
{
    private static BattleManager _manager;

    public static Transform DynamicCanvas => _manager.DynamicCanvas;
    public static Transform FxParent => _manager.FxParent;
    public static Transform CharacterParent => _manager.CharacterParent;
    public static float CLICK_VALID_DISTANCE = 1.0f;

    public static void Init(BattleManager manager)
    {
        _manager = manager;
    }

    public static bool GetCharactersInDetectRange(CharacterEntity owner, out CharacterEntity target)
    {
        target = default;
        float bestDistance = -1;
        foreach (CharacterEntity character in _manager.Characters)
        {
            if (!character.Enable)
                continue;

            if (character.Team == owner.Team)
                continue;

            var distance = Vector3.Distance(owner.Position, character.Position);
            bool isInDetectRange = distance * 10 < owner.DetectRange;
            if (!isInDetectRange)
                continue;

            bool isBestDistance = bestDistance == -1 || distance < bestDistance;
            if (!isBestDistance)
                continue;
            target = character;
            bestDistance = distance;
        }

        if (bestDistance != -1)
            return true;

        return false;
    }

    public static bool GetCharactersInForm(CharacterEntity owner, Vector3 centerPosition, Vector3 targetPosition, BattleUnit.Form form, int[] formParam, bool isTeam, out CharacterEntity[] targets)
    {
        var targetLists = new List<CharacterEntity>();
        foreach (CharacterEntity character in _manager.Characters)
        {
            if (!character.Enable)
                continue;

            if (isTeam && owner.Team != character.Team)
                continue;

            if (!isTeam && owner.Team == character.Team)
                continue;

            bool isIn = false;
            if (form.Equals(BattleUnit.Form.Circle))
            {
                var radius = formParam[0] * 0.1f;
                isIn = Vector3.Distance(targetPosition, character.Position) <= radius;
            }
            else if (form.Equals(BattleUnit.Form.Fan60))
            {
                var angle = 60;
                var radius = formParam[1];
                var isInRadius = Vector3.Distance(targetPosition, character.Position) <= radius;

                var arrow = new Vector2(targetPosition.x - centerPosition.x, targetPosition.z - centerPosition.z);
                var leftX = Mathf.Cos(-angle) * arrow.x - Mathf.Sin(-angle) * arrow.y;
                var leftY = Mathf.Sin(-angle) * arrow.x + Mathf.Cos(-angle) * arrow.y;
                var left = new Vector2(leftX, leftY);

                var rightX = Mathf.Cos(angle) * arrow.x - Mathf.Sin(angle) * arrow.y;
                var rightY = Mathf.Sin(angle) * arrow.x + Mathf.Cos(angle) * arrow.y;
                var right = new Vector2(rightX, rightY);

                // ! 총 각이 180도 이상인 경우, 아직 고려하지 않음
                var leftCross = (left.x * arrow.y) - (left.y * arrow.x);
                var rightCross = (arrow.x * right.y) - (arrow.y * right.x);
                var isInAngle = leftCross * rightCross >= 0;
                isIn = isInRadius && isInAngle;
            }

            if (isIn)
                targetLists.Add(character);
        }

        targets = targetLists.ToArray();
        return true;
    }
    public static bool GetCharactersTeam(CharacterEntity owner, out CharacterEntity[] targets)
    {
        var targetLists = new List<CharacterEntity>();
        foreach (CharacterEntity character in _manager.Characters)
        {
            if (!character.Enable)
                continue;

            if (owner.Team != character.Team)
                continue;
            
            targetLists.Add(character);
        }

        targets = targetLists.ToArray();
        return true;
    }
    public static void DamageEntity(CharacterEntity owner, CharacterEntity target, int value)
    {
        var elementMultiple = 1.0f;
        if (target.Element == CharacterEntity.ElementType.HYDRO && owner.Element == CharacterEntity.ElementType.PYRO) elementMultiple = 0.9f;
        else if (target.Element == CharacterEntity.ElementType.PYRO && owner.Element == CharacterEntity.ElementType.HYDRO) elementMultiple = 1.1f;
        else if (target.Element == CharacterEntity.ElementType.PYRO && owner.Element == CharacterEntity.ElementType.ELECTRO) elementMultiple = 1.1f;
        else if (target.Element == CharacterEntity.ElementType.ELECTRO && owner.Element == CharacterEntity.ElementType.PYRO) elementMultiple = 1.1f;
        var elementedDamage = value * elementMultiple;
        var finalDamage = (int)(elementedDamage - target.DefenseValue);

        target.Damaged(finalDamage);
        Vector3 damageFxPosition = new Vector3(target.Position.x, target.Position.y, target.Position.z);
        var damageFloatingParam = new object[2] {damageFxPosition, finalDamage.ToString() };
        FXService.Play(FXHelper.GetElementDamageFlopting(owner.Element), damageFloatingParam);
    }
    public static bool IsThereTeam(Vector3 position, in CharacterEntity actor, out CharacterEntity target)
    {
        target = null;
        bool isThereEntity = false;
        float smallestDistance = CLICK_VALID_DISTANCE;
        foreach (CharacterEntity character in _manager.Characters)
        {
            if (!character.Enable)
                continue;
            if (character.Team != actor.Team)
                continue;
            var distance = Vector3.Distance(position, character.Position);
            if (distance < smallestDistance)
            {
                isThereEntity = true;
                smallestDistance = distance;
                target = character;
            }
        }

        if (isThereEntity)
            return true;
        return false;
    }
    public static bool IsThereEnemy(Vector3 position, in CharacterEntity actor, out CharacterEntity target)
    {
        target = null;
        bool isThereEntity = false;
        float smallestDistance = CLICK_VALID_DISTANCE;
        foreach (CharacterEntity character in _manager.Characters)
        {
            if (!character.Enable)
                continue;
            if (character.Team == actor.Team)
                continue;
            var distance = Vector3.Distance(position, character.Position);
            if (distance < smallestDistance)
            {
                isThereEntity = true;
                smallestDistance = distance;
                target = character;
            }
        }

        if (isThereEntity)
            return true;
        return false;
    }
}
