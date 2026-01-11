FROM node:18-alpine

WORKDIR /app

# Install production dependencies
COPY package.json ./
RUN npm install --omit=dev

# Copy source
COPY src ./src

EXPOSE 3000

CMD ["npm", "start"]
