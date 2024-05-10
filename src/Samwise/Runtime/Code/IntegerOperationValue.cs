// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public class AddValue : IIntegerValue, IBinaryOperationIntegerValue
    {
        public IIntegerValue A { get; set; }
        public IIntegerValue B { get; set; }

        public AddValue()
        {
            A = null;
            B = null;
        }

        public AddValue(IIntegerValue a, IIntegerValue b)
        {
            A = a;
            B = b;
        }
        
        public long EvaluateInteger(IDialogueContext context) 
        {
            return A.EvaluateInteger(context) + B.EvaluateInteger(context);
        }

        public void Traverse(System.Action<IValue> action)
        {
            action.Invoke(this);
            A.Traverse(action);
            B.Traverse(action);
        }

        public IBinaryOperationValue Overload(IValue a, IValue b)
        {
            A = a as IIntegerValue;
            B = b as IIntegerValue;
            return (A != null && B != null) ? this : null;
        }

        public override string ToString()
        {
            return A.ToString() + " + " + B.ToString();
        }
    }
    
    public class SubValue : IIntegerValue, IBinaryOperationIntegerValue
    {
        public IIntegerValue A { get; set; }
        public IIntegerValue B { get; set; }

        public SubValue()
        {
            A = null;
            B = null;
        }

        public SubValue(IIntegerValue a, IIntegerValue b)
        {
            A = a;
            B = b;
        }
        
        public long EvaluateInteger(IDialogueContext context) 
        {
            return A.EvaluateInteger(context) - B.EvaluateInteger(context);
        }

        public void Traverse(System.Action<IValue> action)
        {
            action.Invoke(this);
            A.Traverse(action);
            B.Traverse(action);
        }

        public IBinaryOperationValue Overload(IValue a, IValue b)
        {
            A = a as IIntegerValue;
            B = b as IIntegerValue;
            return (A != null && B != null) ? this : null;
        }

        public override string ToString()
        {
            return A.ToString() + " - " + B.ToString();
        }
    }

    public class MulValue : IIntegerValue, IBinaryOperationIntegerValue
    {
        public IIntegerValue A { get; set; }
        public IIntegerValue B { get; set; }

        public MulValue()
        {
            A = null;
            B = null;
        }

        public MulValue(IIntegerValue a, IIntegerValue b)
        {
            A = a;
            B = b;
        }
        
        public long EvaluateInteger(IDialogueContext context) 
        {
            return A.EvaluateInteger(context) * B.EvaluateInteger(context);
        }

        public void Traverse(System.Action<IValue> action)
        {
            action.Invoke(this);
            A.Traverse(action);
            B.Traverse(action);
        }

        public IBinaryOperationValue Overload(IValue a, IValue b)
        {
            A = a as IIntegerValue;
            B = b as IIntegerValue;
            return (A != null && B != null) ? this : null;
        }

        public override string ToString()
        {
            return A.ToString() + " * " + B.ToString();
        }
    }

    public class DivValue : IIntegerValue, IBinaryOperationIntegerValue
    {
        public IIntegerValue A { get; set; }
        public IIntegerValue B { get; set; }

        public DivValue()
        {
            A = null;
            B = null;
        }

        public DivValue(IIntegerValue a, IIntegerValue b)
        {
            A = a;
            B = b;
        }
        
        public long EvaluateInteger(IDialogueContext context) 
        {
            return A.EvaluateInteger(context) / B.EvaluateInteger(context);
        }

        public void Traverse(System.Action<IValue> action)
        {
            action.Invoke(this);
            A.Traverse(action);
            B.Traverse(action);
        }

        public IBinaryOperationValue Overload(IValue a, IValue b)
        {
            A = a as IIntegerValue;
            B = b as IIntegerValue;
            return (A != null && B != null) ? this : null;
        }

        public override string ToString()
        {
            return A.ToString() + " / " + B.ToString();
        }
    }
}