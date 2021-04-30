using System.Linq;
using Xunit;

namespace WindowsAudioSource.Test
{
    /// <summary>
    /// Tests to ensure the strings used for setting name/description will not be truncated by the size of the settngs window.
    /// </summary>
    public class SettingsConstantsTests
    {
        // This seems to be a good length that shouldn't get truncated by the settings window
        private const int MaxLineLength = 90;

        [Theory]
        [MemberData(nameof(SettingConstantNameValuePairs))]
        public void SettingConstant_CheckValueLength(string name, string value)
        {
            var settingConstantValueLines = value.Split('\n');
            for (int lineIndex = 0; lineIndex < settingConstantValueLines.Length; lineIndex++)
            {
                var line = settingConstantValueLines[lineIndex];
                Assert.True(line.Length <= MaxLineLength, $"Line {lineIndex + 1} of setting constant '{name}' exceeds the max line length. (Max={MaxLineLength}; Actual={line.Length})");
            }
        }

        /// <summary>
        /// Reflectively gets all constants defined in <see cref="SettingConstants"/> and composes a <see cref="TheoryData{T1, T2}"/>
        /// containing their names and values for parameterized tests.
        /// </summary>
        /// <returns>A <see cref="TheoryData{T1, T2}"/> containing the name and value of each setting constant.</returns>
        public static TheoryData<string, string> SettingConstantNameValuePairs()
        {
            var data = new TheoryData<string, string>();
            foreach (var constant in typeof(SettingConstants).GetFields().Where(field => field.IsLiteral && !field.IsInitOnly))
            {
                data.Add(constant.Name, constant.GetValue(null).ToString());
            }
            return data;
        }
    }
}
