# portfolio
코드 스타일을 보여주기 위한 포트폴리오 문서입니다.

# Code1. ObjectPoolTemplate
```반복해서 사용하는 ObjectPool을 generic한 코드로 만들고싶어 사용한 Template```
다양한 게임을 만들면서, 오브젝트 풀링을 여러 번 만들었습니다. 자주 나오는 기능인만큼 코드를 최대한 재사용 할 수 있는 방향으로 구현하고 싶었습니다. 기본적으로 **시간이 지나면 사용을 중지하고 풀로 돌아가는** 기능은 제공하되 풀링하는 객체들의 종류, 초기화, 루틴을 커스터마이징 할 수 있도록 했습니다.
- ObjectPool.cs
- DamageFloaterManager.cs

# Code2. CharacterState
```추상클래스를 이용한 스택식 캐릭터 상태 변화 구현```
캐릭터가 현재 무슨 행동을 하는지 이동, 탐색, 공격, 스킬, 행동 불가를 추상클래스를 이용하여 구현했습니다. 상태는 매 프레임 루틴 함수를 실행합니다. 다른 상태로 넘어갈 때에는 현재 진행 중인 상태 스택 위에 새로운 상태를 생성하거나, 현재 상태를 스택에서 제거합니다.
- CharacterState.cs
- SkillState.cs
- AttackState.cs

# Code3. MVPatternCharacter
```캐릭터를 Model(Entity)와 View로 나눠 구현한 코드```
캐릭터의 이동, 공격 등 행동 로직과 GameObject의 위치, 애니메이션 등의 코드를 분할하기 위하여 MV* 패턴을 응용하여 제작했습니다. 컨트롤은 4개의 캐릭터 중 순서대로 앞의 3개의 캐릭터만 사용하여 전달해줍니다.
- CharacterEntity
- CharacterView.cs
- BattleManager.cs

# Code4. BattleSystem
```전투시스템에서 1. 초기화와 루틴 2. Call을 나누어 구현한 코드```
전투시스템을 관리하는 클래스를 얼마나 접근하는지에 따라 분할했습니다. 초기화와 루틴이 되는 부분은 BattleManager로, 다른 함수에서 호출하는 부분은 BattleService로 분할하여 작성했습니다.
- BattleManager.cs
- BattlerService.cs
