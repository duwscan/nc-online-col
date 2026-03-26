.PHONY: db-up db-down db-restart db-logs db-ps db-reset db-env-check
.PHONY: app-build app-up app-down app-restart app-logs app-ps app-rm
.PHONY: build up down restart logs ps rm

COMPOSE ?= docker compose

db-up:
	$(COMPOSE) up -d mssql

db-down:
	$(COMPOSE) down

db-restart: db-down db-up

db-logs:
	$(COMPOSE) logs -f --tail=200 mssql

db-ps:
	$(COMPOSE) ps

db-reset:
	$(COMPOSE) down -v

db-env-check:
	@echo "Checking Docker environment..."
	@docker --version && docker compose version || (echo "Docker not found!" && exit 1)

app-build:
	$(COMPOSE) build app

app-up:
	$(COMPOSE) up -d app

app-down:
	$(COMPOSE) down

app-restart: app-down app-up

app-logs:
	$(COMPOSE) logs -f --tail=200 app

app-ps:
	$(COMPOSE) ps

app-rm:
	$(COMPOSE) rm -f app

build: app-build
up: app-up
down: app-down
restart: app-restart
logs: app-logs
ps: app-ps
rm: app-rm

docker-up: db-up
docker-build: app-build
docker-start: up

docker-logs-app:
	$(COMPOSE) logs -f app

docker-logs-db:
	$(COMPOSE) logs -f mssql

docker-restart-app:
	$(COMPOSE) restart app

docker-restart-db:
	$(COMPOSE) restart mssql

docker-clean:
	$(COMPOSE) down -v --rmi local

docker-status:
	@echo "=== Docker Status ===" && $(COMPOSE) ps -a
