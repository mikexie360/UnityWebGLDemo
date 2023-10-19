using GameFramework.Core;
using Game.Events;

namespace Game
{
    public class DeviceManager : Singleton<DeviceManager>
    {
        private string inputDevice;
        private string outputDevice;

        private void OnEnable()
        {
            SettingEvents.OnInputUpdated += OnInputUpdated;
            SettingEvents.OnOutputUpdated += OnOutputUpdated;
        }

        private void OnDisable()
        {
            SettingEvents.OnInputUpdated -= OnInputUpdated;
            SettingEvents.OnOutputUpdated -= OnOutputUpdated;
        }

        private void OnInputUpdated(string input)
        {
            inputDevice = input;
        }

        private void OnOutputUpdated(string output)
        {
            outputDevice = output;
        }

        public string GetInput()
        {
            return inputDevice;
        }

        public string GetOutput()
        {
            return outputDevice;
        }

    }
}
