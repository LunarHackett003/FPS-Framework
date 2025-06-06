using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;


public enum AuthorityMode : int
{
    Server = 0,
    Client = 1,
}

public class ClientNetworkTransform : NetworkTransform
{
    public AuthorityMode authorityMode = AuthorityMode.Client;
    protected override bool OnIsServerAuthoritative() => authorityMode == AuthorityMode.Server;
}
