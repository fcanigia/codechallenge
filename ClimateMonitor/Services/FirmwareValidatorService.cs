using System.Text.RegularExpressions;

namespace ClimateMonitor.Services;

public class FirmwareValidatorService
{
    public bool ValidateFirmware(string firmware)
    {
        var pattern = @"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-((?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$";

        bool isValid = Regex.IsMatch(firmware, pattern);

        return isValid;
    }
}
