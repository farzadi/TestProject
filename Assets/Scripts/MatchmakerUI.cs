using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
using UnityEngine.UI;

public class MatchmakerUI : MonoBehaviour
{
    public static MatchmakerUI Instance;
    
    public const string DEFAULT_QUEUE = "default-queue";

    [SerializeField] private Button findMatchButton;
    [SerializeField] private Text lookingForMatchTransform;
    [SerializeField] private Button joinBtn ;

    private CreateTicketResponse createTicketResponse;
    private float pollTicketTimer;
    private float pollTicketTimerMax = 1.1f;
    private string serverAddress;
    private ushort serverPort;
    private string matchId;

    private void Awake()
    {
        Instance = this;
        findMatchButton.onClick.AddListener(() => { FindMatch(); });
        joinBtn.interactable = false;
    }

    private async void FindMatch()
    {
        Debug.Log("FindMatch");

        lookingForMatchTransform.gameObject.SetActive(true);

        createTicketResponse = await MatchmakerService.Instance.CreateTicketAsync(
            new List<Player>
            {
                new Player(AuthenticationService.Instance.PlayerId)
            }, new CreateTicketOptions { QueueName = DEFAULT_QUEUE });

        // Wait a bit, don't poll right away
        pollTicketTimer = pollTicketTimerMax;
    }

    private void Update()
    {
        if (createTicketResponse != null)
        {
            // Has ticket
            pollTicketTimer -= Time.deltaTime;
            if (pollTicketTimer <= 0f)
            {
                pollTicketTimer = pollTicketTimerMax;

                PollMatchmakerTicket();
            }
        }
    }

    private async void PollMatchmakerTicket()
    {
        Debug.Log("PollMatchmakerTicket");
        TicketStatusResponse ticketStatusResponse =
            await MatchmakerService.Instance.GetTicketAsync(createTicketResponse.Id);

        if (ticketStatusResponse == null)
        {
            // Null means no updates to this ticket, keep waiting
            Debug.Log("Null means no updates to this ticket, keep waiting");
            return;
        }

        // Not null means there is an update to the ticket
        if (ticketStatusResponse.Type == typeof(MultiplayAssignment))
        {
            // It's a Multiplay assignment
            MultiplayAssignment multiplayAssignment = ticketStatusResponse.Value as MultiplayAssignment;

            Debug.Log("multiplayAssignment.Status " + multiplayAssignment.Status);
            switch (multiplayAssignment.Status)
            {
                case MultiplayAssignment.StatusOptions.Found:
                    createTicketResponse = null;
                    lookingForMatchTransform.text = "Match Found " + multiplayAssignment.MatchId;
                    Debug.Log(multiplayAssignment.Ip + " " + multiplayAssignment.Port + " " +
                              multiplayAssignment.MatchId);
                    joinBtn.interactable = true;

                    matchId = multiplayAssignment.MatchId;
                    serverAddress = multiplayAssignment.Ip;
                    serverPort = (ushort)multiplayAssignment.Port;
                    break;
                case MultiplayAssignment.StatusOptions.InProgress:
                    lookingForMatchTransform.text = "Searching For Match";
                    Debug.Log("Still waiting...");
                    break;
                case MultiplayAssignment.StatusOptions.Failed:
                    createTicketResponse = null;
                    Debug.Log("Failed to create Multiplay server!");
                    lookingForMatchTransform.gameObject.SetActive(false);
                    break;
                case MultiplayAssignment.StatusOptions.Timeout:
                    createTicketResponse = null;
                    Debug.Log("Multiplay Timeout!");
                    lookingForMatchTransform.gameObject.SetActive(false);
                    break;
            }
        }
    }

    public string GetServerAddress()
    {
        return serverAddress;
    }

    public ushort GetServerPort()
    {
        return serverPort;
    }

    public string GetMatchId()
    {
        return matchId;
    }
}