﻿// IMessage

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace Musixx.Shared.Messages
{
    public interface IMessage
    {
        void Serialize(ValueSet value);
        void Deserialize(ValueSet value);
    }
}
