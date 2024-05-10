namespace Peevo.Samwise
{
    public class DummyCodeParser: IExternalCodeParser
    {
        public bool Parse(string code, out IStatement statement, System.Action<string> logError)
        {
            statement = new DummyStatement(code);
            return true;
        }

        public bool ParseCondition(string code, out IBoolValue expression, System.Action<string> logError)
        {
            expression = new DummyBoolValue(code);
            return true;
        }

        public bool ParseAsync(string code, out IAsyncCode asyncCode, System.Action<string> logError)
        {
            asyncCode = new DummyAsyncCode(code);
            return true;
        }
    }

    public class DummyStatement : IStatement
    {
        public string Code { get; private set; }
        public DummyStatement(string code) { Code = code; }

        public void Execute(IDialogueContext context)
        {

        }

        public override string ToString()
        {
            return Code;
        }
    }

    public class DummyBoolValue : IBoolValue
    {
        public string Code { get; private set; }
        public DummyBoolValue(string code) { Code = code; }

        public bool EvaluateBool(IDialogueContext context)
        {
            return true;
        }

        public override string ToString()
        {
            return Code;
        }

        public void OnVisited(IDialogueContext context) {}
        public void Traverse(System.Action<IValue> action) { action.Invoke(this); }
    }

    public class DummyAsyncCode : IAsyncCode 
    {
        public string Code { get; private set; }
        public DummyAsyncCode(string code) { Code = code; }

        public override string ToString()
        {
            return Code;
        }
    }
}