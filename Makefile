.PHONY: db-up db-down db-restart db-logs db-ps db-reset db-env-check

COMPOSE ?= docker compose
SERVICE ?= mssql

db-up:
	$(COMPOSE) up -d $(SERVICE)

db-down:
	$(COMPOSE) down

db-restart: db-down db-up

db-logs:
	$(COMPOSE) logs -f --tail=200 $(SERVICE)

db-ps:
	$(COMPOSE) ps

db-reset:
	$(COMPOSE) down -v
