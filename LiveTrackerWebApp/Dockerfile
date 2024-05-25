FROM node:20.13-alpine

# Define build arguments for environment variables
ARG VITE_SERVER_URL
ARG VITE_TRACK_SERVER_URL
ARG VITE_RECAPTCHA_SITE_KEY

# Set environment variables during the build process
ENV VITE_SERVER_URL=http://localhost:8081
ENV VITE_TRACK_SERVER_URL=http://localhost:8091/racehub
ENV VITE_RECAPTCHA_SITE_KEY=xxx

WORKDIR /app

COPY package.json .

RUN npm install

COPY . .

EXPOSE 8070

CMD [ "npm", "run", "dev" ]