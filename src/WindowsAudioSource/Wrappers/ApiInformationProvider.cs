using Windows.Foundation.Metadata;

namespace WindowsAudioSource.Wrappers
{
    public class ApiInformationProvider : IApiInformationProvider
    {
        public bool IsTypePresent(string typeName) =>
            ApiInformation.IsTypePresent(typeName);

        public bool IsMethodPresent(string typeName, string methodName) =>
            ApiInformation.IsMethodPresent(typeName, methodName);

        public bool IsMethodPresent(string typeName, string methodName, uint inputParameterCount) =>
            ApiInformation.IsMethodPresent(typeName, methodName, inputParameterCount);

        public bool IsEventPresent(string typeName, string eventName) =>
            ApiInformation.IsEventPresent(typeName, eventName);

        public bool IsPropertyPresent(string typeName, string propertyName) =>
            ApiInformation.IsPropertyPresent(typeName, propertyName);

        public bool IsReadOnlyPropertyPresent(string typeName, string propertyName) =>
            ApiInformation.IsReadOnlyPropertyPresent(typeName, propertyName);

        public bool IsWriteablePropertyPresent(string typeName, string propertyName) =>
            ApiInformation.IsWriteablePropertyPresent(typeName, propertyName);

        public bool IsEnumNamedValuePresent(string enumTypeName, string valueName) =>
            ApiInformation.IsEnumNamedValuePresent(enumTypeName, valueName);

        public bool IsApiContractPresent(string contractName, ushort majorVersion) =>
            ApiInformation.IsApiContractPresent(contractName, majorVersion);

        public bool IsApiContractPresent(string contractName, ushort majorVersion, ushort minorVersion) =>
            ApiInformation.IsApiContractPresent(contractName, majorVersion, minorVersion);
    }
}
