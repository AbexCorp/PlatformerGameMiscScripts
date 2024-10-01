/// <summary>
/// Can be blocked by player shield
/// </summary>
public interface IBlockable
{
    /// <summary>
    /// Called by shield script to block the attack or projectile
    /// </summary>
    public void GetBlocked();
}