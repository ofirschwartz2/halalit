using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class Authentication : MonoBehaviour
{

    // Initialize runs once per load of the menu or the leaderboard (#176), so SetupEvents adds its handlers only once
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
            Debug.Log("Player signed in.");
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
            // A warning, not an error: being offline is expected
            Debug.LogWarning($"Authentication: could not sign in. {e.Message}");
        }
    }

    // Returns the name Unity stored, which is expected to end in a tag such as "#1234" (unconfirmed)
    public static async Task<string> UpdatePlayerNameAsync(string playerName)
    {
        return await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);
    }

}