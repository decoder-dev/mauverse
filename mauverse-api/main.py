import click

from apps.chat.server.commands import chat_group
from apps.debt.server.commands import debt_group
from apps.gateway.server.commands import server_group
from apps.parser.server.commands import parser_group
from apps.schedule.commands.server import schedule_group


@click.group()
def run_group() -> None:
    """Expose the supported MAUverce service commands."""


if __name__ == "__main__":
    run_group.add_command(server_group)
    run_group.add_command(schedule_group)
    run_group.add_command(parser_group)
    run_group.add_command(debt_group)
    run_group.add_command(chat_group)
    run_group()
