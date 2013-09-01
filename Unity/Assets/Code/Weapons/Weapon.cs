using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public abstract WeaponType Type { get; }

    public void Fire(Ray fireDirection)
    {
        if (animation.isPlaying)
            return;
        
        animation.Play("BoxingGloveFire", PlayMode.StopAll);

        //if (!Server.IsClient)
        //    DoFire(fireDirection);
    }

    protected abstract void DoFire(Ray fireDirection);
}
