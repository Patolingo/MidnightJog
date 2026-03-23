using System;
using UnityEngine;

public abstract class ActorController : MonoBehaviour, IActorController, IDisposable
{
    public Actor controllingActor { get; set; }

    protected bool _disposed = false;

    public virtual void Dispose()
    {
        if (_disposed) return;

        if(controllingActor != null)
            controllingActor.Possess(null);

        _disposed = true;
    }

    public virtual void PossessActor(Actor actor)
    {
        if (actor == null && controllingActor != null)
            controllingActor.Possess(null);
        if (actor != null)
            actor.Possess(this);
        controllingActor = actor;
    }

    private void OnDestroy()
    {
        Dispose();
    }
    private void OnDisable()
    {
        Dispose();
    }
}
