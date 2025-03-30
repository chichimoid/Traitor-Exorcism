using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class ServiceStarter : MonoBehaviour
{
    private async void Start()
    {
        // Was too lazy to create a separate loading scene.
        // So this is in MainMenu for now, with this silly if statement
        if (UnityServices.State == ServicesInitializationState.Initialized) return;
        await UnityServices.InitializeAsync();
            
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }
}