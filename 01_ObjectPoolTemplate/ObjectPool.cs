using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolWrapper<T> where T : Transform
{
    public T Instance;
    public float LifeTime;
    public float Duration;
    public bool Enable; // true �Ͻ� ��� ������(Object Pool�� ��������, �ΰ��ӿ��� ������� �ƴ�)

    public ObjectPoolWrapper(T instance, float lifeTime)
    {
        Instance = instance;
        instance.gameObject.SetActive(false);
        LifeTime = lifeTime;
        Duration = 0.0f;
        Enable = true;
    }

    public virtual void Reset()
    {
        Instance.gameObject.SetActive(false);
        Duration = 0.0f;
        Enable = true;
    }
}

/// <summary>
/// ������ ������ ��� ������ �ʿ��� ������Ʈ�� ����ϴ� Ǯ���� Ŭ�����Դϴ�.
/// �ڽ� Ŭ������ �����Ͽ� Set(), OnUpdated() �� �����ؼ� ����ϴ� ���� �����մϴ�.
/// </summary>
/// <typeparam name="T"> Instance�� ���� �� �ִ� Transform / RectTrasnform �� ����մϴ�. </typeparam>
public abstract class ObjectPool<T, Q>
    where T : Transform
    where Q : ObjectPoolWrapper<T>
{
    protected GameObject _prefab;
    protected uint _limitSize;
    protected float _generalLifeTime;
    protected Transform _root;
    protected Queue<Q> _pool;
    protected List<Q> _working;

    /// <summary>
    /// Prefab �� Resources���� �ҷ��� ����, parent ���� �� �ʱ� 1ȸ�� �����ϴ� �����Դϴ�.
    /// </summary>
    /// <param name="objectKey"> Resources.Load�� ȣ���� �� �ִ� Prefab ��� </param>
    /// <param name="parent"> ��� instance�� �θ��� root�� �θ�� ��ġ�� Transform </param>
    /// <param name="generalLifeTime"></param>
    /// <param name="expectedSize"></param>
    /// <param name="rootName"></param>
    public virtual void Init(string objectKey, Transform parent, float generalLifeTime, uint expectedSize = 10, string rootName = "")
    {
        _pool = new Queue<Q>();
        _working = new List<Q>();

        _generalLifeTime = generalLifeTime;
        _limitSize = expectedSize / 2 + 1;
        _limitSize = (_limitSize <= 1) ? 2 : _limitSize;
        var root = new GameObject();
        root.transform.parent = parent;
        root.name = (string.IsNullOrEmpty(rootName)) ? objectKey : rootName;
        _root = root.transform;
        _prefab = Resources.Load(objectKey) as GameObject;
        if (_prefab == null)
            Debug.LogError($"Resources: '{objectKey}' is not exist");
    }

    /// <summary>
    /// Pool���� ������ Object�� ����ϱ� �����մϴ�.
    /// </summary>
    /// <example>
    /// {
    ///    if (_limitSize > _pool.Count)
    ///    {
    ///        for (int i = 0; i < _limitSize; i++)
    ///        {
    ///           var ins = GameObject.Instantiate(_prefab, Vector3.zero, Quaternion.identity, _root) as T;
    ///           var insWrap = new ObjectPoolWrapper<T>(ins, _generalLifeTime);
    ///            _pool.Enqueue(insWrap);
    ///        }
    ///    }
    ///
    ///     var target = _pool.Dequeue();
    ///   _working.Add(target);
    ///   target.Instance.gameObject.SetActive(true);
    ///   return target.Instance;
    /// }
    ///</example>
    public abstract Q Set();

    /// <summary>
    /// �����ϴ� Scene���� �� �����Ӹ��� OnUpdated�� �����Ͽ� �۵��մϴ�.
    /// </summary>
    /// <param name="deltaTime"> Ŭ���̾�Ʈ delta Time </param>
    public virtual void OnUpdated(float deltaTime)
    {
        for (int i = _working.Count - 1; 0 <= i; i--)
        {
            var target = _working[i];
            if (target.Enable)
                continue;

            target.Duration += deltaTime;
            if (target.LifeTime <= target.Duration)
            {
                target.Reset();
                _pool.Enqueue(target);
                _working.RemoveAt(i);
            }
        }
    }
}
