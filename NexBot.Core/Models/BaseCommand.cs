namespace NexBot.Core.Models
{
    using Fanex.Data.Repository;

    public abstract class BaseCommand : NonQueryCommand
    {
        public virtual int ErrCode { get; set; }

        public virtual string ErrMsg { get; set; }
    }
}