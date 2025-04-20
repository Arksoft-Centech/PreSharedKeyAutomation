using System;

namespace KeyExchange.Client.Items
{
    /// <summary>
    /// Author: Can DOĞU
    /// </summary>
    public class InternalKeyItem
    {
        public Guid ClientGuid { get; set; }
        public string PreSharedKey { get; set; }
        public string IvPaddingKey { get; set; }
        public string SignatureKey { get; set; }
    }
}
