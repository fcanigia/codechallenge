using Microsoft.AspNetCore.Mvc;
using ClimateMonitor.Services;
using ClimateMonitor.Services.Models;
using Microsoft.AspNetCore.Mvc.Formatters.Xml;
using System.ComponentModel;

namespace ClimateMonitor.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ReadingsController : ControllerBase
{
    private readonly DeviceSecretValidatorService _secretValidator;
    private readonly AlertService _alertService;
    private readonly FirmwareValidatorService _firmwareValidatorService;

    public ReadingsController(
        DeviceSecretValidatorService secretValidator, 
        AlertService alertService,
        FirmwareValidatorService firmwareValidatorService)
    {
        _secretValidator = secretValidator;
        _alertService = alertService;
        _firmwareValidatorService = firmwareValidatorService;
    }

    /// <summary>
    /// Evaluate a sensor readings from a device and return possible alerts.
    /// </summary>
    /// <remarks>
    /// The endpoint receives sensor readings (temperature, humidity) values
    /// as well as some extra metadata (firmwareVersion), evaluates the values
    /// and generate the possible alerts the values can raise.
    /// 
    /// There are old device out there, and if they get a firmwareVersion 
    /// format error they will request a firmware update to another service.
    /// </remarks>
    /// <param name="deviceSecret">A unique identifier on the device included in the header(x-device-shared-secret).</param>
    /// <param name="deviceReadingRequest">Sensor information and extra metadata from device.</param>
    [HttpPost("evaluate")]
    public ActionResult<IEnumerable<Alert>> EvaluateReading(
        [FromBody] DeviceReadingRequest deviceReadingRequest)
    {
        const string customHeaderName = "x-device-shared-secret";
        Request.Headers.TryGetValue(customHeaderName, out var deviceSecret);

        if (!_secretValidator.ValidateDeviceSecret(deviceSecret))
        {
            return Problem(
                detail: "Device secret is not within the valid range.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        if (!_firmwareValidatorService.ValidateFirmware(deviceReadingRequest.FirmwareVersion))
        {
            return ValidationProblem(
                detail: "The firmware value does not match semantic versioning format.",
                statusCode: StatusCodes.Status400BadRequest
                );
        }

        return Ok(_alertService.GetAlerts(deviceReadingRequest));
    }
}
