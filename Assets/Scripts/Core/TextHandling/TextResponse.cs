using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using Zenject;

//  ласс л€ отображени€ текстовых сообщений с анимацией печати и автоматическим скрытием после просто€. “акже предусмотрена локализаци€.
public class TextResponse : MonoBehaviour
{
    [SerializeField] private LocalizeStringEvent localizedString;
    [SerializeField] private TextMeshProUGUI textWithMessage;
    [SerializeField] private float charDelay = 0.05f;
    [SerializeField] private float idleHideDelay = 5f;
    private GameEventsHandler _gameEvent;
    private CompositeDisposable _disposables = new CompositeDisposable();
    private CancellationTokenSource _cancellationTokenSource;
    private CancellationTokenSource _cancellationTokenSourceIdle;

    [Inject]
    public void Construct(GameEventsHandler gameEvent)
    {
        _gameEvent = gameEvent;
        textWithMessage.raycastTarget = false;

        _gameEvent
            .textEvent
            .Subscribe(_ => OnGameTypeChanged(_))
            .AddTo(_disposables);
    }

    private void Awake()
    {
        var locale = LocalizationSettings.AvailableLocales.GetLocale(Application.systemLanguage);
        LocalizationSettings.SelectedLocale =
            locale ?? LocalizationSettings.AvailableLocales.Locales[0];
    }

    private void OnGameTypeChanged(GameEventType type)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new();

        _cancellationTokenSourceIdle?.Cancel();
        _cancellationTokenSourceIdle = new();

        Debug.Log(type.ToString());
        localizedString.StringReference.TableEntryReference = type.ToString();

        _ = PlayTextAnim(localizedString.StringReference.GetLocalizedString(), _cancellationTokenSource.Token);

        _ = HideTextAfterIdle(idleHideDelay, _cancellationTokenSourceIdle.Token);
    }

    private async Task PlayTextAnim(string message, CancellationToken token)
    {
        textWithMessage.text = "";
        foreach (var c in message)
        {
            if (token.IsCancellationRequested)
                return;

            textWithMessage.text += c;
            await Task.Delay(TimeSpan.FromSeconds(charDelay));
        }
    }

    private async Task HideTextAfterIdle(float delaySeconds, CancellationToken token)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), token);
            textWithMessage.text = "";
        }
        catch (TaskCanceledException)
        {
        }
    }

    private void OnDestroy()
    {
        _cancellationTokenSource?.Cancel();
        _disposables.Dispose();
    }
}
