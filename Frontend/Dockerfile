# Stage 1: Build
# FROM node:20-alpine AS build
# WORKDIR /app
# COPY Client/package.json ./
# RUN npm install
# COPY . .
# RUN npm run build

# # Stage 2: Serve
# FROM nginx:alpine
# COPY --from=build /app/dist/frontend /usr/share/nginx/html
# EXPOSE 80

# Stage 1: Build Angular app
FROM node:22.12-alpine AS build

# Set working directory inside container
WORKDIR /app

# Copy package files first (for efficient caching)
COPY Client/package*.json ./

# Install dependencies
RUN npm ci

# Copy the rest of the Angular app
COPY Client ./

# Build the Angular app (ensure outputPath matches your angular.json)
RUN npm run build

# Stage 2: NGINX server
FROM nginx:1.27-alpine

# Copy built files from Node stage
COPY --from=build /app/dist/client/browser /usr/share/nginx/html

# Expose port 80
EXPOSE 80

# Optional: custom NGINX config (if needed)
# COPY nginx.conf /etc/nginx/conf.d/default.conf
