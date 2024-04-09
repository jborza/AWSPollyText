using Amazon.Polly;
using Amazon.Polly.Model;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class TextToSpeech : MonoBehaviour
{
    [SerializeField]
    private AudioSource audioSource;

    // Start is called before the first frame update
    async void Start()
    {
        var accessKey = Environment.GetEnvironmentVariable("AWSAccessKey");
        var secretKey = Environment.GetEnvironmentVariable("AWSSecretAccessKey");
        var credentials = new Amazon.Runtime.BasicAWSCredentials(accessKey, secretKey);
        var client = new AmazonPollyClient(credentials, Amazon.RegionEndpoint.EUSouth1);
        var request = new SynthesizeSpeechRequest
        {
            Text = "Testing Amazon Polly, in Unity",
            OutputFormat = OutputFormat.Mp3,
            VoiceId = VoiceId.Joanna,
            Engine = Engine.Neural
        };

        var response = await client.SynthesizeSpeechAsync(request);
        // write the result to a file 
        WriteIntoFile(response.AudioStream);

        using (var www = UnityWebRequestMultimedia.GetAudioClip(GetFilePath(), AudioType.MPEG))
        {
            var op = www.SendWebRequest();
            while (!op.isDone)
            {
                await Task.Yield();
            }
            var clip = DownloadHandlerAudioClip.GetContent(www);
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    string GetFilePath()
    {
        return $"{Application.persistentDataPath}/audio.mp3";
    }

    private void WriteIntoFile(Stream stream)
    {
        using (var fs = new FileStream(GetFilePath(), FileMode.Create))
        {
            byte[] buffer = new byte[8 * 1024];
            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                fs.Write(buffer, 0, bytesRead);
            }
        }
    }
}
