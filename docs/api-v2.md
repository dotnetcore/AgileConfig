# AgileConfig REST API v2

API v2 is available below `/api/v2`. It is additive: the existing `/api/*` endpoints remain available and keep
their original contracts.

## Authentication

- Administrative resources use HTTP Basic authentication with an AgileConfig administrator username and password.
- `published-configurations` uses the application ID and secret as the Basic authentication username and password.
- Service registration and heartbeat endpoints remain unauthenticated for compatibility with the existing service
  registry protocol. Service instance queries and deletion require administrator Basic authentication.

Administrative actions continue to enforce the same AgileConfig function permissions as their v1 equivalents.

## Resources

| Method | Path | Result |
| --- | --- | --- |
| `GET` | `/api/v2/applications` | List applications |
| `POST` | `/api/v2/applications` | Create an application |
| `GET` | `/api/v2/applications/{applicationId}` | Get an application |
| `PUT` | `/api/v2/applications/{applicationId}` | Replace an application |
| `DELETE` | `/api/v2/applications/{applicationId}` | Delete an application |
| `GET` | `/api/v2/applications/{applicationId}/environments/{environment}/configurations` | List editable configurations |
| `POST` | `/api/v2/applications/{applicationId}/environments/{environment}/configurations` | Create a configuration |
| `GET` | `/api/v2/applications/{applicationId}/environments/{environment}/configurations/{configurationId}` | Get a configuration |
| `PUT` | `/api/v2/applications/{applicationId}/environments/{environment}/configurations/{configurationId}` | Replace a configuration |
| `DELETE` | `/api/v2/applications/{applicationId}/environments/{environment}/configurations/{configurationId}` | Mark a configuration for deletion |
| `GET` | `/api/v2/applications/{applicationId}/environments/{environment}/published-configurations` | Pull the effective published configuration |
| `GET` | `/api/v2/applications/{applicationId}/environments/{environment}/releases` | List releases |
| `POST` | `/api/v2/applications/{applicationId}/environments/{environment}/releases` | Publish pending changes |
| `GET` | `/api/v2/applications/{applicationId}/environments/{environment}/releases/{releaseId}` | Get a release |
| `POST` | `/api/v2/applications/{applicationId}/environments/{environment}/releases/rollbacks` | Roll back to a release |
| `GET` | `/api/v2/nodes` | List cluster nodes |
| `POST` | `/api/v2/nodes` | Add a cluster node |
| `GET` | `/api/v2/nodes/{nodeId}` | Get a cluster node |
| `DELETE` | `/api/v2/nodes/{nodeId}` | Delete a cluster node |
| `GET` | `/api/v2/service-instances?status={Healthy\|Unhealthy}` | List or filter service instances |
| `POST` | `/api/v2/service-instances` | Register or refresh a service instance |
| `GET` | `/api/v2/service-instances/{serviceInstanceId}` | Get a service instance |
| `DELETE` | `/api/v2/service-instances/{serviceInstanceId}` | Unregister a service instance |
| `PUT` | `/api/v2/service-instances/{serviceInstanceId}/heartbeat` | Record a heartbeat |

Node IDs are opaque URL-safe values returned by the API. Clients should not construct them from node addresses.

## HTTP semantics

- Creation returns `201 Created`, the created representation, and a `Location` header. Re-registering an existing
  service instance returns `200 OK` because the existing resource is updated.
- Successful deletion and rollback return `204 No Content`.
- Missing resources return `404 Not Found`; identifier/key conflicts return `409 Conflict`.
- Validation and domain errors use `application/problem+json` (`ProblemDetails`). Authentication and permission
  failures retain the server's existing `403 Forbidden` behavior.
- Published configuration responses include `ETag` and `X-Publish-Timeline-Id`. Sending the ETag in
  `If-None-Match` returns `304 Not Modified` when the published version has not changed.

Deleting an already published configuration removes it from the v2 editable resource view immediately, while the
previous published value remains available to application clients until the pending deletion is released.
