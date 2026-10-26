FROM python:3.14-slim-bookworm@sha256:945c2676dc2b18e0c1f16879244122c57c0ffede60006bf3df159c507a31bcae

ENV PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1 \
    PIP_DISABLE_PIP_VERSION_CHECK=1 \
    PIP_NO_CACHE_DIR=1

WORKDIR /app

RUN groupadd --gid 10001 mauverce \
    && useradd --uid 10001 --gid 10001 --no-create-home --shell /usr/sbin/nologin mauverce

COPY requirements.txt ./requirements.txt
RUN python -m pip install --no-cache-dir --requirement requirements.txt \
    && python -m pip check

COPY --chown=10001:10001 apps ./apps
COPY --chown=10001:10001 main.py ./main.py

USER 10001:10001
EXPOSE 8000

HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD ["python", "-c", "import urllib.request; r=urllib.request.urlopen('http://127.0.0.1:8000/health', timeout=2); code=r.status; r.close(); raise SystemExit(0 if code == 200 else 1)"]

CMD ["uvicorn", "apps.gateway.main:app", "--host", "0.0.0.0", "--port", "8000", "--workers", "1", "--proxy-headers"]
