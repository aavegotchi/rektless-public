using Weapons.Base;

namespace Weapons.Implementations
{
    public class Axe : ThrowableWeapon
    {
        protected override void Awake()
        {
            base.Awake();
            rb.freezeRotation = true; // Prevent the axe from rotating
        }
    }
}