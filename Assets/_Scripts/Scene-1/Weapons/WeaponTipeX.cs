using UnityEngine;

public class WeaponTipeX : WeaponRange
{
    public override void OnAttack()
    {
        // Play animation

        // Only do this if local
        if (IsLocal())
        {
            // Send massage to spawn bullet
            Vector2 attackPoint = GetOwnerAttackPoint();
            Vector2 mousePos = GetOwnerMousePos();
            //NetworkClient.Instance.SpawnBullet()
        }
    }

    public void SpawnBullet(Vector2 spawnPos, Vector2 mosePos)
    {
        // Make a bullet
        
        GameObject temp = Instantiate(bullet, spawnPos, Quaternion.identity);
    }
}
