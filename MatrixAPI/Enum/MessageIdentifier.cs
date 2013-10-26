namespace MatrixAPI.Enum
{
    /// <summary>
    /// Identifiers for various inter-system messages.
    /// </summary>
    public enum MessageIdentifier : byte
    {
        /// <summary>
        /// The initial "hello".
        /// </summary>
        Init,

        /// <summary>
        /// Server assigning client an identity.
        /// </summary>
        SetIdentity,

        /// <summary>
        /// Server requesting encryption md5.
        /// </summary>
        BeginEncryption,

        /// <summary>
        /// Encryption key is not registered in the server. Host will be disconnected.
        /// </summary>
        InvalidKey,

        /// <summary>
        /// Confirm encryption and begin operating.
        /// </summary>
        ConfirmEncryption
    }
}
