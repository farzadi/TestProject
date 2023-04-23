using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class UnityAuthentication : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        InitializeUnityAuthentication();
    }

    private async void InitializeUnityAuthentication()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            InitializationOptions initializationOptions = new InitializationOptions();

            initializationOptions.SetProfile(Random.Range(0, 99999).ToString());
            await UnityServices.InitializeAsync(initializationOptions);


            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        else
        {
            Debug.Log("Already Initialized");
        }
    }
}