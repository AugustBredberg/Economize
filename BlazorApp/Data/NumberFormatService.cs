using System.Globalization;

namespace BlazorApp.Data
{
    public class NumberFormatService
    {

        public double RoundToThousands(double numberToFormat)
        {
            numberToFormat = numberToFormat % 1000 >= 500 ? numberToFormat + 1000 - numberToFormat % 1000 : numberToFormat - numberToFormat % 1000;
            return numberToFormat;
        }

        public string NumberWithSpaces(long number)
        {
            var nfi = new NumberFormatInfo();
            nfi.NumberGroupSeparator = " "; // set the group separator to a space

            var numberWithSpacesString = number.ToString("N2", nfi);

            // Find and remove the trailing .00
            var dotIndex = numberWithSpacesString.IndexOf('.');
            numberWithSpacesString = numberWithSpacesString.Substring(0, dotIndex);

            return numberWithSpacesString;
        }

    }
}