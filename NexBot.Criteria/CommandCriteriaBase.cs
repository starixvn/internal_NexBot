namespace NexBot.Criteria
{
    using Fanex.Data.Repository;

    public abstract class CommandCriteriaBase : CriteriaBase
    {
        public virtual int ErrCode { get; set; }

        public virtual string ErrMsg { get; set; }
    }
}