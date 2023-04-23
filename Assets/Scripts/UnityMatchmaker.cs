
using Fusion.Sample.DedicatedServer;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Multiplay;
using UnityEngine;

public class UnityMatchmaker : MonoBehaviour
{
    public const int MAX_PLAYER_AMOUNT = 2;

    // Unity matchmaker
    private float autoAllocateTimer = 9999999f;
    private bool alreadyAutoAllocated;
    private static IServerQueryHandler serverQueryHandler;
    private string backfillTicketId;
    private float acceptBackfillTicketsTimer;
    private float acceptBackfillTicketsTimerMax = 1.1f;
    private PayloadAllocation payloadAllocation;


    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        if (CommandLineUtils.IsHeadlessMode() == false)
        {
            return;
        }

        InitUnityMatchmaker();
    }

    private void Update()
    {
        autoAllocateTimer -= Time.deltaTime;
        if (autoAllocateTimer <= 0f)
        {
            autoAllocateTimer = 999f;
            MultiplayEventCallbacks_Allocate(null);
        }

        if (serverQueryHandler != null)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                serverQueryHandler.CurrentPlayers = (ushort)NetworkManager.Singleton.ConnectedClientsIds.Count;
            }

            serverQueryHandler.UpdateServerCheck();
        }

        if (backfillTicketId != null)
        {
            acceptBackfillTicketsTimer -= Time.deltaTime;
            if (acceptBackfillTicketsTimer <= 0f)
            {
                acceptBackfillTicketsTimer = acceptBackfillTicketsTimerMax;
                HandleBackfillTickets();
            }
        }
    }

    private async void InitUnityMatchmaker()
    {
        MultiplayEventCallbacks multiplayEventCallbacks = new MultiplayEventCallbacks();
        multiplayEventCallbacks.Allocate += MultiplayEventCallbacks_Allocate;
        multiplayEventCallbacks.Deallocate += MultiplayEventCallbacks_Deallocate;
        multiplayEventCallbacks.Error += MultiplayEventCallbacks_Error;
        multiplayEventCallbacks.SubscriptionStateChanged += MultiplayEventCallbacks_SubscriptionStateChanged;
        IServerEvents serverEvents =
            await MultiplayService.Instance.SubscribeToServerEventsAsync(multiplayEventCallbacks);

        serverQueryHandler =
            await MultiplayService.Instance.StartServerQueryHandlerAsync(4, "MyServerName", "action", "1.0", "Default");

        var serverConfig = MultiplayService.Instance.ServerConfig;
        if (serverConfig.AllocationId != "")
        {
            // Already Allocated
            MultiplayEventCallbacks_Allocate(new MultiplayAllocation("", serverConfig.ServerId,
                serverConfig.AllocationId));
        }
    }

    private void MultiplayEventCallbacks_Deallocate(MultiplayDeallocation obj)
    {
        Debug.Log("DEDICATED_SERVER MultiplayEventCallbacks_Deallocate");
    }

    private void MultiplayEventCallbacks_Error(MultiplayError obj)
    {
        Debug.Log("DEDICATED_SERVER MultiplayEventCallbacks_Error");
        Debug.Log(obj.Reason);
    }

    private void MultiplayEventCallbacks_SubscriptionStateChanged(MultiplayServerSubscriptionState obj)
    {
        Debug.Log("DEDICATED_SERVER MultiplayEventCallbacks_SubscriptionStateChanged");
        Debug.Log(obj);
    }

    private void MultiplayEventCallbacks_Allocate(MultiplayAllocation obj)
    {
        Debug.Log("DEDICATED_SERVER MultiplayEventCallbacks_Allocate");

        if (alreadyAutoAllocated)
        {
            Debug.Log("Already auto allocated!");
            return;
        }

        SetupBackfillTickets();

        alreadyAutoAllocated = true;

        var serverConfig = MultiplayService.Instance.ServerConfig;
        Debug.Log($"Server ID[{serverConfig.ServerId}]");
        Debug.Log($"AllocationID[{serverConfig.AllocationId}]");
        Debug.Log($"Port[{serverConfig.Port}]");
        Debug.Log($"QueryPort[{serverConfig.QueryPort}]");
        Debug.Log($"LogDirectory[{serverConfig.ServerLogDirectory}]");

        string ipv4Address = "0.0.0.0";
        ushort port = serverConfig.Port;
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ipv4Address, port, "0.0.0.0");
    }

    private async void SetupBackfillTickets()
    {
        Debug.Log("SetupBackfillTickets");
        payloadAllocation = await MultiplayService.Instance.GetPayloadAllocationFromJsonAs<PayloadAllocation>();

        backfillTicketId = payloadAllocation.BackfillTicketId;
        Debug.Log("backfillTicketId: " + backfillTicketId);

        acceptBackfillTicketsTimer = acceptBackfillTicketsTimerMax;
    }

    private async void HandleBackfillTickets()
    {
        if (HasAvailablePlayerSlots())
        {
            BackfillTicket backfillTicket =
                await MatchmakerService.Instance.ApproveBackfillTicketAsync(backfillTicketId);
            backfillTicketId = backfillTicket.Id;
        }
    }


    // private async void HandleUpdateBackfillTickets() {
    //     if (backfillTicketId != null && payloadAllocation != null && HasAvailablePlayerSlots()) {
    //         Debug.Log("HandleUpdateBackfillTickets");
    //
    //         List<Unity.Services.Matchmaker.Models.Player> playerList = new List<Unity.Services.Matchmaker.Models.Player>();
    //
    //         foreach (PlayerData playerData in KitchenGameMultiplayer.Instance.GetPlayerDataNetworkList()) {
    //             playerList.Add(new Unity.Services.Matchmaker.Models.Player(playerData.playerId.ToString()));
    //         }
    //
    //         MatchProperties matchProperties = new MatchProperties(
    //             payloadAllocation.MatchProperties.Teams, 
    //             playerList, 
    //             payloadAllocation.MatchProperties.Region, 
    //             payloadAllocation.MatchProperties.BackfillTicketId
    //         );
    //
    //         try {
    //             await MatchmakerService.Instance.UpdateBackfillTicketAsync(payloadAllocation.BackfillTicketId,
    //                 new BackfillTicket(backfillTicketId, properties: new BackfillTicketProperties(matchProperties))
    //             );
    //         } catch (MatchmakerServiceException e) {
    //             Debug.Log("Error: " + e);
    //         }
    //     }
    // }

    public bool HasAvailablePlayerSlots()
    {
        return NetworkManager.Singleton.ConnectedClientsIds.Count < MAX_PLAYER_AMOUNT;
    }
}

public class PayloadAllocation
{
    public Unity.Services.Matchmaker.Models.MatchProperties MatchProperties;
    public string GeneratorName;
    public string QueueName;
    public string PoolName;
    public string EnvironmentId;
    public string BackfillTicketId;
    public string MatchId;
    public string PoolId;
}