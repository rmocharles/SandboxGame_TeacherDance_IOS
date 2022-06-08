// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("mxgWGSmbGBMbmxgYGZyY2W5ybKTIY6GJUhD6tBKN7ZH+jblhmQzyg01Xe3CwUboYM8Yf57nNzjcpfTfF5s3GcirNVU5lLWXDbwvss1WVnaWBxwgBYQKgZ8W12A0hxlI7FnO+JvK9N6xq9gXNtxvTsNw2bgq3apGWLoU2ek/bjWUH1IL9xx4UchGoqNWzpy6a7IMsp173nfuAXu5BnxlqDCmbGDspFB8QM59Rn+4UGBgYHBkaZmNsEIAU2vvUILPWAIR1PczcPbwWgyBDJ1kmc0HjyQ2WSzTYognlirpDq5RMVUXwNBhCIJmbcSlxbY0BnwA57qaR9fDIzy1erkrrhYmpXep9Jz/ATfKc1ZyLDl6yLSW7j0oBazZGAVM6U/6SaBsaGBkY");
        private static int[] order = new int[] { 1,11,10,8,8,5,12,10,11,10,11,13,12,13,14 };
        private static int key = 25;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
