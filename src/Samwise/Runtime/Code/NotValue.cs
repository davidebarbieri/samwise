// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public class NotValue : IBoolValue, IUnaryOperationBoolValue
    {
        public IBoolValue A { get; set; }

        public NotValue()
        {
            A = null;
        }

        public NotValue(IBoolValue a)
        {
            A = a;
        }
        
        public bool EvaluateBool(IDialogueContext context) 
        {
            return !A.EvaluateBool(context);
        }

        public void OnVisited(IDialogueContext context) 
        {
            A.OnVisited(context);
        }

        public void Traverse(System.Action<IValue> action)
        {
            action.Invoke(this);
            A.Traverse(action);
        }

        public override string ToString()
        {
            return "!" + A.ToString();
        }
    }
}
