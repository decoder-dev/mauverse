import click
import uvicorn


@click.group(name="parser")
def parser_group() -> None:
    """Run the legacy parser gateway command."""


@parser_group.command(name="run")
@click.option(
    "-h",
    "--host",
    "uv_host",
    default="127.0.0.1",
    help=("IP address or local domain name to run server on"),
)
@click.option("-p", "--port", "uv_port", default=8002, help="Server port")
@click.option(
    "-l",
    "--log-level",
    "uv_log_level",
    default="info",
    help="Logging level. One of: [critical|error|warning|info|debug|trace]",
)
@click.option("-r", "--reload", "uv_reload", default=True, help="Enable reloading subprocess")
@click.option("-w", "--workers", "uv_workers", default=None, help="Count of workers for server")
def run_server(
    uv_host: str = "127.0.0.1",
    uv_port: int = 8002,
    uv_log_level: str = "info",
    uv_workers: int | None = None,
    uv_reload: bool = True,
) -> None:
    uvicorn.run(
        "apps.gateway.main:app",
        host=uv_host,
        port=uv_port,
        ws="websockets",
        log_level=uv_log_level,
        reload=uv_reload,
        workers=uv_workers,
    )
