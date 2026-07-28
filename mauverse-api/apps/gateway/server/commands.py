import click
import uvicorn


@click.group("server")
def server_group() -> None:
    """Run MAUverce API server commands."""


@server_group.command(name="main")
@click.option(
    "-h",
    "--host",
    "uv_host",
    default="127.0.0.1",
    help=("IP address or local domain name to run server on"),
)
@click.option("-p", "--port", "uv_port", default=8000, help="Server port")
@click.option(
    "-l",
    "--log-level",
    "uv_log_level",
    default="info",
    help="Logging level. One of: [critical|error|warning|info|debug|trace]",
)
@click.option(
    "--reload/--no-reload", "uv_reload", default=False, help="Enable the development reloader"
)
@click.option("-w", "--workers", "uv_workers", default=None, help="Count of workers for server")
def run_server(
    uv_host: str = "127.0.0.1",
    uv_port: int = 8000,
    uv_log_level: str = "info",
    uv_workers: int | None = None,
    uv_reload: bool = False,
) -> None:
    uvicorn.run(
        "apps.gateway.main:app",
        host=uv_host,
        port=uv_port,
        log_level=uv_log_level,
        reload=uv_reload,
        workers=uv_workers,
    )
