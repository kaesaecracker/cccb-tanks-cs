TAG=cccb-tanks-cs

build:
	podman build tanks-backend --tag=$(TAG)-backend
	podman build tank-frontend --tag=$(TAG)-frontend
	podman build . --tag=$(TAG)

run: build
	podman run -i -p 3000:3000 localhost/$(TAG):latest

