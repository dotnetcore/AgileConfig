# Application layer

`AgileConfig.Server.Application` contains transport-independent application use cases. Both the legacy MVC controllers and the REST API controllers call this layer so that business behavior is implemented once.

## Dependency direction

```text
Apisite controllers
        |
        v
AgileConfig.Server.Application
        |
        v
AgileConfig.Server.IService / domain events
        |
        v
repositories
```

The Application project must not reference ASP.NET Core MVC, Apisite request models, controllers, or HTTP result types.

## Responsibilities

Controllers own:

- authentication and authorization filters;
- HTTP request and response models;
- status codes, headers, routes, and `ProblemDetails`;
- compatibility mapping for legacy response envelopes.

Application services own:

- use-case validation and conflict detection;
- entity creation and state transitions;
- orchestration of the existing business services;
- current-user audit values and timestamps;
- domain-event publication and other required side effects.

The existing services in `AgileConfig.Server.IService` and `AgileConfig.Server.Service` continue to own persistence-oriented operations, queries, caches, and lower-level domain behavior.

Expected business failures are returned as `ApplicationResult` values. Application services do not return `IActionResult`, select HTTP status codes, or format user-facing HTTP payloads.

## Compatibility

Legacy and v2 controllers deliberately map the same application result differently:

- legacy controllers preserve their existing `{ success, message, data }` envelopes and status-code behavior;
- v2 controllers use resource responses, standard status codes, and `ProblemDetails`.

Any application-service migration must keep integration coverage for both contracts.
