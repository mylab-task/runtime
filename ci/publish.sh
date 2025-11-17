docker build --platform linux/amd64 -f ./Dockerfile -t ghcr.io/mylab-task/runtime:latest -t ghcr.io/mylab-task/runtime:$1 ..

docker push ghcr.io/mylab-task/runtime:$1
docker push ghcr.io/mylab-task/runtime:latest