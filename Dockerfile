FROM localhost/cccb-tanks-cs-backend:latest

COPY --from=localhost/cccb-tanks-cs-frontend /app/dist ./client

