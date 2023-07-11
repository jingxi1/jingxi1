### add -v file in container with dev
```
 docker run --rm -it -v /home/jingxi/DockerMntFiles/dev:/
root/.vscode-server mcr.microsoft.com/dotnet/sdk:7.0.304-jammy-amd64 
```

###  nuget upload pack.
### setup harbor
```
cat /etc/docker/daemon.json 
{"log-driver":"json-file","log-opts":{"max-size":"100m"},"registry-mirrors":["https://mirror.baidubce.com"],"insecure-registries": ["http://gitlab.omnicode.cn:30888"]}
```
0. add the name of project on the harbor.
1. docker login http://gitlab.omnicode.cn:30888
2. docker tag ipxesrc:v01 gitlab.omnicode.cn:30888/ipxebootiso/ipxesrc:v1
3. docker push gitlab.omnicode.cn:30888/ipxebootiso/ipxesrc:v1

### nuget upload pack.

```

dotnet nuget push -s https://gitlab.omnicode.cn:30882/v3/index.json .\nugetConsoleApp.0.0.1.nupkg -k Developer200.

```

### cockpit

```sh
	sudo yum install cockpit -y
    sudo systemctl enable --now cockpit.socket
    sudo firewall-cmd --permanent --zone=public --add-service=cockpit
    sudo firewall-cmd --reload
```

### Remote SSh

```

ssh jingxi@gitlab.omnicode.cn -p22022

```

port scope 25000-25100/5000-5100

  

internal with jingxi@10.10.10.103 of fedora linux

  ## Docker build img
```
docker commit -a "vellhe" -m "py3.6_tf1.8_keras2.2" 00ff1b764a1b tf_keras:v1
```

## dotnet test debug output

```
dotnet test -l "console;verbosity=detailed"
```
  
  

### Git Store password

git config --global credential.helper store

git config --global user.email "jingxi@me.com"

git config --global user.name "jingxi"

  
  

### Mindmap Note

  

```mermaid

graph LR

y(core)-->A(industry)-->B2(MFG)

B2-->C1(GoCode)

B2-->C2(PCode)

```

  

### CI/CD

```

  

stages:          # List of stages for jobs, and their order of execution

  - build

  - test

  - deploy

# image: mcr.microsoft.com/dotnet/sdk:6.0.406-jammy-amd64

build-job:       # This job runs in the build stage, which runs first.

  stage: build

  script:

    - echo "Compiling the code..."

    - ls

    - dotnet restore

    - echo "Compile complete."

unit-test-job:   # This job runs in the test stage.

  stage: test    # It only starts when the job in the build stage completes successfully.

  script:

    - echo "Running unit tests... This will take about 60 seconds."

    - echo "sleep 60"

    #- dotnet test --no-build -c Release   -l  "trx;LogFileName=../../testRes.trx"

    - dotnet test   -l  "trx;LogFileName=../../testRes.trx"

    #- echo "Code coverage is 90%"

  after_script:

     - /root/.dotnet/tools/trx2junit testRes.trx

  artifacts:

    when: always

    paths: ['testRes.xml']

    expose_as: 'testing report'

    reports:

      junit: testRes.xml

  

deploy-job:      # This job runs in the deploy stage.

  stage: deploy  # It only runs when *both* jobs in the test stage complete successfully.

  script:

    - echo "Deploying application..."

    - echo "Application successfully deployed."

  

```


```yaml
  
stages:

# List of stages for jobs, and their order of execution

    - build

    - test

    - deploy

# image: mcr.microsoft.com/dotnet/sdk:6.0.406-jammy-amd64

  

build-job:

    stage: build

    script:

    - echo "Compiling the code..."

    - ls

    - dotnet restore

    - echo "Compile complete."

  

unit-test-job:

    stage: test

    script:

    - echo "Running unit tests... This will take about 60 seconds."

    - echo "sleep 60"  #- dotnet test --no-build -c Release   -l  "trx;LogFileName=../../testRes.trx"

    - dotnet test   -l  "trx;LogFileName=../../testRes.trx"

    after_script:

    - /root/.dotnet/tools/trx2junit testRes.trx

    artifacts:

        when: always

        paths: ['testRes.xml']

        expose_as: 'testing report'

        reports:

            junit: testRes.xml

  
  

deploy-job:

    stage: deploy

    script:

        - echo "Deploying application..."

        - echo "Application successfully deployed."
```