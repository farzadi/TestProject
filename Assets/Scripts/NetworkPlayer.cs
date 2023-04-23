using Fusion;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    public static NetworkPlayer Local { get; set; }

    [Networked(OnChanged = nameof(OnColorChanged))]
    public NetworkString<_16> playerColor { get; set; }
    
    
    // Start is called before the first frame update
    void Start()
    {
        Color randomColor = Random.ColorHSV();
        playerColor = "#"+ColorUtility.ToHtmlStringRGBA(randomColor);
        Debug.Log("playerColor.Length" + playerColor.Length);
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Local = this;
          
            if (!playerColor.Equals("") || (playerColor.Length > 1 && playerColor.Length < 16))
            {
                RPC_SetColor(playerColor);
            }
           
            Debug.Log("Spawned local player");
        }
        else Debug.Log("Spawned remote player");
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (player == Object.InputAuthority)
            Runner.Despawn(Object);
    }

    static void OnColorChanged(Changed<NetworkPlayer> changed)
    {
        changed.Behaviour.OnColorChanged();
    }
    
    private void OnColorChanged()
    {

        string hexColor = playerColor.ToString(); 
        Color newColor;

        if (ColorUtility.TryParseHtmlString(hexColor, out newColor))
        {
            GetComponent<Renderer>().material.color = newColor;
        }
    }
    
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetColor(NetworkString<_16> playerColor, RpcInfo info = default)
    {
        this.playerColor = playerColor;
    }
}