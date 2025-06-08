FROM grafana/k6:latest

WORKDIR /tests
COPY scripts/ ./scripts
COPY data/ ./data
COPY k6-run.sh .

ENTRYPOINT ["./k6-run.sh"]