using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class UpdateManager : MonoBehaviour
{
    /// <summary>Get or create the singleton instance</summary>
    public static UpdateManager Instance => _instance != null ? _instance : (_instance = CreateInstance());
    protected static UpdateManager _instance;

    private static UpdateManager CreateInstance()
    {
        var gameObject = new GameObject(nameof(UpdateManager))
        {
            hideFlags = HideFlags.DontSave,
        };
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            gameObject.hideFlags = HideFlags.HideAndDontSave;
        }
        else
#endif
        {
            DontDestroyOnLoad(gameObject);
        }
        return gameObject.AddComponent<UpdateManager>();
    }

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void DestroyEditorUpdateManager()
    {
        if (_instance)
        {
            DestroyImmediate(_instance.gameObject);
        }
    }
#endif

    /// <summary>
    /// Returns whether there are any objects registered for managed updates.
    /// </summary>
    public bool HasRegisteredObjects => _updatableObjects.Count > 0
        || _lateUpdatableObjects.Count > 0
        || _fixedUpdatableObjects.Count > 0;

    private readonly List<IUpdatable> _updatableObjects = new List<IUpdatable>();
    private readonly List<ILateUpdatable> _lateUpdatableObjects = new List<ILateUpdatable>();
    private readonly List<IFixedUpdatable> _fixedUpdatableObjects = new List<IFixedUpdatable>();

    protected void Update()
    {
        foreach (IUpdatable updatable in _updatableObjects)
        {
            try
            {
                updatable.ManagedUpdate();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }

    protected void LateUpdate()
    {
        foreach (ILateUpdatable lateUpdatable in _lateUpdatableObjects)
        {
            try
            {
                lateUpdatable.ManagedLateUpdate();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }

    protected void FixedUpdate()
    {
        foreach (IFixedUpdatable fixedUpdatable in _fixedUpdatableObjects)
        {
            try
            {
                fixedUpdatable.ManagedFixedUpdate();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }

    /// <summary>
    /// Register <paramref name="obj"/> to be updated every frame.
    /// </summary>
    /// <remarks>
    /// Registering updatable objects is O(1).
    /// Registering an object more than once is a no-op.
    /// </remarks>
    public void Register(IManagedObject obj)
    {
        if (obj is IUpdatable updatable)
        {
            _updatableObjects.Add(updatable);
        }
        if (obj is ILateUpdatable lateUpdatable)
        {
            _lateUpdatableObjects.Add(lateUpdatable);
        }
        if (obj is IFixedUpdatable fixedUpdatable)
        {
            _fixedUpdatableObjects.Add(fixedUpdatable);
        }
        enabled = HasRegisteredObjects;
    }

    /// <summary>
    /// Unregister <paramref name="updatable"/>, so it is not updated every frame anymore.
    /// </summary>
    /// <remarks>
    /// Unregistering updatable objects is O(1).
    /// Unregistering an object that wasn't registered is a no-op.
    /// </remarks>
    public void Unregister(IManagedObject obj)
    {
        if (obj is IUpdatable updatable)
        {
            _updatableObjects.Remove(updatable);
        }
        if (obj is ILateUpdatable lateUpdatable)
        {
            _lateUpdatableObjects.Remove(lateUpdatable);
        }
        if (obj is IFixedUpdatable fixedUpdatable)
        {
            _fixedUpdatableObjects.Remove(fixedUpdatable);
        }
        enabled = HasRegisteredObjects;
    }

    /// <summary>
    /// Unregisters all updatable objects at once.
    /// </summary>
    public void Clear()
    {
        _updatableObjects.Clear();
        _lateUpdatableObjects.Clear();
        _fixedUpdatableObjects.Clear();
        enabled = false;
    }
}
