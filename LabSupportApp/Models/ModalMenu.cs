using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LabSupportApp.Models
{
    public class ModalMenu
    {
        public bool Sound { get; set; }
        public bool Vibration { get; set; }
        public bool PushNotifications { get; set; }

        public event Action OnChange;

        public void SetSoundPreferences(bool value)
        {
            Sound = value;
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
