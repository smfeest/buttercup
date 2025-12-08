# podman manifest rm smfeest/buttercup:migrations
podman manifest create smfeest/buttercup:migrations

dotnet ef migrations bundle -f -r linux-arm64 -s ../../src/Buttercup.Web/ -o ../../src/Buttercup.Web/efbundle
podman build --platform linux/arm64 --manifest smfeest/buttercup:migrations -t smfeest/buttercup:migrations-arm64 -f ./Containerfile ../../src/Buttercup.Web/
podman push smfeest/buttercup:migrations-arm64 docker://ghcr.io/smfeest/buttercup:migrations-arm64

dotnet ef migrations bundle -f -r linux-x64 -s ../../src/Buttercup.Web/ -o ../../src/Buttercup.Web/efbundle
podman build --platform linux/amd64 --manifest smfeest/buttercup:migrations -t smfeest/buttercup:migrations-x64 -f ./Containerfile ../../src/Buttercup.Web/
podman push smfeest/buttercup:migrations-x64 docker://ghcr.io/smfeest/buttercup:migrations-x64

podman manifest push smfeest/buttercup:migrations docker://ghcr.io/smfeest/buttercup:migrations
