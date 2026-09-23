# PartsHub

A miniature industrial-parts catalog and ordering system built as two **.NET 8 microservices** — a small-scale homage to real-world industrial parts e-commerce sites (think EZParts / VPParts style catalogs).

- **Catalog.API** — owns the parts catalog: list, search, fetch, and add industrial parts (couplings, bearings, gears…).
- **Orders.API** — owns order placement: validates the requested part against Catalog.API over HTTP before creating an order.

Everything runs in-memory — no databases, no secrets, no external dependencies. Clone it, run it, extend it.

## Architecture

```
                        +------------------+
                        |   HTTP client    |
                        | (curl / browser  |
                        |  / frontend)     |
                        +--------+---------+
                                 |
                 +----------------+------------------+
                 |                                   |
                 v                                   v
        +-----------------+                 +-----------------+
        |   Catalog.API   |<----------------|   Orders.API    |
        | localhost:5001  |  GET /parts/{id}| localhost:5002  |
        |                 |  validates that |                 |
        | in-memory parts |  the part exists| in-memory orders|
        | store (seeded)  |  and checks     | store           |
        +-----------------+  stock          +-----------------+
```

**Order flow:** `POST /orders` → Orders.API calls `GET /parts/{id}` on Catalog.API → if the part exists and stock covers the quantity, the order is created (`201 Created`) with a generated order number, unit price snapshot, and total.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/) — only needed for the containerized run

## Run locally

```bash
# from the repo root
dotnet build PartsHub.sln

# terminal 1 — catalog on http://localhost:5001
dotnet run --project src/Catalog.API

# terminal 2 — orders on http://localhost:5002
dotnet run --project src/Orders.API
```

## Run with Docker Compose

```bash
docker compose up --build
```

- Catalog.API → http://localhost:5001
- Orders.API → http://localhost:5002

Stop everything with `docker compose down`.

## API reference

### Catalog.API (`http://localhost:5001`)

| Method | Endpoint               | Description                              |
|--------|------------------------|------------------------------------------|
| GET    | `/healthz`             | Health check                             |
| GET    | `/parts`               | List all parts (`?category=Coupling` filters) |
| GET    | `/parts/search?q=`     | Search by name, SKU, or description      |
| GET    | `/parts/{id}`          | Get one part by id                       |
| POST   | `/parts`               | Add a part (validates input, rejects duplicate SKUs) |

```bash
# List everything
curl http://localhost:5001/parts

# Filter by category
curl "http://localhost:5001/parts?category=Coupling"

# Search
curl "http://localhost:5001/parts/search?q=bearing"

# Get one part
curl http://localhost:5001/parts/bearing-6205-2rs

# Add a part
curl -X POST http://localhost:5001/parts \
  -H "Content-Type: application/json" \
  -d '{
    "sku": "SEAL-OIL-TC-40X62",
    "name": "Oil Seal TC 40x62x10",
    "category": "Seal",
    "description": "Double-lip rotary shaft oil seal, 40x62x10 mm.",
    "unitPrice": 4.25,
    "stockQuantity": 210
  }'
```

Sample response (`GET /parts/bearing-6205-2rs`):

```json
{
  "id": "bearing-6205-2rs",
  "sku": "BRG-6205-2RS",
  "name": "Deep Groove Ball Bearing 6205-2RS",
  "category": "Bearing",
  "description": "Single-row deep groove ball bearing, 25 x 52 x 15 mm, rubber sealed both sides.",
  "unitPrice": 8.49,
  "stockQuantity": 320
}
```

### Orders.API (`http://localhost:5002`)

| Method | Endpoint        | Description                                                  |
|--------|-----------------|--------------------------------------------------------------|
| GET    | `/healthz`      | Health check                                                 |
| GET    | `/orders`       | List all orders, newest first                                |
| GET    | `/orders/{id}`  | Get one order by id                                          |
| POST   | `/orders`       | Create an order (validates the part with Catalog.API first)  |

```bash
# Place an order for 10 bearings
curl -X POST http://localhost:5002/orders \
  -H "Content-Type: application/json" \
  -d '{
    "partId": "bearing-6205-2rs",
    "quantity": 10,
    "customerName": "Acme Manufacturing"
  }'

# Ordering a part that doesn't exist -> 404
curl -X POST http://localhost:5002/orders \
  -H "Content-Type: application/json" \
  -d '{ "partId": "nope", "quantity": 1, "customerName": "Acme" }'

# Ordering more than is in stock -> 409
curl -X POST http://localhost:5002/orders \
  -H "Content-Type: application/json" \
  -d '{ "partId": "hex-bolt-m12x50", "quantity": 999999, "customerName": "Acme" }'

# List orders, then fetch one by id
curl http://localhost:5002/orders
curl http://localhost:5002/orders/<id-from-above>
```

Sample response (`POST /orders` → `201 Created`):

```json
{
  "id": "a1b2c3d4e5f6",
  "orderNumber": "PO-20260922-4821",
  "partId": "bearing-6205-2rs",
  "partSku": "BRG-6205-2RS",
  "partName": "Deep Groove Ball Bearing 6205-2RS",
  "quantity": 10,
  "unitPrice": 8.49,
  "totalPrice": 84.90,
  "customerName": "Acme Manufacturing",
  "status": "Placed",
  "createdAt": "2026-09-22T22:40:00+00:00"
}
```

## Configuration

| Setting | Default | Description |
|---------|---------|-------------|
| `CatalogApi:BaseUrl` | `http://localhost:5001` | Where Orders.API finds Catalog.API. Docker Compose sets this to `http://catalog-api:8080` via the `CatalogApi__BaseUrl` env var. |

## Seeded parts

| SKU | Name | Category | Price | Stock |
|-----|------|----------|------:|------:|
| JAW-L095-CI | Jaw Coupling - L095 with Spider Insert | Coupling | $24.99 | 140 |
| BRG-6205-2RS | Deep Groove Ball Bearing 6205-2RS | Bearing | $8.49 | 320 |
| GEAR-SPUR-20T-M2 | Spur Gear - 20 Teeth, Module 2 | Gear | $18.75 | 85 |
| CPLG-FLN-F100 | Flanged Rigid Coupling F100 | Coupling | $42.00 | 60 |
| FAST-HHB-M12X50 | Hex Head Bolt M12 x 50 - Class 8.8 | Fastener | $0.85 | 2500 |
| BELT-V-B60 | V-Belt - B Section, 1524 mm | Belt | $12.30 | 190 |

## Ideas to extend

- **Persistence** — swap the in-memory stores for PostgreSQL / SQL Server with EF Core.
- **Stock management** — decrement stock when an order is placed (or reserve it with a saga).
- **Order lifecycle** — add `PATCH /orders/{id}/cancel` and status transitions.
- **Auth** — JWT bearer auth with roles (admin vs. customer).
- **API gateway** — put YARP in front of both services with a single public port.
- **Events** — publish order events to RabbitMQ; add a notification worker.
- **Observability** — OpenTelemetry tracing across the service boundary.
- **Tests** — xUnit integration tests using `WebApplicationFactory`.
- **CI** — GitHub Actions workflow: build, test, and publish Docker images.
- **Frontend** — a Blazor or React storefront consuming both APIs.
