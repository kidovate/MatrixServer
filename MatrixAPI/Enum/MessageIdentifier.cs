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
        /// Confirm encryption and begin node sync sequence.
        /// </summary>
        ConfirmEncryption,

        /// <summary>
        /// Node synchronization request.
        /// </summary>
        NodeSync,

        /// <summary>
        /// Server says all-ok, nodes are synchronized, begin operation.
        /// </summary>
        BeginOperation,

        /// <summary>
        /// Is the message valid?
        /// </summary>
        InvalidIdentity,

        /// <summary>
        /// Disconnect the host.
        /// </summary>
        Disconnect,

        /// <summary>
        /// Request a library URL for downloading.
        /// </summary>
        GetLibraryURL,

        /// <summary>
        /// Verify a node exists.
        /// </summary>
        NodeVerify
    }
}
