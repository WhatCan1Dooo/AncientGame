using System;
using System.Collections;
using Pancake.Monetization;
using UnityEngine;

namespace Pancake.SceneFlow
{
    public class AdStatusComponent : GameComponent
    {
        [SerializeField] private RewardVariable rewardAd;
        [SerializeField] private GameObject fetch;

        private readonly WaitForSeconds _wait = new WaitForSeconds(0.1f);
        private IEnumerator _enumerator;

        protected override void OnEnabled()
        {
            if (!Application.isMobilePlatform)
            {
                fetch.SetActive(false);
                return;
            }
            _enumerator = IeValidate();
            App.StartCoroutine(_enumerator);
        }

        protected override void OnDisabled()
        {
#if UNITY_EDITOR
            try
            {
#endif
                if (_enumerator != null) App.StopCoroutine(_enumerator);
#if UNITY_EDITOR
            }
            catch (Exception)
            {
                // ignored
            }
#endif
        }

        private IEnumerator IeValidate()
        {
            while (true)
            {
                fetch.SetActive(!rewardAd.Context().IsReady());
                yield return _wait;
            }
            // ReSharper disable once IteratorNeverReturns
        }
    }
}