using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Application.Configurations;
using AgileConfig.Server.Apisite.Controllers.api.v2.Models;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Apisite.Metrics;
using AgileConfig.Server.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace AgileConfig.Server.Apisite.Controllers.api.v2;

/// <summary>
///     Published configuration resources consumed by application clients.
/// </summary>
[ApiController]
[Route("api/v2/applications/{applicationId}/environments/{environment}/published-configurations")]
[TypeFilter(typeof(AppBasicAuthenticationAttribute))]
[TypeFilter(typeof(V2EnvironmentFilterAttribute))]
public sealed class PublishedConfigurationsController : ControllerBase
{
    private readonly IMeterService _meterService;
    private readonly IPublishedConfigurationQueryService _publishedConfigurationQueryService;

    public PublishedConfigurationsController(
        IPublishedConfigurationQueryService publishedConfigurationQueryService,
        IMeterService meterService)
    {
        _publishedConfigurationQueryService = publishedConfigurationQueryService;
        _meterService = meterService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PublishedConfigurationResource>>> GetAll(
        string applicationId,
        string environment)
    {
        var authenticatedApplicationId = Encrypt.UnboxBasicAuth(Request).Item1;
        if (applicationId != authenticatedApplicationId)
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Application identifier mismatch.",
                detail: "The route application identifier must match the Basic Authentication username.");

        var result = await _publishedConfigurationQueryService.GetAsync(applicationId, environment);
        if (!result.Succeeded) return NotFound();

        var timelineId = result.Value.PublishTimelineId;
        if (!string.IsNullOrEmpty(timelineId))
        {
            var etag = $"\"{timelineId}\"";
            Response.Headers.ETag = etag;
            Response.Headers.Append("X-Publish-Timeline-Id", timelineId);

            if (Request.Headers.IfNoneMatch == new StringValues(etag)) return StatusCode(StatusCodes.Status304NotModified);
        }

        _meterService.PullAppConfigCounter?.Add(1,
            new KeyValuePair<string, object>("appId", applicationId),
            new KeyValuePair<string, object>("env", environment));

        return Ok(result.Value.Configurations.Select(x => new PublishedConfigurationResource
        {
            Id = x.Id,
            ApplicationId = applicationId,
            Environment = environment,
            Group = x.Group,
            Key = x.Key,
            Value = x.Value
        }).ToList());
    }
}
