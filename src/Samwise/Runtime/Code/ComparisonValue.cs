// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

using System;

namespace Peevo.Samwise
{

    public class EqualValue : IBoolValue, IBinaryOperationValue
    {
        public bool EvaluateBool(IDialogueContext context) 
        {
            throw new NotSupportedException("Trying to evaluate a non-compiled operation");
        }

        public void OnVisited(IDialogueContext context) {}

        public void Traverse(System.Action<IValue> action)
        {
            throw new NotSupportedException("Trying to traverse a non-compiled operation");
        }

        public IBinaryOperationValue Overload(IValue a, IValue b)
        {
            if (a is IIntegerValue && b is IIntegerValue)
                return new IntegerEqualValue((IIntegerValue)a, (IIntegerValue)b);

            if (a is IBoolValue && b is IBoolValue)
                return new BoolEqualValue((IBoolValue)a, (IBoolValue)b);

            if (a is ISymbolValue && b is ISymbolValue)
                return new SymbolEqualValue((ISymbolValue)a, (ISymbolValue)b);

            return null;
        }
    }
    public class DifferentValue : IBoolValue, IBinaryOperationValue
    {
        public bool EvaluateBool(IDialogueContext context) 
        {
            throw new NotSupportedException("Trying to evaluate a non-compiled operation");
        }

        public void OnVisited(IDialogueContext context) {}

        public void Traverse(System.Action<IValue> action)
        {
            throw new NotSupportedException("Trying to traverse a non-compiled operation");
        }

        public IBinaryOperationValue Overload(IValue a, IValue b)
        {
            if (a is IIntegerValue && b is IIntegerValue)
                return new IntegerDifferentValue((IIntegerValue)a, (IIntegerValue)b);

            if (a is IBoolValue && b is IBoolValue)
                return new BoolDifferentValue((IBoolValue)a, (IBoolValue)b);

            if (a is ISymbolValue && b is ISymbolValue)
                return new SymbolDifferentValue((ISymbolValue)a, (ISymbolValue)b);

            return null;
        }
    }

    public class IntegerEqualValue : IBoolValue, IBinaryOperationIntegerValue
    {
        public IIntegerValue A { get; set; }
        public IIntegerValue B { get; set; }

        public IntegerEqualValue(IIntegerValue a, IIntegerValue b)
        {
            A = a;
            B = b;
        }
        
        public bool EvaluateBool(IDialogueContext context) 
        {
            return A.EvaluateInteger(context) == B.EvaluateInteger(context);
        }

        public void OnVisited(IDialogueContext context) {}

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
            return "(" + A.ToString() + " == " + B.ToString() + ")";
        }
    }

    public class BoolEqualValue : IBoolValue, IBinaryOperationBoolValue
    {
        public IBoolValue A { get; set; }
        public IBoolValue B { get; set; }

        public BoolEqualValue(IBoolValue a, IBoolValue b)
        {
            A = a;
            B = b;
        }
        
        public bool EvaluateBool(IDialogueContext context) 
        {
            return A.EvaluateBool(context) == B.EvaluateBool(context);
        }

        public void OnVisited(IDialogueContext context) {}

        public void Traverse(System.Action<IValue> action)
        {
            action.Invoke(this);
            A.Traverse(action);
            B.Traverse(action);
        }

        public IBinaryOperationValue Overload(IValue a, IValue b)
        {
            A = a as IBoolValue;
            B = b as IBoolValue;
            return (A != null && B != null) ? this : null;
        }

        public override string ToString()
        {
            return "(" + A.ToString() + " == " + B.ToString() + ")";
        }
    }

    public class SymbolEqualValue : IBoolValue, IBinaryOperationSymbolValue
    {
        public ISymbolValue A { get; set; }
        public ISymbolValue B { get; set; }

        public SymbolEqualValue(ISymbolValue a, ISymbolValue b)
        {
            A = a;
            B = b;
        }
        
        public bool EvaluateBool(IDialogueContext context) 
        {
            return A.EvaluateSymbol(context) == B.EvaluateSymbol(context);
        }

        public void OnVisited(IDialogueContext context) {}

        public void Traverse(System.Action<IValue> action)
        {
            action.Invoke(this);
            A.Traverse(action);
            B.Traverse(action);
        }

        public IBinaryOperationValue Overload(IValue a, IValue b)
        {
            A = a as ISymbolValue;
            B = b as ISymbolValue;
            return (A != null && B != null) ? this : null;
        }

        public override string ToString()
        {
            return "(" + A.ToString() + " == " + B.ToString() + ")";
        }
    }

    public class IntegerDifferentValue : IBoolValue, IBinaryOperationIntegerValue
    {
        public IIntegerValue A { get; set; }
        public IIntegerValue B { get; set; }

        public IntegerDifferentValue(IIntegerValue a, IIntegerValue b)
        {
            A = a;
            B = b;
        }
        
        public bool EvaluateBool(IDialogueContext context) 
        {
            return A.EvaluateInteger(context) != B.EvaluateInteger(context);
        }

        public void OnVisited(IDialogueContext context) {}

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
            return "(" + A.ToString() + " != " + B.ToString() + ")";
        }
    }

    public class BoolDifferentValue : IBoolValue, IBinaryOperationBoolValue
    {
        public IBoolValue A { get; set; }
        public IBoolValue B { get; set; }

        public BoolDifferentValue(IBoolValue a, IBoolValue b)
        {
            A = a;
            B = b;
        }
        
        public bool EvaluateBool(IDialogueContext context) 
        {
            return A.EvaluateBool(context) != B.EvaluateBool(context);
        }

        public void OnVisited(IDialogueContext context) {}

        public void Traverse(System.Action<IValue> action)
        {
            action.Invoke(this);
            A.Traverse(action);
            B.Traverse(action);
        }

        public IBinaryOperationValue Overload(IValue a, IValue b)
        {
            A = a as IBoolValue;
            B = b as IBoolValue;
            return (A != null && B != null) ? this : null;
        }

        public override string ToString()
        {
            return "(" + A.ToString() + " != " + B.ToString() + ")";
        }
    }

    public class SymbolDifferentValue : IBoolValue, IBinaryOperationSymbolValue
    {
        public ISymbolValue A { get; set; }
        public ISymbolValue B { get; set; }

        public SymbolDifferentValue(ISymbolValue a, ISymbolValue b)
        {
            A = a;
            B = b;
        }
        
        public bool EvaluateBool(IDialogueContext context) 
        {
            return A.EvaluateSymbol(context) != B.EvaluateSymbol(context);
        }

        public void OnVisited(IDialogueContext context) {}

        public void Traverse(System.Action<IValue> action)
        {
            action.Invoke(this);
            A.Traverse(action);
            B.Traverse(action);
        }

        public IBinaryOperationValue Overload(IValue a, IValue b)
        {
            A = a as ISymbolValue;
            B = b as ISymbolValue;
            return (A != null && B != null) ? this : null;
        }

        public override string ToString()
        {
            return "(" + A.ToString() + " != " + B.ToString() + ")";
        }
    }

    public class LEqualValue : IBoolValue, IBinaryOperationIntegerValue
    {
        public IIntegerValue A { get; set; }
        public IIntegerValue B { get; set; }

        public LEqualValue()
        {
            A = null;
            B = null;
        }

        public LEqualValue(IIntegerValue a, IIntegerValue b)
        {
            A = a;
            B = b;
        }
        
        public bool EvaluateBool(IDialogueContext context) 
        {
            return A.EvaluateInteger(context) <= B.EvaluateInteger(context);
        }

        public IBinaryOperationValue Overload(IValue a, IValue b)
        {
            A = a as IIntegerValue;
            B = b as IIntegerValue;
            return (A != null && B != null) ? this : null;
        }

        public void OnVisited(IDialogueContext context) {}

        public void Traverse(System.Action<IValue> action)
        {
            action.Invoke(this);
            A.Traverse(action);
            B.Traverse(action);
        }

        public override string ToString()
        {
            return "(" + A.ToString() + " <= " + B.ToString() + ")";
        }
    }

    public class GEqualValue : IBoolValue, IBinaryOperationIntegerValue
    {
        public IIntegerValue A { get; set; }
        public IIntegerValue B { get; set; }

        public GEqualValue()
        {
            A = null;
            B = null;
        }

        public GEqualValue(IIntegerValue a, IIntegerValue b)
        {
            A = a;
            B = b;
        }
        
        public bool EvaluateBool(IDialogueContext context) 
        {
            return A.EvaluateInteger(context) >= B.EvaluateInteger(context);
        }

        public IBinaryOperationValue Overload(IValue a, IValue b)
        {
            A = a as IIntegerValue;
            B = b as IIntegerValue;
            return (A != null && B != null) ? this : null;
        }

        public void OnVisited(IDialogueContext dataContext) {}

        public void Traverse(System.Action<IValue> action)
        {
            action.Invoke(this);
            A.Traverse(action);
            B.Traverse(action);
        }

        public override string ToString()
        {
            return "(" + A.ToString() + " >= " + B.ToString() + ")";
        }
    }

    public class LessValue : IBoolValue, IBinaryOperationIntegerValue
    {
        public IIntegerValue A { get; set; }
        public IIntegerValue B { get; set; }

        public LessValue()
        {
            A = null;
            B = null;
        }

        public LessValue(IIntegerValue a, IIntegerValue b)
        {
            A = a;
            B = b;
        }
        
        public bool EvaluateBool(IDialogueContext context) 
        {
            return A.EvaluateInteger(context) < B.EvaluateInteger(context);
        }

        public IBinaryOperationValue Overload(IValue a, IValue b)
        {
            A = a as IIntegerValue;
            B = b as IIntegerValue;
            return (A != null && B != null) ? this : null;
        }

        public void OnVisited(IDialogueContext dataContext) {}

        public void Traverse(System.Action<IValue> action)
        {
            action.Invoke(this);
            A.Traverse(action);
            B.Traverse(action);
        }

        public override string ToString()
        {
            return "(" + A.ToString() + " < " + B.ToString() + ")";
        }
    }

    public class GreaterValue : IBoolValue, IBinaryOperationIntegerValue
    {
        public IIntegerValue A { get; set; }
        public IIntegerValue B { get; set; }

        public GreaterValue()
        {
            A = null;
            B = null;
        }

        public GreaterValue(IIntegerValue a, IIntegerValue b)
        {
            A = a;
            B = b;
        }
        
        public bool EvaluateBool(IDialogueContext context) 
        {
            return A.EvaluateInteger(context) > B.EvaluateInteger(context);
        }

        public IBinaryOperationValue Overload(IValue a, IValue b)
        {
            A = a as IIntegerValue;
            B = b as IIntegerValue;
            return (A != null && B != null) ? this : null;
        }

        public void OnVisited(IDialogueContext dataContext) {}

        public void Traverse(System.Action<IValue> action)
        {
            action.Invoke(this);
            A.Traverse(action);
            B.Traverse(action);
        }

        public override string ToString()
        {
            return "(" + A.ToString() + " > " + B.ToString() + ")";
        }
    }
}