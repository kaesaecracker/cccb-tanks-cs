TAG=cccb-tanks-cs

build:
	podman build . --tag=$(TAG)

run: build
	podman run -i -p 3000:3000 localhost/$(TAG):latest
