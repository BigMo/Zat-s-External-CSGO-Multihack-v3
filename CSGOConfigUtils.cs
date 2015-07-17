using ExternalUtilsCSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGOTriggerbot
{
    public class CSGOConfigUtils : ConfigUtils
    {
        #region PROPERTIES
        public List<string> UIntegerSettings { get; set; }
        public List<string> IntegerSettings { get; set; }
        public List<string> FloatSettings { get; set; }
        public List<string> KeySettings { get; set; }
        public List<string> BooleanSettings { get; set; }
        #endregion

        #region CONSTRUCTOR
        public CSGOConfigUtils() : base()
        {
            this.IntegerSettings = new List<string>();
            this.UIntegerSettings = new List<string>();
            this.FloatSettings = new List<string>();
            this.KeySettings = new List<string>();
            this.BooleanSettings = new List<string>();
        }
        #endregion

        #region METHODS
        public void FillDefaultValues()
        {
            foreach (string integerV in IntegerSettings)
                this.SetValue(integerV, 0);
            foreach (string uintegerV in UIntegerSettings)
                this.SetValue(uintegerV, 0);
            foreach (string floatV in FloatSettings)
                this.SetValue(floatV, 0f);
            foreach (string keyV in KeySettings)
                this.SetValue(keyV, WinAPI.VirtualKeyShort.LBUTTON);
            foreach (string booleanV in BooleanSettings)
                this.SetValue(booleanV, false);
        }
        public override void ReadSettings(byte[] data)
        {
            string text = Encoding.Unicode.GetString(data);

            //Split text into lines
            string[] lines = text.Contains("\r\n") ? text.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries) : text.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                //Trim current line
                string tmpLine = line.Trim();
                //Skip invalid ones
                if (tmpLine.StartsWith("#")) // comment
                    continue;
                else if (!tmpLine.Contains("=")) // it's no key-value pair!
                    continue;

                //Trim both parts of the key-value pair
                string[] parts = tmpLine.Split('=');
                parts[0] = parts[0].Trim();
                parts[1] = parts[1].Trim();
                if (string.IsNullOrEmpty(parts[0]) || string.IsNullOrEmpty(parts[1]))
                    continue;
                if (parts[1].Contains('#')) //If value-part contains comment, split it
                    parts[1] = parts[1].Split('#')[0];
                InterpretSetting(parts[0], parts[1]);
            }
        }

        private void InterpretSetting(string name, string value)
        {
            try
            {
                if (this.FloatSettings.Contains(name))
                    this.SetValue(name, Convert.ToSingle(value));
                else if (this.IntegerSettings.Contains(name))
                    this.SetValue(name, Convert.ToInt32(value));
                else if (this.UIntegerSettings.Contains(name))
                    this.SetValue(name, Convert.ToUInt32(value));
                else if (this.BooleanSettings.Contains(name))
                    this.SetValue(name, Convert.ToBoolean(value));
                else if (this.KeySettings.Contains(name))
                    this.SetValue(name, ParseEnum<WinAPI.VirtualKeyShort>(value));
                else
                    WithOverlay.PrintError("Unknown settings-field \"{0}\" (value: \"{1}\")", name, value);
            }
            catch(Exception ex)
            {
                WithOverlay.PrintException(ex);
            }
        }

        public override byte[] SaveSettings()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(@"#	                        .____   ____ ____________           ___________             .__               .__                ____.                                  ");
            builder.AppendLine(@"#	                        |   _| |    |   \_   ___ \          \_   _____/__  ___ ____ |  |  __ __  _____|__|__  __ ____   |_   |                                  ");
            builder.AppendLine(@"#	                        |  |   |    |   /    \  \/   ______  |    __)_\  \/  // ___\|  | |  |  \/  ___/  \  \/ // __ \    |  |                                  ");
            builder.AppendLine(@"#	                        |  |   |    |  /\     \____ /_____/  |        \>    <\  \___|  |_|  |  /\___ \|  |\   /\  ___/    |  |                                  ");
            builder.AppendLine(@"#	                        |  |_  |______/  \______  /         /_______  /__/\_ \\___  >____/____//____  >__| \_/  \___  >  _|  |                                  ");
            builder.AppendLine(@"#	                        |____|                  \/ puddin tells lies\/      \/    \/                \/              \/  |____|                                  ");
            builder.AppendLine(@"#	__________       __ /\        _________   _________ ________ ________               _____        .__   __  .__.__                   __     ____   ____________  ");
            builder.AppendLine(@"#	\____    /____ _/  |)/ ______ \_   ___ \ /   _____//  _____/ \_____  \             /     \  __ __|  |_/  |_|__|  |__ _____    ____ |  | __ \   \ /   /\_____  \ ");
            builder.AppendLine(@"#	  /     /\__  \\   __\/  ___/ /    \  \/ \_____  \/   \  ___  /   |   \   ______  /  \ /  \|  |  \  |\   __\  |  |  \\__  \ _/ ___\|  |/ /  \   Y   /   _(__  < ");
            builder.AppendLine(@"#	 /     /_ / __ \|  |  \___ \  \     \____/        \    \_\  \/    |    \ /_____/ /    Y    \  |  /  |_|  | |  |   Y  \/ __ \\  \___|    <    \     /   /       \");
            builder.AppendLine(@"#	/_______ (____  /__| /____  >  \______  /_______  /\______  /\_______  /         \____|__  /____/|____/__| |__|___|  (____  /\___  >__|_ \    \___/   /______  /");
            builder.AppendLine(@"#	        \/    \/          \/          \/        \/        \/         \/                  \/                        \/     \/     \/     \/                   \/ ");
            object[] keys = new object[this.GetKeys().Count];
            this.GetKeys().CopyTo(keys, 0);
            var keysSorted = keys.OrderBy(x => x);
            foreach (string key in keysSorted)
            {
                builder.AppendFormat("{0} = {1}\n", key, this.GetValue(key));
            }
            return Encoding.Unicode.GetBytes(builder.ToString());
        }
        #endregion
    }
}
