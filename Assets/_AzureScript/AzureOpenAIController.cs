using Azure;
using Azure.AI.OpenAI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using TMPro;
using UnityEngine;

public class AzureOpenAIController : MonoBehaviour
{
    public static Action<string, int> OnDeviceTargeted;
    public string key;
    public string deployment_name;
    public string endpoint;
    public TMP_InputField inputField;
    public SpeechService speechService;

    private OpenAIClient chatClient;
    private OpenAIClient funcCallclient;
    private IList<ChatMessage> messages;
    private IList<ChatMessage> funcMessages;

    [Serializable]
    public class FunctionCallResponse
    {
        public string name;
        public Arguments arguments;
    }

    [Serializable]
    public class Arguments
    {
        public string name;
        public string number;
    }

    // Start is called before the first frame update
    void Start()
    {
        string systemPrompt = @"You are a tour guide inside the Lab2041,
        and your responsibility is to help the user understand the lab and its various pieces of equipment, their are
            -1. 3D Projection fan
            -2. workbench
            -3. MaxHub smart screen
            -4. VR glasses
            -5. 3D printer
            -6. Omniverse workstation
            -7. robot
        While introducing the laboratory equipment, politely inquire whether the user wants to start the introduction.
        You need to introduce them one by one, Each introduction only introduces one piece of equipment. If the user interrupts the conversation, 
        you can politely respond, but you also have to remember the equipment you were supposed to introduce before the interruption, 
        as that is your primary duty.Each statement you make should not exceed 100 words.";

        string FuncPrompt = @"You are a tour guide inside the Lab2041,
        and your responsibility is to help the user understand the lab and its various pieces of equipment, their are
            -1. 3D Projection fan
            -2. workbench
            -3. MaxHub smart screen
            -4. VR glasses
            -5. 3D printer
            -6. Omniverse workstation
            -7. robot
        Don't make assumptions about what values to plug into functions. After understanding the whole paragraph, plug values into functions";
        // Create chatgpt client
        chatClient = new(new Uri(endpoint), new AzureKeyCredential(key));
        funcCallclient = new(new Uri(endpoint), new AzureKeyCredential(key));
        // Build request
        messages = new List<ChatMessage> { new ChatMessage(ChatRole.System, systemPrompt) };

        funcMessages = new List<ChatMessage> { new ChatMessage(ChatRole.System, FuncPrompt) };
    }

    private async void CallGPT()
    {
        Debug.Log("GPT starts");

        var chatCompletionsOptions = new ChatCompletionsOptions(messages)
        {
            MaxTokens = 4096,
        };
        try
        {
            Response<ChatCompletions> response = await chatClient.GetChatCompletionsAsync(deployment_name, chatCompletionsOptions);
            if (response.Value.Choices[0].Message.Content != null)
            {
                string res_str = response.Value.Choices[0].Message.Content;
                AddCharacterResToMessage(res_str);
                speechService.SynthesizeAudioAsync(res_str);
                FuncCall(res_str);

            }
        }
        catch (ArgumentNullException ex)
        {
            Debug.LogError(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            Debug.LogError(ex.Message);
        }
        catch (HttpRequestException ex)
        {
            Debug.LogError(ex.Message);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }

    }

    private async void FuncCall(string assistantWord)
    {
        funcMessages.Add(new ChatMessage(ChatRole.Assistant, assistantWord));
        Debug.Log("Trying functioncall: "+ assistantWord);
        string param = @"
        {
            ""type"": ""object"",
            ""properties"": {
                ""name"": {
                    ""type"": ""string"",
                    ""description"": ""The name of the last device introduced""
                },
                ""number"": {
                    ""type"": ""string"",
                    ""description"": ""The number of the device.""
                }
            },
            ""required"": [""name"", ""number""]
        }";

        var chatCompletionsOption = new ChatCompletionsOptions(funcMessages)
        {
            Temperature = 0,
            MaxTokens = 4096,
            Functions =
            {
                new FunctionDefinition
                {
                    Name = "get_device",
                    Description = "What was the last device introduced?",
                    Parameters = new BinaryData(param)
                }
            }
        };
        try
        {
            Response<ChatCompletions> funcResponse = await funcCallclient.GetChatCompletionsAsync(deployment_name, chatCompletionsOption);
            
            if (funcResponse.Value.Choices[0].Message.FunctionCall != null)
            {
                
                string res_str = funcResponse.Value.Choices[0].Message.FunctionCall.Arguments;
                Arguments args = JsonUtility.FromJson<Arguments>(res_str);
                string func_name = funcResponse.Value.Choices[0].Message.FunctionCall.Name;
                string name = args.name;
                string number = args.number;
                Debug.Log("---------------------func_name-----------------: " + func_name);
                Debug.Log("name: " + name);
                Debug.Log("number: " + number);
                OnDeviceTargeted?.Invoke(name, int.Parse(number));
            }
            if (funcResponse.Value.Choices[0].Message.Content != null)
            {
                string res_str = funcResponse.Value.Choices[0].Message.Content;
                Debug.Log("---------------------it not a content can invoke function call-----------------: " + res_str);
            }
            funcMessages.RemoveAt(1);
            Debug.Log("the numbers of messages in funcMessage: " + funcMessages.Count);
        }
        catch (ArgumentNullException ex)
        {
            Debug.LogError(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            Debug.LogError(ex.Message);
        }
        catch (HttpRequestException ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    IEnumerator Call() {
        yield return new WaitForSeconds(1);
        Debug.Log("djasjdkalsjdakdwij");
    }

    public void AddCharacterResToMessage(string content)
    {
        var characterMessage = new ChatMessage(ChatRole.Assistant, content);
        messages.Add(characterMessage);
        Debug.Log("Avatar's message added: " + content);
    }

    public void UserInput()
    {
        messages.Add(new ChatMessage(ChatRole.User, inputField.text));
        Debug.Log("User input: " + inputField.text);
        CallGPT();
    }

    public void UserInput(string speechtext)
    {
        messages.Add(new ChatMessage(ChatRole.User, speechtext));
        Debug.Log("User says: " + speechtext);
        CallGPT();
    }

    public void AssistantInput(string speechtext)
    {
        messages.Add(new ChatMessage(ChatRole.Assistant, speechtext));
        Debug.Log("Assistant says: " + speechtext);
    }
}
