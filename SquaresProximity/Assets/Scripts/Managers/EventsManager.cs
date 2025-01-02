namespace Managers
{
    using System;
    using System.Collections.Generic;

    public enum Event
    {
        AIHumanToggled,
        CoinBuffedUp,
        CoinCaptured,
        CoinPlaced,
        GameDataReset,
        GameOver,
        JoystickXPressed,
        GamePaused,
        GameRestarted,
        GameResumed,
        GameStarted,
        GameStartedMultiplayer,
        GameTied,
        HolesToggled,
        JoystickDownPressed,
        JoystickLeftPressed,
        JoystickRightPressed,
        JoystickUpPressed,
        LobbyJoinButtonPressed,
        LobbyPlayersListUpdated,
        KeyboardTabPressed,
        MouseLeftClicked,
        MouseMoved,
        NumberOfPlayersSelected,
        NumberOfPlayersToggled,
        PlayerJoinedRoom,
        PlayerNamesUpdated,
        PlayerNowOnline,
        PlayerTotalReceived,
        PlayerWins,
        RandomTurnsToggled,
        ScoreUpdated,
        TouchscreenTapped
    }

    public static class EventsManager
    {
        private static Dictionary<Event , Delegate> _eventSubscriptions = new ();

        public static void SubscribeToEvent(Event gameEvent , Delegate actionDelegate)
        {
            AddDelegateToEvent(gameEvent , actionDelegate);
        }

        private static void AddDelegateToEvent(Event gameEvent , Delegate actionDelegate)
        {
            if(_eventSubscriptions.ContainsKey(gameEvent))
            {
                _eventSubscriptions[gameEvent] = Delegate.Combine(_eventSubscriptions[gameEvent] , actionDelegate);
            }
            else
            {
                _eventSubscriptions.Add(gameEvent , actionDelegate);
            }
        }

        public static void UnsubscribeFromEvent(Event gameEvent , Delegate actionDelegate)
        {
            RemoveDelegateFromEvent(gameEvent , actionDelegate);
        }

        private static void RemoveDelegateFromEvent(Event gameEvent , Delegate actionDelegate)
        {
            if(_eventSubscriptions.ContainsKey(gameEvent))
            {
                _eventSubscriptions[gameEvent] = Delegate.Remove(_eventSubscriptions[gameEvent] , actionDelegate);
            }
        }

        public static void Invoke(Event gameEvent)
        {
            if(_eventSubscriptions.TryGetValue(gameEvent , out var subscription))
            {
                var delegateList = subscription as Action;
                delegateList?.Invoke();
            }
        }

        public static void Invoke<T>(Event gameEvent , T value)
        {
            if(_eventSubscriptions.TryGetValue(gameEvent , out var subscription))
            {
                var delegateList = subscription as Action<T>;
                delegateList?.Invoke(value);
            }
        }

        public static void Invoke<T1 , T2>(Event gameEvent , T1 arg1 , T2 arg2)
        {
            if(_eventSubscriptions.TryGetValue(gameEvent , out var subscription))
            {
                var delegateList = subscription as Action<T1 , T2>;
                delegateList?.Invoke(arg1 , arg2);
            }
        }
        
        public static void Invoke<T1 , T2 , T3>(Event gameEvent , T1 arg1 , T2 arg2 , T3 arg3)
        {
            if(_eventSubscriptions.TryGetValue(gameEvent , out var subscription))
            {
                var delegateList = subscription as Action<T1 , T2 , T3>;
                delegateList?.Invoke(arg1 , arg2 , arg3);
            }
        }
    }
}