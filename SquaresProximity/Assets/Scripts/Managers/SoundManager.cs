namespace Managers
{
    using System;
    using UnityEngine;

    public class SoundManager : MonoBehaviour
    {
        [SerializeField] private AudioClip coinPlacedClip;
        [SerializeField] private AudioSource audioSource;

        private void Start()
        {
            ToggleEventSubscription(true);
        }

        private void OnDestroy()
        {
            ToggleEventSubscription(false);
        }

        private void OnCoinPlaced(int a , int b)
        {
            audioSource.clip = coinPlacedClip;
            audioSource.Play();
        }
        
        private void ToggleEventSubscription(bool shouldSubscribe)
        {
            if(shouldSubscribe)
            {
                EventsManager.SubscribeToEvent(Event.CoinPlaced , (Action<int , int>)OnCoinPlaced);
            }
            else
            {
                EventsManager.UnsubscribeFromEvent(Event.CoinPlaced , (Action<int , int>)OnCoinPlaced);
            }
        }
    }
}