echo "Build image '$1' and 'latest'..."
docker build -f ./Dockerfile -t ghcr.io/mylab-task/runtime:$1 -t ghcr.io/mylab-task/runtime:latest ../src

echo "Publish image '$1' ..."
docker push ghcr.io/mylab-task/runtime:$1

echo "Publish image 'latest' ..."
docker push ghcr.io/mylab-task/runtime:latest
