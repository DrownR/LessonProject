using UnityEngine;

public interface IUpdatable : IManagedObject
{
    /// <summary>
    /// Method called every frame for objects registered in <see cref="UpdateManager"/>.
    /// </summary>
    /// <seealso cref="UpdateManager"/>
    void ManagedUpdate();
}
