using System.Collections.Generic;
using UnityEngine;

public interface IBlockable
{
    List<object> requiringBlock { get; }
    bool isBlocked { get; }

    void Block(object whoIsBlocking);
    void Unblock(object whoIsUnblocking);
}
