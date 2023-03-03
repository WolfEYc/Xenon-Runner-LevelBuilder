#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration.API
{
#if STEAMWORKSNET
    /// <summary>
    /// Handles Steam's Authentication interface for both Client and Server interfaces
    /// </summary>
    public static class Authentication
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            ActiveTickets = new List<AuthenticationTicket>();
            ActiveSessions = new List<AuthenticationSession>();
            m_GetAuthSessionTicketResponce = null;
            m_GetAuthSessionTicketResponceServer = null;
            m_ValidateAuthSessionTicketResponce = null;
            m_ValidateAuthSessionTicketResponceServer = null;
        }

        /// <summary>
        /// Tickets this player has sent out
        /// </summary>
        public static List<AuthenticationTicket> ActiveTickets = new List<AuthenticationTicket>();
        /// <summary>
        /// Sessions this player has started
        /// </summary>
        public static List<AuthenticationSession> ActiveSessions = new List<AuthenticationSession>();

#pragma warning disable IDE0052 // Remove unread private members
        private static Callback<GetAuthSessionTicketResponse_t> m_GetAuthSessionTicketResponce;
        private static Callback<GetAuthSessionTicketResponse_t> m_GetAuthSessionTicketResponceServer;
        private static Callback<ValidateAuthTicketResponse_t> m_ValidateAuthSessionTicketResponce;
        private static Callback<ValidateAuthTicketResponse_t> m_ValidateAuthSessionTicketResponceServer;
#pragma warning restore IDE0052 // Remove unread private members

        /// <summary>
        /// Determins if the provided ticket handle is valid
        /// </summary>
        /// <param name="ticket">the ticket to test for validity</param>
        /// <returns></returns>
        public static bool IsAuthTicketValid(AuthenticationTicket ticket)
        {
            if (ticket.Handle == default || ticket.Handle == HAuthTicket.Invalid)
                return false;
            else
                return true;
        }

        /// <summary>
        /// <para>Encodes a ticekt to hex string format</para>
        /// This is most commonly used with web calls such as <a href="https://partner.steamgames.com/doc/webapi/ISteamUserAuth#AuthenticateUserTicket">https://partner.steamgames.com/doc/webapi/ISteamUserAuth#AuthenticateUserTicket</a>
        /// </summary>
        /// <param name="ticket">The ticket to be encoded</param>
        /// <returns>Returns the hex encoded string representation of the ticket data array.</returns>
        public static string EncodedAuthTicket(AuthenticationTicket ticket)
        {
            if (!IsAuthTicketValid(ticket))
                return "";
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach (byte b in ticket.Data)
                    sb.AppendFormat("{0:X2}", b);

                return sb.ToString();
            }
        }

        /// <summary>
        /// Requests a new Auth Session Ticket
        /// </summary>
        /// <param name="callback">Invoked when the call completes, Action(Ticket result, bool IOFailure)</param>
        public static void GetAuthSessionTicket(Action<AuthenticationTicket, bool> callback)
        {
#if !UNITY_SERVER
            if (m_GetAuthSessionTicketResponce == null)
                m_GetAuthSessionTicketResponce = Callback<GetAuthSessionTicketResponse_t>.Create(HandleGetAuthSessionTicketResponce);

            var ticket = new AuthenticationTicket(callback, true);
            if (ActiveTickets == null)
                ActiveTickets = new List<AuthenticationTicket>();

            ActiveTickets.Add(ticket);
#else
            if (m_GetAuthSessionTicketResponceServer == null)
                m_GetAuthSessionTicketResponceServer = Callback<GetAuthSessionTicketResponse_t>.CreateGameServer(HandleGetAuthSessionTicketResponce);

            var ticket = new AuthenticationTicket(callback, false);

            if (ActiveTickets == null)
                ActiveTickets = new List<AuthenticationTicket>();

            ActiveTickets.Add(ticket);
#endif
        }

        /// <summary>
        /// Cancels the auth ticket rather its client or server based.
        /// </summary>
        /// <param name="ticket"></param>
        public static void CancelAuthTicket(AuthenticationTicket ticket)
        {
            ticket.Cancel();

            ActiveTickets.Remove(ticket);
        }

        /// <summary>
        /// Starts an authorization session with the indicated user given the applied auth ticket
        /// </summary>
        /// <param name="authTicket">The ticket data to validate</param>
        /// <param name="user">The user the session will relate to</param>
        /// <param name="callback">This will be invoked when the responce comes back and will contain the response state.</param>
        public static EBeginAuthSessionResult BeginAuthSession(byte[] authTicket, CSteamID user, Action<AuthenticationSession> callback)
        {
#if !UNITY_SERVER
            if (m_ValidateAuthSessionTicketResponce == null)
                m_ValidateAuthSessionTicketResponce = Callback<ValidateAuthTicketResponse_t>.Create(HandleValidateAuthTicketResponse);

            var session = new AuthenticationSession(user, callback, true);

            if (ActiveSessions == null)
            {
                ActiveSessions = new List<AuthenticationSession>();
            }
            ActiveSessions.Add(session);

            return SteamUser.BeginAuthSession(authTicket, authTicket.Length, user);
#else
            if (m_ValidateAuthSessionTicketResponceServer == null)
                m_ValidateAuthSessionTicketResponceServer = Callback<ValidateAuthTicketResponse_t>.Create(HandleValidateAuthTicketResponse);

            var session = new AuthenticationSession(user, callback, false);

            if (ActiveSessions == null)
            {
                ActiveSessions = new List<AuthenticationSession>();
            }
            ActiveSessions.Add(session);

            return SteamGameServer.BeginAuthSession(authTicket, authTicket.Length, user);
#endif
        }

        /// <summary>
        /// Ends the auth session with the indicated user if any
        /// </summary>
        /// <param name="user"></param>
        public static void EndAuthSession(CSteamID user)
        {
#if !UNITY_SERVER
            SteamUser.EndAuthSession(user);
#else
            SteamGameServer.EndAuthSession(user);
#endif

            ActiveSessions.RemoveAll(p => p.User == user);
        }
        /// <summary>
        /// Checks if the user owns a specific piece of Downloadable Content (DLC).
        /// </summary>
        /// <remarks>
        /// This can only be used after BeginAuthSession has been ran on the user's ticket
        /// </remarks>
        /// <param name="user">The authenticated user to check</param>
        /// <param name="appId">The app Id of the app to check for</param>
        /// <returns></returns>
        public static EUserHasLicenseForAppResult UserHasLicenseForApp(CSteamID user, AppId_t appId)
        {
#if !UNITY_SERVER
            return SteamUser.UserHasLicenseForApp(user, appId);
#else
            return SteamGameServer.UserHasLicenseForApp(user, appId);
#endif
        }

        private static void HandleGetAuthSessionTicketResponce(GetAuthSessionTicketResponse_t pCallback)
        {
            if (ActiveTickets != null && ActiveTickets.Any(p => p.Handle == pCallback.m_hAuthTicket))
            {
                var ticket = ActiveTickets.First(p => p.Handle == pCallback.m_hAuthTicket);
                ticket.Authenticate(pCallback);
            }
        }

        private static void HandleValidateAuthTicketResponse(ValidateAuthTicketResponse_t param)
        {
            if (ActiveSessions != null && ActiveSessions.Any(p => p.User == param.m_SteamID))
            {
                var session = ActiveSessions.First(p => p.User == param.m_SteamID);
                session.Authenticate(param);

                if (SteamSettings.current != null && SteamSettings.current.isDebugging)
                    Debug.Log("Processing session request data for " + param.m_SteamID.m_SteamID.ToString() + " status = " + param.m_eAuthSessionResponse);

                if (param.m_eAuthSessionResponse != EAuthSessionResponse.k_EAuthSessionResponseOK)
                    ActiveSessions.Remove(session);

                if (session.OnStartCallback != null)
                    session.OnStartCallback.Invoke(session);
            }
            else
            {
                if (SteamSettings.current != null && SteamSettings.current.isDebugging)
                    Debug.LogWarning("Recieved an authentication ticket responce for user " + param.m_SteamID.m_SteamID + " no matching session was found for this user.");
            }
        }

        /// <summary>
        /// Ends all tracked sessions
        /// </summary>
        public static void EndAllSessions()
        {
            foreach (var session in ActiveSessions)
                session.End();

            ActiveSessions.Clear();
        }

        /// <summary>
        /// Cancels all tracked tickets
        /// </summary>
        public static void CancelAllTickets()
        {
            foreach (var ticket in ActiveTickets)
                ticket.Cancel();

            ActiveTickets.Clear();
        }
    }
#elif FACEPUNCH
    public static class Authentication
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            ActiveTickets = new List<AuthenticationTicket>();
            ActiveSessions = new List<AuthenticationSession>();
        }

        /// <summary>
        /// Tickets this player has sent out
        /// </summary>
        public static List<AuthenticationTicket> ActiveTickets = new List<AuthenticationTicket>();
        /// <summary>
        /// Sessions this player has started
        /// </summary>
        public static List<AuthenticationSession> ActiveSessions = new List<AuthenticationSession>();

        /// <summary>
        /// Determins if the provided ticket handle is valid
        /// </summary>
        /// <param name="ticket">the ticket to test for validity</param>
        /// <returns></returns>
        public static bool IsAuthTicketValid(AuthenticationTicket ticket)
        {
            if (ticket.Handle == default || ticket.Handle == null)
                return false;
            else
                return true;
        }

        /// <summary>
        /// <para>Encodes a ticekt to hex string format</para>
        /// This is most commonly used with web calls such as <a href="https://partner.steamgames.com/doc/webapi/ISteamUserAuth#AuthenticateUserTicket">https://partner.steamgames.com/doc/webapi/ISteamUserAuth#AuthenticateUserTicket</a>
        /// </summary>
        /// <param name="ticket">The ticket to be encoded</param>
        /// <returns>Returns the hex encoded string representation of the ticket data array.</returns>
        public static string EncodedAuthTicket(AuthenticationTicket ticket)
        {
            if (!IsAuthTicketValid(ticket))
                return "";
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach (byte b in ticket.Data)
                    sb.AppendFormat("{0:X2}", b);

                return sb.ToString();
            }
        }

        /// <summary>
        /// Requests a new Auth Session Ticket
        /// </summary>
        /// <param name="callback">Invoked when the call completes, Action(Ticket result, bool IOFailure)</param>
        public static void GetAuthSessionTicket(Action<AuthenticationTicket, bool> callback)
        {
#if !UNITY_SERVER
            var ticket = new AuthenticationTicket(callback, true);
            if (ActiveTickets == null)
                ActiveTickets = new List<AuthenticationTicket>();

            ActiveTickets.Add(ticket);
            callback?.Invoke(ticket, false);
#else
            Debug.LogError("Facepunch does not support Steam Game Server getting Auth Session Tickets, if you require this feature please use Steamworks.NET instead of Facepunch for you API wrapper.");
            callback?.Invoke(null, true);
#endif
        }

        /// <summary>
        /// Cancels the auth ticket rather its client or server based.
        /// </summary>
        /// <param name="ticket"></param>
        public static void CancelAuthTicket(AuthenticationTicket ticket)
        {
            ticket.Cancel();

            ActiveTickets.Remove(ticket);
        }

        /// <summary>
        /// Starts an authorization session with the indicated user given the applied auth ticket
        /// </summary>
        /// <param name="authTicket">The ticket data to validate</param>
        /// <param name="user">The user the session will relate to</param>
        /// <param name="callback">This will be invoked when the responce comes back and will contain the response state.</param>
        public static BeginAuthResult BeginAuthSession(byte[] authTicket, SteamId user, Action<AuthenticationSession> callback)
        {
#if !UNITY_SERVER
            SteamUser.OnValidateAuthTicketResponse -= HandleValidateAuthTicketResponse;
            SteamUser.OnValidateAuthTicketResponse += HandleValidateAuthTicketResponse;

            var session = new AuthenticationSession(user, callback, true);

            if (ActiveSessions == null)
            {
                ActiveSessions = new List<AuthenticationSession>();
            }
            ActiveSessions.Add(session);

            return SteamUser.BeginAuthSession(authTicket, user);
#else
            SteamServer.OnValidateAuthTicketResponse -= HandleValidateAuthTicketResponse;
            SteamServer.OnValidateAuthTicketResponse += HandleValidateAuthTicketResponse;

            var session = new AuthenticationSession(user, callback, false);

            if (ActiveSessions == null)
            {
                ActiveSessions = new List<AuthenticationSession>();
            }
            ActiveSessions.Add(session);
            var r = SteamServer.BeginAuthSession(authTicket, user);
            return r ? BeginAuthResult.OK : BeginAuthResult.InvalidTicket;
#endif
        }

        /// <summary>
        /// Ends the auth session with the indicated user if any
        /// </summary>
        /// <param name="user"></param>
        public static void EndAuthSession(UserData user)
        {
#if !UNITY_SERVER
            SteamUser.EndAuthSession(user);
#else
            SteamServer.EndSession(user);
#endif

            ActiveSessions.RemoveAll(p => p.User == user);
        }
        /// <summary>
        /// Checks if the user owns a specific piece of Downloadable Content (DLC).
        /// </summary>
        /// <remarks>
        /// This can only be used after BeginAuthSession has been ran on the user's ticket
        /// </remarks>
        /// <param name="user">The authenticated user to check</param>
        /// <param name="appId">The app Id of the app to check for</param>
        /// <returns></returns>
        public static UserHasLicenseForAppResult UserHasLicenseForApp(UserData user, AppId appId)
        {
#if !UNITY_SERVER
            Debug.LogError("Facepunch does not support this funciton for client builds, if you require this please use Steamworks.NET as your API wrapper instead of Facepunch");
            return UserHasLicenseForAppResult.NoAuth;
#else
            return SteamServer.UserHasLicenseForApp(user, appId)
#endif
        }

        private static void HandleValidateAuthTicketResponse(SteamId user, SteamId owner, AuthResponse responce)
        {
            if (ActiveSessions != null && ActiveSessions.Any(p => p.User == user))
            {
                var session = ActiveSessions.First(p => p.User == user);
                session.Authenticate(user, owner, responce);

                if (SteamSettings.current != null && SteamSettings.current.isDebugging)
                    Debug.Log("Processing session request data for " + user.ToString() + " status = " + responce);

                if (responce != AuthResponse.OK)
                    ActiveSessions.Remove(session);

                if (session.OnStartCallback != null)
                    session.OnStartCallback.Invoke(session);
            }
            else
            {
                if (SteamSettings.current != null && SteamSettings.current.isDebugging)
                    Debug.LogWarning("Recieved an authentication ticket responce for user " + user + " no matching session was found for this user.");
            }
        }

        /// <summary>
        /// Ends all tracked sessions
        /// </summary>
        public static void EndAllSessions()
        {
            foreach (var session in ActiveSessions)
                session.End();

            ActiveSessions.Clear();
        }

        /// <summary>
        /// Cancels all tracked tickets
        /// </summary>
        public static void CancelAllTickets()
        {
            foreach (var ticket in ActiveTickets)
                ticket.Cancel();

            ActiveTickets.Clear();
        }
    }
#endif
}
#endif