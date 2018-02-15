FROM mono:4.8

RUN apt-get update && apt-get install -y git ssh tar gzip ca-certificates
RUN curl -sL https://deb.nodesource.com/setup_6.x | bash -E -
RUN apt-get install -y nodejs npm
