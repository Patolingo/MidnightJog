public interface IActorController
{
    Actor controllingActor { get; set; }

    public void PossessActor(Actor actor);
}
