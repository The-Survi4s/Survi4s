using System.Collections;
using System.Collections.Generic;

public class NetworkDataFormat
{
    public enum Format { Sender, Header }
    public enum Header
    {
        RCrd, RJnd, RnFd, RsF, MPos, BtDw, BtUp, PlCt, StGm, SwPy
    }
    public enum Sender { Server, Client }

    public Sender sender; 
}
