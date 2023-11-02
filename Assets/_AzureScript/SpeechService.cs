using UnityEngine;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Unity.VisualScripting;

public class SpeechService : MonoBehaviour
{
    SpeechConfig config;
    SpeechRecognizer recognizer;
    SpeechSynthesizer synthesizer;
    public AzureOpenAIController gpt;
    private object threadLocker = new object();
    private string message;
    private bool recongnizable = true;
    private bool killed;
    public void Start()
    {
        // Auth: Auzre Speech API key and region
        config = SpeechConfig.FromSubscription("87df75af6be3479a80484a90b0404966", "eastus");
        // 'zh-CN': Chinese; 'en-US': English
        //diction:'zh-CN-XiaoxiaoNeural' 'en-US-AriaNeural'
        // here is Azure speech language list: https://learn.microsoft.com/en-us/azure/ai-services/speech-service/language-support?tabs=tts
        config.SpeechSynthesisLanguage = "en-US";
        config.SpeechSynthesisVoiceName = "en-US-AriaNeural";
        config.SpeechRecognitionLanguage = "en-US";

        // Create speech recongnizer
        recognizer = new SpeechRecognizer(config);
        // subscribution: callback RecognizedHandler() when recognizer start to recognize
        recognizer.Recognized += RecognizedHandler;
        // subscribution: callback StartToThink() when recognizer start to process recognizing
        recognizer.Recognizing += AvatarAnimite;

        // Create speech synthesizer
        synthesizer = new SpeechSynthesizer(config);
        synthesizer.SynthesisStarted += StopRecord;
        synthesizer.SynthesisCompleted += RestartRecord;

        Debug.Log("Speech sdk inited");
        string[] aaa = Microphone.devices;
        OpenMic();
    }

    public async void OpenMic()
    {
        using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false); // this will start the listening when you click the button, if it's already off
        lock (threadLocker)
        {
            Debug.Log("Start recording");
        }
    }

    private void RecognizedHandler(object sender, SpeechRecognitionEventArgs e)
    {
        if (recongnizable)
        {
            Debug.Log("Start to speech to text");
            lock (threadLocker)
            {
                switch (e.Result.Reason)
                {
                    case ResultReason.RecognizedSpeech:
                        Debug.Log($"RECOGNIZED: Text={e.Result.Text}");
                        break;
                    case ResultReason.NoMatch:
                        Debug.Log($"NOMATCH: Speech could not be recognized.");
                        break;
                    case ResultReason.Canceled:
                        var cancellation = CancellationDetails.FromResult(e.Result);
                        Debug.Log($"CANCELED: Reason={cancellation.Reason}");

                        if (cancellation.Reason == CancellationReason.Error)
                        {
                            Debug.Log($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                            Debug.Log($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                            Debug.Log($"CANCELED: Did you set the speech resource key and region values?");
                        }
                        break;
                }
                message = e.Result.Text;
                if (message != "")
                {
                    Debug.Log(message);
                    gpt.UserInput(message);
                }
                recongnizable = false;

            }

        }
    }

    private void AvatarAnimite(object sender, SpeechRecognitionEventArgs e)
    {
        
    }
    // stop record user speech
    private async void StopRecord(object sender, SpeechSynthesisEventArgs e)
    {
        
        Debug.Log("Stop recording");
        await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false); // this will start the listening when you click the button, if it's already off
        lock (threadLocker)
        {
           // Todo, set some bool flag states

        }
    }

    // start to record user speech
    private async void RestartRecord(object sender, SpeechSynthesisEventArgs e)
    {
        recongnizable = true;
        if (!killed)
        {
            Debug.Log("Avatar stops talking and recording starts");
            // this will start the listening when you click the button, if it's already off
            await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
            lock (threadLocker)
            {
                // Todo, set some bool flag states

            }
        }
    }

    public async void KillRecord()
    {
        Debug.Log("Kill record");
        await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
        Debug.Log("Kill Avatar speaking");
        await synthesizer.StopSpeakingAsync();
    }

    // text to speech
    // avatar is talking
    public async void SynthesizeAudioAsync(string text)
    {
        Debug.Log("Start text to speech, and avatar starts to speak");
        await synthesizer.SpeakTextAsync(text);
    }

    void OnDestroy()
    {
        killed = true;
        KillRecord();
    }
}