# In-game bug reports (P84, docs/DEPLOY.md). Built on Geoffrey as kestrel9-reports:latest; started by deploy/server/compose.yml.
FROM python:3.12-alpine
RUN adduser -D -H -u 10001 reports
COPY server.py /app/server.py
USER 10001
ENV PYTHONUNBUFFERED=1 PYTHONDONTWRITEBYTECODE=1
EXPOSE 8000
CMD ["python", "/app/server.py"]
