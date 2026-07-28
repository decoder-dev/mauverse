class DatabaseUnavailableError(RuntimeError):
    """Raised when a database operation cannot be completed safely."""


class UpstreamResponseError(RuntimeError):
    """Raised when an upstream service returns an unusable response."""


class MoodleAuthenticationError(RuntimeError):
    """Raised when Moodle explicitly rejects authentication material."""


class MoodleInvalidCredentialsError(MoodleAuthenticationError):
    """Raised when Moodle reports the invalidlogin error code."""


class MoodleInvalidTokenError(MoodleAuthenticationError):
    """Raised when Moodle reports the invalidtoken error code."""


class MoodleServiceError(RuntimeError):
    """Raised when Moodle reports an internal failure or is unavailable."""
