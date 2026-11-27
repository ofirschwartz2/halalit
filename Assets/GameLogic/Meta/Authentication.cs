using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class Authentication : MonoBehaviour
{

    public static Task InitializationTask { get; private set; }

    // Initialize runs on every menu load (#176), so SetupEvents adds its handlers only once
    private static bool _areEventsSetUp;

    internal static async Task Initialize()
    {
        try
        {
            await UnityServices.InitializeAsync();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        SetupEvents();

        await SignInAnonymously();
    }

    // Setup authentication event handlers if desired
    static void SetupEvents()
    {
        if (_areEventsSetUp)
        {
            return;
        }

        AuthenticationService.Instance.SignedIn += () =>
        {
            // Shows how to get a playerID
            PlayerStats._playerId = AuthenticationService.Instance.PlayerId;
            Debug.Log("Player signed in.");
        };

        AuthenticationService.Instance.SignInFailed += (err) =>
        {
            Debug.LogError(err);
        };

        AuthenticationService.Instance.SignedOut += () =>
        {
            Debug.Log("Player signed out.");
        };

        AuthenticationService.Instance.Expired += () =>
        {
            Debug.Log("Player session could not be refreshed and expired.");
        };

        // Set last, so a set-up that throws is tried again on the next load
        _areEventsSetUp = true;
    }

    private static async Task SignInAnonymously()
    {
        try
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public static string GetPlayerId()
    {
        return AuthenticationService.Instance.PlayerId;
    }

    // Returns the name Unity stored, which is expected to end in a tag such as "#1234" (unconfirmed)
    public static async Task<string> UpdatePlayerNameAsync(string playerName)
    {
        return await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);
    }

}