// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public class IncrementAssignmentStatement : IntegerAssignmentStatement
    {
        public override void Execute(IDialogueContext context)
        {
            var dataContext = context.LookupOrCreateDataContext(Context);

            dataContext.SetValueInt(Name, dataContext.GetValueInt(Name) + Value.EvaluateInteger(context));
        }

        public override string ToString()
        {
            return Context + Name + " += " + Value.ToString();
        }
    }
    public class DecrementAssignmentStatement : IntegerAssignmentStatement
    {
        public override void Execute(IDialogueContext context)
        {
            var dataContext = context.LookupOrCreateDataContext(Context);

            dataContext.SetValueInt(Name, dataContext.GetValueInt(Name) - Value.EvaluateInteger(context));
        }

        public override string ToString()
        {
            return Context + Name + " -= " + Value.ToString();
        }
    }
}