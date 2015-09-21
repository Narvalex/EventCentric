namespace EventCentric.Transport
{
    public interface IHttpLongPoller
    {
        /// <summary>
        /// Poll with the long polling mechanism, almost like the stream is pushing new events to us.
        /// </summary>
        /// <remarks>
        /// More info: http://www.pubnub.com/blog/http-long-polling/ 
        /// </remarks>
        void PollSubscription(string streamType, string url, string token, int lastReceivedVersion);
    }
}
