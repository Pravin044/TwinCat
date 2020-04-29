namespace TwinCAT.Ads
{
    internal interface IInterceptionFactory
    {
        ICommunicationInterceptor CreateInterceptor();
    }
}

