using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace EasyPeasy.Client
{
    public interface IHttpRequest
    {
        IList<X509Certificate> ClientCertificates { get; }
    }
}
