// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public class NegateValue : IIntegerValue, IUnaryOperationIntegerValue
    {
        public IIntegerValue A { get; set; }

        public NegateValue()
        {
            A = null;
        }

        public NegateValue(IIntegerValue a)
        {
            A = a;
        }
        
        public long EvaluateInteger(IDialogueContext context) 
        {
            return -A.EvaluateInteger(context);
        }

        public void Traverse(System.Action<IValue> action)
        {
            action.Invoke(this);
            A.Traverse(action);
        }

        public override string ToString()
        {
            return "-" + A.ToString();
        }
    }
}
