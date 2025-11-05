using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    public NetworkVariable<Vector2> Position = new NetworkVariable<Vector2>(writePerm: NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        Position.OnValueChanged += OnStateChanged;
    }

    public override void OnNetworkDespawn()
    {
        Position.OnValueChanged -= OnStateChanged;
    }

    public void OnStateChanged(Vector2 previous, Vector2 current)
    {

    }
}
