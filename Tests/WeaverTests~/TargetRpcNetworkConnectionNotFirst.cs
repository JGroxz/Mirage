using System;
using System.Collections;
using UnityEngine;
using Mirror;

namespace MirrorTest
{
    class MirrorTestPlayer : NetworkBehaviour
    {
        [TargetRpc]
        private void TargetRpcMethod(int potatoesRcool, NetworkConnection nc)
        {
        }
    }
}