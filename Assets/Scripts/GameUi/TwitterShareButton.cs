using UnityEngine.Networking;
using UnityEngine;

public class TwitterShareButton : MonoBehaviour
{
    public string IPlayRektlessText, IAmRektlessText, BornRektlessText;
    public string IPlayRektlessGif, IAmRektlessGif, BornRektlessGif;
    public string RektlessURL;

    public void ShareToTwitter()
    {
        string gifURL = IPlayRektlessGif;
        string tweetText = IPlayRektlessText;
        if (Player.Instance.DistanceStatistic > 500)
        {
            tweetText = BornRektlessText;
            gifURL = BornRektlessGif; 
        }
        if (Player.Instance.DistanceStatistic > 1000)
        {
            tweetText = IAmRektlessText;
            gifURL = IAmRektlessGif;
        }

        //string twitterURL = "https://twitter.com/intent/tweet";
        //string fullURL = $"{twitterURL}?text={UnityWebRequest.EscapeURL(tweetText)}&url={UnityWebRequest.EscapeURL(gifURL)}";

        string twitterUrl = "https://twitter.com/intent/tweet?text=" + UnityWebRequest.EscapeURL(tweetText) +
                            "&url=" + UnityWebRequest.EscapeURL(gifURL) +
                            "%20" + UnityWebRequest.EscapeURL(RektlessURL); // Add the website link to the tweet


        // Open the URL in the browser
        Application.OpenURL(twitterUrl);
    }
}