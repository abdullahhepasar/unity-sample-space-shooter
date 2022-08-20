using UnityEngine;
using Fusion;

public class AsteroidBehaviour : NetworkBehaviour
{
    [SerializeField] private int _points = 1;
    
    [HideInInspector][Networked] public NetworkBool IsBig { get; set; }
    [HideInInspector][Networked] public NetworkBool IsMedium { get; set; }

    public void HitAsteroid(PlayerRef player)
    {
        if (Object == null) return;
        if (Object.HasStateAuthority == false) return;
        
        if(IsMedium)
            FindObjectOfType<AsteroidSpawner>().BreakUpBigAsteroid(transform.position, IsMedium);
        if (IsBig)
            FindObjectOfType<AsteroidSpawner>().BreakUpBigAsteroid(transform.position, IsMedium);

        if (Runner.TryGetPlayerObject(player, out var playerNetworkObject))
            playerNetworkObject.GetComponent<PlayerDataNetworked>().AddToScore(_points);

        Runner.Despawn(Object);
    }
}
