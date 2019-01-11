using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RosClientLib
{
    public abstract class IRosMessage
    {
    }

    public interface IRosOp
    {
        Task<object> CallServiceAsync(IRosClient rosClient);

        void CallService<TOut>(
            IRosClient rosClient, RosClientLib.RosClient.ServiceCallback<TOut> callback) 
            where TOut : IRosMessage;
    }
}
