using UnityEngine;

public interface IFixedUpdatable : IManagedObject
{
    /// <summary>
    /// Method called every physics frame for objects registered in <see cref="UpdateManager"/>.
    /// </summary>
    /// <seealso cref="UpdateManager"/>
    void ManagedFixedUpdate();
}
