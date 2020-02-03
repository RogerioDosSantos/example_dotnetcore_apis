# .Net Core API - Several Examples

Several Examples implemented using .Net core API

All examples uses Open API with a [Swagger](http://rogeriodossantos.github.io/Wiki/stage/swashbuckle.html) page.


## How to Compile

From this folder execute:

```shell
docker-compose -f ./build/docker-compose-build-linux.yaml build
```

## How to Run

From this folder execute:

```shell
docker-compose -f ./build/docker-compose-run-linux.yaml up -d
```

Than you can connect in the following URL:

[http://localhost:8000](http://localhost:8000)

## Examples

- **Create Certificate**: [/api/Certificate/Certificate](http://localhost:8000/api/Certificate/Certificate?certificateName=certificate_name&certificatePassword=certificate_password&expirationInYears=10)

