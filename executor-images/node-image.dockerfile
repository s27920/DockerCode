FROM node:20-alpine

WORKDIR /app

COPY ./exec-scripts/exec-node.sh execute_node.sh
COPY ./scripts/stdin-receiver.sh stdin-receiver.sh
RUN chmod +x execute_node.sh && chmod +x stdin-receiver.sh

ENTRYPOINT ["/bin/sh", "-c", "./stdin-receiver.sh js && ./execute_node.sh /app/code.js"]
