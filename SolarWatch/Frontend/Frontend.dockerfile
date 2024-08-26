FROM node:alpine3.11 AS builder

WORKDIR /app

COPY Frontend/package*.json ./

RUN npm install

COPY Frontend/. .

RUN npm run build

# Stage 2: Serve the static files using nginx
FROM nginx:alpine

# Copy custom nginx configuration
COPY Frontend/nginx-dev.conf /etc/nginx/conf.d/default.conf

# Copy build files from the previous stage
COPY --from=builder /app/dist /usr/share/nginx/html

# Expose port 80 to the outside world
EXPOSE 80

# Run nginx in the foreground
CMD ["nginx", "-g", "daemon off;"]