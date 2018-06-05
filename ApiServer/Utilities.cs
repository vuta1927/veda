using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiServer
{
    public static class Utilities
    {
        public static Tuple<double, double> Centroid(double x1, double y1, double x2, double y2)  // tinh toa do trong tam
        {
            double x = (x1+x2)/2;
            double y = (y1+y2)/2;

            return Tuple.Create(x, y);
        }

        public static string ConvertTime(TimeSpan timeSpan)
        {
            var result = "";

            if (timeSpan.Hours > 0)
            {
                result += timeSpan.Hours + " hours " + timeSpan.Minutes + " minutes ";
                if (timeSpan.Seconds > 0)
                {
                    result += timeSpan.Seconds + " seconds";
                }
            }
            else
            {
                if (timeSpan.Minutes > 0)
                {
                    result += timeSpan.Minutes + " minutes ";
                    if (timeSpan.Seconds > 0)
                    {
                        result += timeSpan.Seconds + " seconds";
                    }
                }
                else
                {
                    result += timeSpan.Seconds + " seconds";
                }
            }
            return result;
        }

        public static string GeneratePassword()
        {
            bool requireNonLetterOrDigit = false;
            bool requireDigit = false;
            bool requireLowercase = true;
            bool requireUppercase = true;

            string randomPassword = string.Empty;

            int passwordLength = 8;

            Random random = new Random();
            while (randomPassword.Length != passwordLength)
            {
                int randomNumber = random.Next(48, 122);  // >= 48 && < 122 
                if (randomNumber == 95 || randomNumber == 96) continue;  // != 95, 96 _'

                char c = Convert.ToChar(randomNumber);

                if (requireDigit)
                    if (char.IsDigit(c))
                        requireDigit = false;

                if (requireLowercase)
                    if (char.IsLower(c))
                        requireLowercase = false;

                if (requireUppercase)
                    if (char.IsUpper(c))
                        requireUppercase = false;

                if (requireNonLetterOrDigit)
                    if (!char.IsLetterOrDigit(c))
                        requireNonLetterOrDigit = false;

                randomPassword += c;
            }

            if (requireDigit)
                randomPassword += Convert.ToChar(random.Next(48, 58));  // 0-9

            if (requireLowercase)
                randomPassword += Convert.ToChar(random.Next(97, 123));  // a-z

            if (requireUppercase)
                randomPassword += Convert.ToChar(random.Next(65, 91));  // A-Z

            if (requireNonLetterOrDigit)
                randomPassword += Convert.ToChar(random.Next(33, 48));  // symbols !"#$%&'()*+,-./

            return randomPassword;
        }
        
    }
}
