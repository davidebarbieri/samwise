// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    internal struct ExpressionOperator
    {
        public ConditionTokenId token;
        public int leftPriority;
        public int rightPriority;
        public System.Func<int, bool> evaluator;
        public System.Func<IValue> allocator;

        public ExpressionOperator(ConditionTokenId token, int leftPriority, int rightPriority, System.Func<int, bool> evaluator, System.Func<IValue> allocator)
        {
            this.token = token;
            this.leftPriority = leftPriority;
            this.rightPriority = rightPriority;
            this.evaluator = evaluator;
            this.allocator = allocator;
        }
    } 
}