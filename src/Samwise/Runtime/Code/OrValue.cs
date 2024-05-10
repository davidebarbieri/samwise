// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public class OrCondition : IBoolValue, IBinaryOperationBoolValue
    {
        public IBoolValue A { get; set; }
        public IBoolValue B { get; set; }

        public OrCondition()
        {
            A = null;
            B = null;
        }

        public OrCondition(IBoolValue a, IBoolValue b)
        {
            A = a;
            B = b;
        }
        
        public bool EvaluateBool(IDialogueContext context) 
        {
            return A.EvaluateBool(context) || B.EvaluateBool(context);
        }

        public IBinaryOperationValue Overload(IValue a, IValue b)
        {
            A = a as IBoolValue;
            B = b as IBoolValue;
            return (A != null && B != null) ? this : null;
        }


        public void OnVisited(IDialogueContext context) 
        {
            A.OnVisited(context);
            B.OnVisited(context);
        }

        public void Traverse(System.Action<IValue> action)
        {
            action.Invoke(this);
            A.Traverse(action);
            B.Traverse(action);
        }

        public override string ToString()
        {
            return "(" + A.ToString() + " | " + B.ToString() + ")";
        }
    }
}