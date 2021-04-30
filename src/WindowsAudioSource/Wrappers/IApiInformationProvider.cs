using Windows.Foundation.Metadata;

namespace WindowsAudioSource.Wrappers
{
    /// <inheritdoc cref="ApiInformation"/>
    /// <remarks>
    /// This is a thin wrapper around <see cref="ApiInformation"/> to support mocking, since that class is static.
    /// </remarks>
    public interface IApiInformationProvider
    {
        /// <inheritdoc cref="ApiInformation.IsTypePresent(string)"/>
        bool IsTypePresent(string typeName);

        /// <inheritdoc cref="ApiInformation.IsMethodPresent(string, string)"/>
        bool IsMethodPresent(string typeName, string methodName);

        /// <inheritdoc cref="ApiInformation.IsMethodPresent(string, string, uint)"/>
        bool IsMethodPresent(string typeName, string methodName, uint inputParameterCount);

        /// <inheritdoc cref="ApiInformation.IsEventPresent(string, string)"/>
        bool IsEventPresent(string typeName, string eventName);

        /// <inheritdoc cref="ApiInformation.IsPropertyPresent(string, string)"/>
        bool IsPropertyPresent(string typeName, string propertyName);

        /// <inheritdoc cref="ApiInformation.IsReadOnlyPropertyPresent(string, string)"/>
        bool IsReadOnlyPropertyPresent(string typeName, string propertyName);

        /// <inheritdoc cref="ApiInformation.IsWriteablePropertyPresent(string, string)"/>
        bool IsWriteablePropertyPresent(string typeName, string propertyName);

        /// <inheritdoc cref="ApiInformation.IsEnumNamedValuePresent(string, string)"/>
        bool IsEnumNamedValuePresent(string enumTypeName, string valueName);

        /// <inheritdoc cref="ApiInformation.IsApiContractPresent(string, ushort)"/>
        bool IsApiContractPresent(string contractName, ushort majorVersion);

        /// <inheritdoc cref="ApiInformation.IsApiContractPresent(string, ushort, ushort)"/>
        bool IsApiContractPresent(string contractName, ushort majorVersion, ushort minorVersion);
    }
}
