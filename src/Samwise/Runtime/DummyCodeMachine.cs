namespace Peevo.Samwise
{
    public class DummyExternalCodeMachine : IExternalCodeMachine
    {
        public void Dispatch(IExternalCodeContext context, IAsyncCode asyncCode)
        {
            context.OnEnd();
        }
    }
}

