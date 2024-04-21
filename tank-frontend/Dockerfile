FROM node:21-alpine AS build-client
WORKDIR /app

# dependencies
COPY package.json .
COPY package-lock.json .
RUN npm i

# build
env CONTAINERMODE 1
COPY . .
RUN npm run build

