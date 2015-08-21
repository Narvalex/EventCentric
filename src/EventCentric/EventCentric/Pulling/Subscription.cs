namespace EventCentric.Pulling
{
    public abstract class Subscription
    {
        public Subscription()
        {
            this.IsBusy = false;
        }

        public bool IsBusy { get; private set; }

        public void EnterBusy()
        {
            this.IsBusy = true;
        }

        public void ExitBusy()
        {
            this.IsBusy = false;
        }
    }
}
