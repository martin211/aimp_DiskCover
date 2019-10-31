using AIMP.SDK;
using AIMP.SDK.MessageDispatcher;

namespace AIMP.DiskCover.Infrastructure
{
    public delegate AimpActionResult OnCoreMessage(AimpCoreMessageType message, int param1, int param2);

    public class AimpMessageHook : IAimpMessageHook
    {
        public event OnCoreMessage OnCoreMessage;

        public AimpActionResult CoreMessage(AimpCoreMessageType message, int param1, int param2)
        {
            if (OnCoreMessage != null)
            {
                return OnCoreMessage(message, param1, param2);
            }

            return AimpActionResult.NotImplemented;
        }
    }
}