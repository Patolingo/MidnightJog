using System.Collections.Generic;
using UnityEngine;

public class BlockableMonoBehaviour : MonoBehaviour, IBlockable
{
    public List<object> requiringBlock { get; private set; } = new List<object>();

    public bool isBlocked { get; private set; } = new bool();

    public virtual void Block(object whoIsBlocking)
    {
        if(!requiringBlock.Contains(whoIsBlocking))
        {
            requiringBlock.Add(whoIsBlocking);
            
            if(requiringBlock.Count > 0)
            {
                isBlocked = true;
            }
        }
    }

    public virtual void Unblock(object whoIsUnblocking)
    {
        if(requiringBlock.Contains(whoIsUnblocking))
        {
            requiringBlock.Remove(whoIsUnblocking);

            if(requiringBlock.Count == 0)
            {
                isBlocked = false;
            }
        }
    }
}
