FROM node:alpine3.11 AS builder

WORKDIR /app

COPY Frontend/package*.json ./

RUN npm install

COPY Frontend/. .

RUN npm run build

EXPOSE 5173

CMD [ "npm", "run", "preview", "--", "--host", "0.0.0.0" ]