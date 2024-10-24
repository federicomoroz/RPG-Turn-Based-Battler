using Projectiles;
using Pool;
public static class Factory
{
    public static Projectile CreateProjectile(SO_Projectile data)
    {        
        var projectile = PoolManager.GetObject<Projectile>()
            .SetData(data)
            .Initialize();

        return projectile;
    }    
}
