using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.CommWPF
{
    public class RepaintAuxiliaryMessage : ValueChangedMessage<AuxiliaryInfo>
    {
        public RepaintAuxiliaryMessage(AuxiliaryInfo value) : base(value)
        {
        }
    }
}
